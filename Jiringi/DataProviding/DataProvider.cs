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
            caches = new Dictionary<int, Cache>();
        }

        private readonly RootConfigHandler setting;
        private int company_step = 0;
        private List<Step> cumulative_frequency_training,
            cumulative_frequency_validation, cumulative_frequency_evaluation;
        private SqlCommand sqlite;
        private readonly object sqlite_lock = new object();
        private readonly Dictionary<int, Cache> caches;

        public uint TrainingCount { get; private set; }
        public uint ValidationCount { get; private set; }
        public uint EvaluationCount { get; private set; }

        public void Initialize()
        {
            lock (sqlite_lock)
            {
                caches.Clear();
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
                    uint count;
                    bool is_inserted = false;
                    int instrument_id = (int)reader[0];

                    count = (uint)(int)reader[1];
                    if (count > 0)
                    {
                        cumulative_frequency_training.Add(
                            new Step(instrument_id, TrainingCount, count, true));
                        TrainingCount += count;
                        is_inserted = true;
                    }

                    count = (uint)(int)reader[2];
                    if (count > 0)
                    {
                        cumulative_frequency_validation.Add(
                            new Step(instrument_id, ValidationCount, count, false));
                        ValidationCount += count;
                        is_inserted = true;
                    }

                    count = (uint)(int)reader[3];
                    if (count > 0)
                    {
                        cumulative_frequency_evaluation.Add(
                            new Step(instrument_id, EvaluationCount, count, false));
                        EvaluationCount += count;
                        is_inserted = true;
                    }

                    if (!is_inserted) continue;

                    if (!caches.ContainsKey(instrument_id))
                        caches.Add(instrument_id, Cache.Build(
                            RECORDS_THIS_YEAR_IN_SIGNAL, YEARS_COUNT, RECORDS_PREVIOUS_ONE_YEAR));
                }

                if (cumulative_frequency_training.Count == 0)
                    throw new Exception("The data set is empty.");

                sqlite.CommandText = sql_trade;
            }
        }
        public Task<Record> PrepareNextData(uint offset, TraingingStages stage)
        {
            return Task.Run(() =>
            {
                var start_time = DateTime.Now.Ticks;
                var result = new double[RESULT_COUNT];
                var signal = new double[SIGNAL_COUNT];

                var (instrument_id, record_offset) = stage switch
                {
                    TraingingStages.Training => FindCompany(cumulative_frequency_training, offset),
                    TraingingStages.Validation => FindCompany(cumulative_frequency_validation, offset),
                    TraingingStages.Evaluation => FindCompany(cumulative_frequency_evaluation, offset),
                    _ => throw new Exception("Invalid stage type"),
                };

                var cache = caches[instrument_id];

                lock (sqlite_lock)
                    if (sqlite != null)
                    {
                        if (cache.RealDataCount > 0 &&
                            record_offset <= cache.FirstValue.Value.Offset &&
                            record_offset + ESTIMATED_TOTAL_RECORD_COUNT >= cache.FirstValue.Value.Offset)
                        {
                            var records_count = (int)(cache.FirstValue.Value.Offset - record_offset);
                            if (!cache.IsFull) records_count++;

                            FillBackward(instrument_id, record_offset, records_count);
                        }
                        else
                        {
                            cache.Clear();
                            FillForward(instrument_id, record_offset, ESTIMATED_TOTAL_RECORD_COUNT);
                        }

                        while (cache.FirstValue.Value.RecordType == null)
                            FillBackward(instrument_id, --record_offset, 1);

                        while (!cache.IsFull)
                            FillForward(instrument_id,
                                record_offset + cache.RealDataCount, ESTIMATED_TOTAL_RECORD_COUNT);
                    }

#if DEBUG
                cache.CheckOffsetSequence(record_offset);
#endif

                int r = 0, s = 0;
                var buffer = new double[CACHE_OUTPUT_COUNT];
                cache.FillBuffer(buffer, ref s);

                while (r < RESULT_COUNT)
                    result[r] = buffer[r++];

                s = 0;
                while (r < CACHE_OUTPUT_COUNT)
                    signal[s++] = buffer[r++];

                Convertor.BinaryState(instrument_id, signal, ref s);

                return new Record(signal, result, instrument_id, DateTime.Now.Ticks - start_time);
            });
        }
        private (int id, uint rec_offset) FindCompany(List<Step> cumulative_frequency, uint offset)
        {
            if (offset <= 0)
            {
                company_step = 0;
                var inst = cumulative_frequency[company_step];
                return inst.GetRecordOffset(0);
            }

            int start = 0, top = cumulative_frequency.Count;
            Step left, right;
            while (true)
            {
                left = cumulative_frequency[company_step];
                if (company_step + 1 < cumulative_frequency.Count)
                    right = cumulative_frequency[company_step + 1];
                else return left.GetRecordOffset(offset);

                if (left.start_point <= offset && offset < right.start_point)
                    return left.GetRecordOffset(offset);
                else if (offset == right.start_point)
                {
                    company_step++;
                    return right.GetRecordOffset(offset);
                }
                else if (offset > right.start_point) start = company_step + 1;
                else if (left.start_point > offset) top = company_step;
                company_step = (top + start) / 2;
            }
        }
        private void FillForward(int instrument_id, uint record_offset, int record_count)
        {
            var cache = caches[instrument_id];

            sqlite.Parameters.Clear();
            sqlite.Parameters.Add("@InstrumentID", SqlDbType.Int).Value = instrument_id;
            sqlite.Parameters.Add("@Offset", SqlDbType.Int).Value = record_offset;
            sqlite.Parameters.Add("@Count", SqlDbType.Int).Value = record_count;
            using var reader = sqlite.ExecuteReader();

            int i = 0;
            while (reader.Read())
            {
                i++;
                char? type = reader[3] is DBNull ? null : (char?)((string)reader[3])[0];
                if (cache.InjectDataToLast(
                    (uint)(long)reader[0], (DateTime)reader[1], (decimal)reader[2], type))
                    break;
            }

            if (i == 0) throw new Exception("There is not more data.");
        }
        private void FillBackward(int instrument_id, uint record_offset, int record_count)
        {
            var cache = caches[instrument_id];
            var stack = new Stack<(uint offset, DateTime date, decimal price, char? type)>();

            sqlite.Parameters.Clear();
            sqlite.Parameters.Add("@InstrumentID", SqlDbType.Int).Value = instrument_id;
            sqlite.Parameters.Add("@Offset", SqlDbType.Int).Value = record_offset;
            sqlite.Parameters.Add("@Count", SqlDbType.Int).Value = record_count;
            using var reader = sqlite.ExecuteReader();

            while (reader.Read())
            {
                char? type = reader[3] is DBNull ? null : (char?)((string)reader[3])[0];
                stack.Push(((uint)(long)reader[0], (DateTime)reader[1], (decimal)reader[2], type));
            }

            if (stack.Count < 1) throw new Exception("There is not more data.");

            while (stack.Count > 0)
            {
                var (offset, date, price, type) = stack.Pop();
                cache.InjectDataToFirst(offset, date, price, type);
                // DO NOT BREAK. we should go to criterion offset
            }
        }

        public void Dispose()
        {
            if (sqlite != null)
                lock (sqlite_lock)
                {
                    caches.Clear();
                    var connection = sqlite.Connection;
                    sqlite.Dispose();
                    connection?.Dispose();
                    sqlite = null;
                }
        }


        #region SQL Queries
        public const int YEARS_COUNT = 3;
        public const int RESULT_COUNT = 20;
        public const int RECORDS_THIS_YEAR_IN_SIGNAL = 183;
        public const int RECORDS_PREVIOUS_ONE_YEAR = 60;
        public const int INSTRUNMENT_ID = 32;
        public const int ESTIMATED_TOTAL_RECORD_COUNT = 240 * (YEARS_COUNT + 1);
        public static readonly int RECORDS_PREVIOUS_YEARS_TOTAL;
        public static readonly int SIGNAL_COUNT;
        public static readonly int CACHE_OUTPUT_COUNT;

        static DataProvider()
        {
            RECORDS_PREVIOUS_YEARS_TOTAL = 0;
            for (int y = 1; y <= YEARS_COUNT;)
                RECORDS_PREVIOUS_YEARS_TOTAL += (int)Math.Ceiling(RECORDS_PREVIOUS_ONE_YEAR / (double)(y++));

            SIGNAL_COUNT = RECORDS_THIS_YEAR_IN_SIGNAL + RECORDS_PREVIOUS_YEARS_TOTAL + INSTRUNMENT_ID;
            CACHE_OUTPUT_COUNT = RESULT_COUNT + RECORDS_THIS_YEAR_IN_SIGNAL + RECORDS_PREVIOUS_YEARS_TOTAL;
        }

        private readonly static string sql_counting = $@"
select		InstrumentID,
			iif(TrainingCount > 0, TrainingCount, 0) as TrainingCount,
			iif(ValidationCount > 0, ValidationCount, 0) as ValidationCount,
			iif(EvaluationCount > 0, EvaluationCount, 0) as EvaluationCount
from (
	select		InstrumentID,
				sum(iif(RecordType = 'T', 1, 0)) as TrainingCount,
				sum(iif(RecordType = 'V', 1, 0)) as ValidationCount,
				sum(iif(RecordType = 'E', 1, 0)) as EvaluationCount
	from		Trade
	where		RecordType is not null
	group by	InstrumentID
	having		sum(iif(RecordType = 'T', 1, 0)) > 0
			or	sum(iif(RecordType = 'V', 1, 0)) > 0
			or	sum(iif(RecordType = 'E', 1, 0)) > 0
) ins_q
order by	InstrumentID";

        private readonly static string sql_trade = $@"
select row_number() over(order by DateTimeEn desc) - 1 as Offset
     , DateTimeEn, ClosePrice, RecordType
from Trade where InstrumentID = @InstrumentID
order by DateTimeEn desc offset @Offset rows fetch first @Count rows only";
        #endregion

    }
}
