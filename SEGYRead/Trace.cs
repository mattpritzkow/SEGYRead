using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SEGYRead
{
    public class Trace
    {
        private uint traceSeqNumLine;   //Bytes 1-4, trace sequence number within line (nums continue to increase if same line continues across multiple segy files)
        private uint ogFieldRecordNum;  //Bytes 9-12, original field record number
        private uint traceNumOGField;   //Bytes 13-16, trace number within the original field record

        private ushort traceIDCode;     //Bytes 29-30, trace identification code

        private ushort numSampsTrace;   //Bytes 115-116, number of samples in this trace
        private ushort sampIntMicro;    //Bytes 117-118, sample interval in microseconds for this trace

        public IList<float> amplitudes { get; set; }

        public Trace()
        {
            amplitudes = new List<float>();
        }

        public Byte[] flipBits(Byte[] buffer, int startIndex, int endIndex)
        {
            //algorithm below in theory should be able to work on any amount of bytes
            int difference = (endIndex - startIndex);       //take difference of endIndex - startIndex (remember, we are using this to declare size, not assign values to locations in the temp array)
            Byte[] flipped = new byte[difference + 1];      //create a temp array the size of the difference between the start & end indexes, add 1 to the total of the difference

            for (int i = 0; i <= (difference); i++)      //for loop that has to be less than the difference calculated earlier
            {
                flipped[i] = buffer[(endIndex - i)];        //still want to access the buffer[] array at the correct index, flipping the buffer[endIndex] values to the beginnning of the flipped[i] values
            }

            return flipped;
        } // end of flipBits()

        public void readTraceHeaders(Byte[] buffer, BinaryFileHeader temp)
        {
            if (buffer.Count() <= 0)
            {
                Debug.WriteLine("Buffer is empty.");
                return;
            }

            //Debug.WriteLine("Beginning Bit Location of Trace Header 1: " + temp.extendedTextualFileHeaderEnd);

            this.traceSeqNumLine = BitConverter.ToUInt32(buffer, temp.extendedTextualFileHeaderEnd);
            this.ogFieldRecordNum = BitConverter.ToUInt32(buffer, temp.extendedTextualFileHeaderEnd + 9);
            this.traceNumOGField = BitConverter.ToUInt32(buffer, temp.extendedTextualFileHeaderEnd + 13);

            this.traceIDCode = BitConverter.ToUInt16(buffer, temp.extendedTextualFileHeaderEnd + 29);

            this.numSampsTrace = BitConverter.ToUInt16(buffer, temp.extendedTextualFileHeaderEnd + 115);
            this.sampIntMicro = BitConverter.ToUInt16(buffer, temp.extendedTextualFileHeaderEnd + 117);

            Debug.WriteLine("*--------------------START OF ANALYSIS OF TRACE HEADER--------------------*");

            Debug.WriteLine("Trace sequence number within line: " + traceSeqNumLine);
            Debug.WriteLine("Original field record number: " + ogFieldRecordNum);
            Debug.WriteLine("Trace number within the original field record: " + traceNumOGField);

            Debug.WriteLine("Trace ID Code: " + traceIDCode);
            if (traceIDCode == 1)
                Debug.WriteLine("Trace ID says that this file contains seismic data");
            else
                Debug.WriteLine("This file doesn't contain seismic data according to the trace ID code.");

            Debug.WriteLine("*--------------------END OF ANALYSIS OF TRACE HEADER--------------------*");
        }

        public void determineTraceData(Byte[] buffer, int start, int stop, BinaryFileHeader binTemp)
        {
            Amplitudes amplitudeReader = null;
            switch (binTemp.measurementUnit)
            {
                case 0x1:
                    amplitudeReader = new Amp4ByteIBMFloating();
                    break;
                case 0x2:
                    amplitudeReader = new Amp4Byte2sComp();
                    break;
                case 0x3:
                    amplitudeReader = new Amp2Byte2sComp();
                    break;
                case 0x4:
                    Debug.WriteLine("OBSOLETE FIXED POINT WITH GAIN FORMAT. NOT SUPPORTED");
                    break;
                case 0x5:
                    amplitudeReader = new Amp4ByteIEEEFloating();
                    break;
                case 0x6:
                    Debug.WriteLine("NOT CURRENTLY USED");
                    break;
                case 0x7:
                    Debug.WriteLine("NOT CURRENTLY USED");
                    break;
                case 0x8:
                    amplitudeReader = new Amp1Byte2sComp();
                    break;
            }

            amplitudes = amplitudeReader.ReadData(buffer, start, stop);     //calls the Abstract class Amplitudes() that will determine how to read the Trace amplitudes
            Debug.WriteLine("List Size: " + amplitudes.Count());
        }

    }
}
