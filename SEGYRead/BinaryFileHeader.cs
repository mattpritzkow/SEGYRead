using System;
using System.Diagnostics;
using System.Linq;

//Refer all variable names to SEGY documentation

namespace SEGYRead
{
    public class BinaryFileHeader
    {
        public ushort numDataTraces;       //Bytes 3213-3214, tells the number of Data traces (generally 12-24)
        private ushort numAuxTraces;        //Bytes 3215-3216, tells number of auxiliary traces

        private ushort sampIntervalMicro;   //Bytes 3217-3218, tells number of sample intervals in microseconds (μs)
        public ushort numSampsPerTrace;    //Bytes 3221-3222, tells number of samples per trace (primary set of seismic data traces with sampIntervalMicro as well)

        private ushort dataSampFormat;      //Bytes 3225-3226, tells if numbers are in  two's compliment, IBM floating point, etc (8 cases, see checkDataSampFormat() for all)
        private ushort expectedNumDataTraces;   //Bytes 3227-3228, tells the expected num of dataTraces, highly recommended to use but some files don't have it
        private ushort traceSorting;        //Bytes 3229-3230, checks to see how expectedNumDataTraces is formatted (9 cases, see checkTraceSorting() for all)

        public ushort measurementUnit;     //Bytes 3255-3256, tells whether the measurement units are in meters of feet

        private ushort formatRevisionNum;   //Bytes 3501-3502, tells if the SEGY file is in the Revision 1.0 form or if it's from the 1975 standard
        private ushort fixedLengthFlag;     //Bytes 3503-3504

        //this determines how many optional 3200 Byte Extended Textual File Header records there are
        private ushort extendedTextualFileNum;      //Bytes 3505-3506

        public int extendedTextualFileHeaderEnd;

        //this function converts a series of bits from little endian to big endian
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

        public ushort getNumDataTraces()
        {
            return numDataTraces;
        }

        public void readBinaryFileHeader(Byte[] buffer, string filePath)
        {
            if (buffer.Count() <= 0)
            {
                Debug.WriteLine("Buffer is empty.");
                return;
            }

            this.numDataTraces = BitConverter.ToUInt16(buffer, 3213);
            this.numAuxTraces = BitConverter.ToUInt16(buffer, 3215);

            this.sampIntervalMicro = BitConverter.ToUInt16(buffer, 3217);
            this.numSampsPerTrace = BitConverter.ToUInt16(buffer, 3221);

            //this line successfully runs flipBits()'
            this.numSampsPerTrace = BitConverter.ToUInt16(flipBits(buffer, 3220, 3221), 0);

            this.dataSampFormat = BitConverter.ToUInt16(buffer, 3225);
            this.expectedNumDataTraces = BitConverter.ToUInt16(buffer, 3227);
            this.traceSorting = BitConverter.ToUInt16(buffer, 3229);

            this.measurementUnit = BitConverter.ToUInt16(buffer, 3255);

            this.formatRevisionNum = BitConverter.ToUInt16(buffer, 3501);
            this.fixedLengthFlag = BitConverter.ToUInt16(buffer, 3503);

            this.extendedTextualFileNum = BitConverter.ToUInt16(buffer, 3505);

            Debug.WriteLine("Num of Data Traces: " + numDataTraces);
            Debug.WriteLine("Num of Aux Traces: " + numAuxTraces);
            Debug.WriteLine("Samp Interval Micro: " + sampIntervalMicro);
            Debug.WriteLine("Num of Samps Per Trace: " + (numSampsPerTrace));

            Debug.WriteLine("Data Samp Format: " + dataSampFormat);
            checkDataSampFormat(dataSampFormat);

            Debug.WriteLine("Expected Num of DataTraces: " + expectedNumDataTraces);
            Debug.WriteLine("Trace Sorting: " + traceSorting);
            checkTraceSorting(traceSorting);

            Debug.WriteLine("Measurement Units: " + measurementUnit);
            Debug.WriteLine("-------------MEASUREMENT UNITS CHECK--------------------");
            if (measurementUnit == 1)
                Debug.WriteLine("Measurement Units are in meters.");
            else if (measurementUnit == 2)
                Debug.WriteLine("Measurement Units are in feet.");
            Debug.WriteLine("-------------END OF MEASUREMENT UNITS CHECK--------------------");

            Debug.WriteLine("Format Revision Num: " + formatRevisionNum);
            Debug.WriteLine("-------------FORMAT REVISION NUM CHECK--------------------");
            if (formatRevisionNum == 0)
                Debug.WriteLine("File is in traditional 1975 standard SEGY format.");
            else
                Debug.WriteLine("File is in SEGY Revision 1.0 format");
            Debug.WriteLine("-------------END OF FORMAT REVISION NUM CHECK--------------------");

            Debug.WriteLine("Fixed Length Flag: " + fixedLengthFlag);
            Debug.WriteLine("-------------FIXED LENGTH FLAG CHECK--------------------");
            if (fixedLengthFlag > 0)
                Debug.WriteLine("All traces in the SEGY file will have the same sample interval and num of samples.");
            else if (fixedLengthFlag == 0)
                Debug.WriteLine("Length of traces in file may vary and num of samples in bytes 115-116 must be examined to determine the length of each trace.");
            Debug.WriteLine("-------------END OF FIXED LENGTH FLAG CHECK--------------------");

            Debug.WriteLine("Extended Textual File Num: " + extendedTextualFileNum);
            Debug.WriteLine("-------------EXTENDED TEXTUAL FILE NUM CHECK--------------------");
            if (extendedTextualFileNum < 0)
            {
                Debug.WriteLine("Exactly " + extendedTextualFileNum + ". Variable amount detected");         //variable number of Extended Textual File Header records
                Debug.WriteLine("extendedTextualFileHeaderEnd not set. ERROR");
            }
            else if (extendedTextualFileNum > 0)
            {
                extendedTextualFileHeaderEnd = (extendedTextualFileHeaderEnd * 3200) + 3600;        //adding however many extendedTextFileHeaders (3200 bits each) to the end of binary file header (ends @ 3600)
                Debug.WriteLine("Exactly " + extendedTextualFileNum + " Extended Textual File Header records.");
            }
            else if (extendedTextualFileNum == 0)
            {
                extendedTextualFileHeaderEnd = 3600;
                Debug.WriteLine("There are no Extended Textual Files THANK YOU");
            }

            Debug.WriteLine("-------------END OF EXTENDED TEXTUAL FILE NUM CHECK--------------------");
        }

