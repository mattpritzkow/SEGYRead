using System;
using System.Collections.Generic;

namespace SEGYRead
{
    class Amp4ByteIBMFloating : Amplitudes
    {
        public override IList<float> ReadData(Byte[] buffer, int start, int end)
        {
            IList<float> retList = new List<float>();
            for (int i = start - 1; i < end - 4; i += 4)
            {
                float val = (float)BitConverter.ToSingle(flipBits(buffer, i, i + 4), 0);
                retList.Add(val);
            }
            return retList;
        }
    }
}
