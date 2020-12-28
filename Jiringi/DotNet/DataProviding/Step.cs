using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataProviding
{
    class Step
    {
        public readonly uint start_point, offset_mapping;
        public readonly int instrument;
        public readonly bool is_training;

        public Step(int instrument, uint start_point, uint count, bool is_training)
        {
            this.start_point = start_point;
            this.instrument = instrument;
            this.is_training = is_training;

            offset_mapping = count + start_point - 1;
        }

        public (int instrument_id, uint record_offset) GetRecordOffset(uint offset)
        {
            return (instrument, offset_mapping - offset);
        }

        public override string ToString()
        {
            return $"{instrument} from:{start_point} map:{offset_mapping} type:{is_training}";
        }
    }
}