        public void checkDataSampFormat(ushort dataSamp)
        {
            Debug.WriteLine("-----------DATA SAMP FORMAT CHECK------------");
            switch (dataSamp)
            {
                case 1:
                    Debug.WriteLine("1 = 4-byte IBM floating-point.");
                    break;
                case 2:
                    Debug.WriteLine("2 = 4 - byte, two's complement integer ");
                    break;
                case 3:
                    Debug.WriteLine("3 = 2-byte, two's complement integer ");
                    break;
                case 4:
                    Debug.WriteLine("4 = 4-byte fixed-point with gain (obsolete)");
                    break;
                case 5:
                    Debug.WriteLine("5 = 4-byte IEEE floating-point");
                    break;
                case 6:
                    Debug.WriteLine("6 = Not currently used");
                    break;
                case 7:
                    Debug.WriteLine("7 = Not currently used");
                    break;
                case 8:
                    Debug.WriteLine("8 = 1-byte, two's complement integer");
                    break;
                default:
                    Debug.WriteLine("Default case hit.");
                    return;
            }
            Debug.WriteLine("-----------END OF DATA SAMP FORMAT CHECK------------");
        }

        public void checkTraceSorting(ushort traceSort)
        {
            Debug.WriteLine("-------------TRACE SORTING CHECK------------- ");
            if (traceSort < 0)
                Debug.WriteLine("-1 = Other(should be explained in user Extended Textual File Header stanza");
            switch (traceSort)
            {
                case 0:
                    Debug.WriteLine("0 = Unknown");
                    break;
                case 1:
                    Debug.WriteLine("1 = As recorded (no sorting)");
                    break;
                case 2:
                    Debug.WriteLine("2 = CDP ensemble");
                    break;
                case 3:
                    Debug.WriteLine("3 = Single fold continuous profile");
                    break;
                case 4:
                    Debug.WriteLine("4 = Horizontally stacked ");
                    break;
                case 5:
                    Debug.WriteLine("5 = Common source point");
                    break;
                case 6:
                    Debug.WriteLine("6 = Common receiver point");
                    break;
                case 7:
                    Debug.WriteLine("7 = Common offset point");
                    break;
                case 8:
                    Debug.WriteLine("8 = Common mid-point");
                    break;
                case 9:
                    Debug.WriteLine("9 = Common conversion point");
                    break;
                default:
                    Debug.WriteLine("Default case.");
                    break;
            }
            Debug.WriteLine("-------------END OF TRACE SORTING CHECK------------- ");
        }
    }
}