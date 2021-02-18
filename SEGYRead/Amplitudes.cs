using System;
using System.Collections.Generic;

namespace SEGYRead
{
    public abstract class Amplitudes
    {
        public abstract IList<float> ReadData(Byte[] buffer, int start, int end);
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
    }
}
