using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataProviding
{
    class Step
    {
        public readonly uint start_point;
        public readonly int instrument;
        public readonly bool is_training;

        public Step(uint start_point, int instrument, bool is_training)
        {
            this.start_point = start_point;
            this.instrument = instrument;
            this.is_training = is_training;
        }

        public override string ToString()
        {
            return $"{instrument} from {start_point} t:{is_training}";
        }
    }
}
