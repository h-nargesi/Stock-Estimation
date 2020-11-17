using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Photon.Jiringi.Config;
using Photon.Jiringi.DataCaching;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi.DataProviding
{
    class DataProvider : IDataProvider, IDisposable
    {
        public DataProvider(RootConfigHandler setting)
        {
            this.setting = setting ?? throw new ArgumentNullException(nameof(setting));
        }

        private readonly RootConfigHandler setting;
        private int company_step = 0;
        private List<Step> cumulative_frequency_training,
            cumulative_frequency_validation, cumulative_frequency_evaluation;
        private SqlCommand sqlite;
        private readonly object sqlite_lock = new object();
        private Cache cache;

        public uint TrainingCount { get; private set; }
        public uint ValidationCount { get; private set; }
        public uint EvaluationCount { get; private set; }

        public void Initialize()
        {
            lock (sqlite_lock)
            {
                sqlite = new SqlCommand
                {
                    Connection = new SqlConnection(setting.DataProvider)
                };
                sqlite.Connection.Open();

                sqlite.CommandText = sql_counting;
                using var reader = sqlite.ExecuteReader();

                TrainingCount = 0;
                cumulative_frequency_training = new List<Step>();
                ValidationCount = 0;
                cumulative_frequency_validation = new List<Step>();
                EvaluationCount = 0;
                cumulative_frequency_evaluation = new List<Step>();

                while (reader.Read())
                {
                    if ((uint)(int)reader[1] > 0)
                    {
                        cumulative_frequency_training.Add(new Step(TrainingCount, (int)reader[0], true));
                        TrainingCount += (uint)(int)reader[1];
                    }

                    if ((uint)(int)reader[2] > 0)
                    {
                        cumulative_frequency_validation.Add(new Step(ValidationCount, (int)reader[0], false));
                        ValidationCount += (uint)(int)reader[2];
                    }

                    if ((uint)(int)reader[3] > 0)
                    {
                        cumulative_frequency_evaluation.Add(
                            new Step(EvaluationCount, (int)reader[0], false));
                        EvaluationCount += (uint)(int)reader[3];
                    }
                }

                if (cumulative_frequency_training.Count == 0)
                    throw new Exception("The data set is empty.");

                sqlite.CommandText = "GetTrade";
                sqlite.CommandType = CommandType.StoredProcedure;
            }

            cache = Cache.Build(SIGNAL_STEP_COUNT, YEARS_COUNT);
        }
        public Task<Record> PrepareNextData(uint offset, TraingingStages stage)
        {
            return Task.Run(() =>
            {
                var start_time = DateTime.Now.Ticks;
                var result = new double[RESULT_COUNT];
                var signal = new double[SIGNAL_COUNT_TOTAL];

                int r = 0, s = 0, company_id;
                (offset, company_id) = stage switch
                {
                    TraingingStages.Training => FindCompany(cumulative_frequency_training, offset),
                    TraingingStages.Validation => FindCompany(cumulative_frequency_validation, offset),
                    TraingingStages.Evaluation => FindCompany(cumulative_frequency_evaluation, offset),
                    _ => throw new Exception("Invalid stage type"),
                };

                lock (sqlite_lock)
                    if (sqlite != null)
                    {
                        sqlite.Parameters.Clear();
                        sqlite.Parameters.Add("@ID", SqlDbType.Int).Value = company_id;
                        sqlite.Parameters.Add("@Type", SqlDbType.Char, 1).Value = stage.ToString()[0];
                        sqlite.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
                        using var reader = sqlite.ExecuteReader();
                        while (reader.Read())
                        {
                            if (r < RESULT_COUNT) result[r++] = (double)(decimal)reader[0];
                            else if (s < SIGNAL_COUNT_TOTAL) signal[s++] = (double)(decimal)reader[0];
                            else break;
                        }
                    }

                if (s <= SIGNAL_COUNT_TOTAL - INSTRUNMENT_ID)
                    Convertor.BinaryState(company_id, signal, ref s);

                if (r < RESULT_COUNT || s < SIGNAL_COUNT_TOTAL)
                    throw new Exception($"Invalid data size ({offset}).");

                return new Record(signal, result, company_id, DateTime.Now.Ticks - start_time);
            });
        }
        private (uint offset, int instrument_id) FindCompany(List<Step> cumulative_frequency, uint offset)
        {
            if (offset <= 0)
            {
                company_step = 0;
                var inst = cumulative_frequency[company_step];
                return (inst.start_point, inst.instrument);
            }

            int start = 0, top = cumulative_frequency.Count;
            Step left, right;
            while (true)
            {
                left = cumulative_frequency[company_step];
                if (company_step + 1 < cumulative_frequency.Count)
                    right = cumulative_frequency[company_step + 1];
                else return (offset - left.start_point, left.instrument);

                if (left.start_point <= offset && offset < right.start_point)
                    return (offset - left.start_point, left.instrument);
                else if (offset == right.start_point)
                {
                    company_step++;
                    return (offset - right.start_point, right.instrument);
                }
                else if (offset > right.start_point) start = company_step + 1;
                else if (left.start_point > offset) top = company_step;
                company_step = (top + start) / 2;
            }
        }

        public void Dispose()
        {
            if (sqlite != null)
                lock (sqlite_lock)
                {
                    var connection = sqlite.Connection;
                    sqlite.Dispose();
                    connection?.Dispose();
                    sqlite = null;
                }
        }


        #region SQL Queries
        public const int YEARS_COUNT = 3;
        public const int RESULT_COUNT = 20;
        public const int SIGNAL_STEP_COUNT = 183;
        public const int SIGNAL_STEP_LAST_YEARS = 60;
        public const int INSTRUNMENT_ID = 32;
        public static readonly int SIGNAL_COUNT_BASICAL;
        public static readonly int SIGNAL_COUNT_LAST_YEARS;
        public static readonly int SIGNAL_COUNT_TOTAL;

        static DataProvider()
        {
            SIGNAL_COUNT_BASICAL = SIGNAL_STEP_COUNT;/* +
                (int)Math.Ceiling(SIGNAL_STEP_COUNT / 2.0) +
                (int)Math.Ceiling(SIGNAL_STEP_COUNT / 3.0) +
                (int)Math.Ceiling(SIGNAL_STEP_COUNT / 4.0);*/

            SIGNAL_COUNT_LAST_YEARS = 0;
            for (int y = 1; y <= YEARS_COUNT;)
                SIGNAL_COUNT_LAST_YEARS += (int)Math.Ceiling(SIGNAL_STEP_LAST_YEARS / (double)(y++));
            SIGNAL_COUNT_TOTAL = SIGNAL_COUNT_BASICAL + SIGNAL_COUNT_LAST_YEARS + INSTRUNMENT_ID;
        }

        private readonly static string sql_counting = $@"
SELECT		InstrumentID,
			iif(TrainingCount > 0, TrainingCount, 0) as TrainingCount,
			iif(ValidationCount > 0, ValidationCount, 0) as ValidationCount,
			iif(EvaluationCount > 0, EvaluationCount, 0) as EvaluationCount
from (
	select		InstrumentID,
				sum(iif(RecordType = 'T', 1, 0)) - {RESULT_COUNT} as TrainingCount,
				sum(iif(RecordType = 'V', 1, 0)) - {RESULT_COUNT} as ValidationCount,
				sum(iif(RecordType = 'E', 1, 0)) - {RESULT_COUNT} as EvaluationCount
	from		Trade
	where		RecordType is not null
	group by	InstrumentID
	having		sum(iif(RecordType = 'T', 1, 0)) > {RESULT_COUNT}
			or	sum(iif(RecordType = 'V', 1, 0)) > {RESULT_COUNT}
			or	sum(iif(RecordType = 'E', 1, 0)) > {RESULT_COUNT}
) ins_q
order by	InstrumentID";
        #endregion

    }
}
