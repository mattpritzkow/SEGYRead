using System;
using System.IO;
using System.Collections.Generic;

namespace SEGYRead
{
    public class SEGYU
    {
        public BinaryFileHeader binaryFileHeader { get; set; }
        public Trace trace { get; set; }

        private int start = 0;
        private int stop = 0;

        public SEGYU()
        {
            binaryFileHeader = new BinaryFileHeader();
            trace = new Trace();
        }

        public ushort getNumDataTraces()
        {
            return binaryFileHeader.numDataTraces;
        }

        public ushort getNumSampsPerTrace()
        {
            return binaryFileHeader.numSampsPerTrace;
        }

        public void readSEGY(string filePath, IList<Trace> storeList)
        {
            Byte[] buffer = File.ReadAllBytes(filePath);

            binaryFileHeader.readBinaryFileHeader(buffer, filePath);
            trace.readTraceHeaders(buffer, binaryFileHeader);
            readTraceData(buffer, storeList, binaryFileHeader);

            //Debug.WriteLine("SIZE OF STORELIST: " + storeList.Count);
        }

        public void readTraceData(Byte[] buffer, IList<Trace> traceList, BinaryFileHeader binTemp)
        {
            //for loop that runs the amount of channels in the file
            for (int i = 0; i < binTemp.numDataTraces; i++)
            {
                Trace temp = new Trace();           //instantiates a new class every time there's a new channel
                if (i == 0)
                {
                    //240 is the size of a trace header, only read one trace header then skip over the rest
                    start = (binTemp.extendedTextualFileHeaderEnd + (240));
                    stop = ((binTemp.extendedTextualFileHeaderEnd + (240)) + (binTemp.numSampsPerTrace * 4));
                    //Debug.WriteLine("START INDEX: " + start);
                    //Debug.WriteLine("END INDEX: " + stop);
                    temp.determineTraceData(buffer, start, stop, binTemp);

                    traceList.Add(temp);
                }
                else
                {
                    start = stop + 240;
                    stop = start + ((binTemp.numSampsPerTrace * 4));
                    //Debug.WriteLine("START INDEX: " + start);
                    //Debug.WriteLine("END INDEX: " + stop);

                    temp.determineTraceData(buffer, start, stop, binTemp);
                    traceList.Add(temp);
                }
            }
        }
    }
}
