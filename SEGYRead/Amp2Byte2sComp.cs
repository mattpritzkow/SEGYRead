using System;
using System.Collections.Generic;

namespace SEGYRead
{
    class Amp2Byte2sComp : Amplitudes
    {
        public override IList<float> ReadData(Byte[] buffer, int start, int end)
        {
            IList<float> retList = new List<float>();
            for (int i = start - 1; i < end - 2; i += 2)
            {
                float val = (float)BitConverter.ToSingle(flipBits(buffer, i, i + 2), 0);
                retList.Add(val);
            }
            return retList;
        }
    }
}
