using System;
using System.Collections.Generic;

namespace SEGYRead
{
    class Amp1Byte2sComp : Amplitudes
    {
        public override IList<float> ReadData(Byte[] buffer, int start, int end)
        {
            IList<float> retList = new List<float>();
            for (int i = start - 1; i < end - 1; i++)
            {
                float val = (float)BitConverter.ToSingle(flipBits(buffer, i, i++), 0);
                retList.Add(val);
            }
            return retList;
        }
    }
}
