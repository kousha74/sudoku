using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku.BusinessLogic
{
    class Bits
    {
        static public readonly ushort BIT0 = 0x0001;
        static public readonly ushort BIT1 = 0x0002;
        static public readonly ushort BIT2 = 0x0004;
        static public readonly ushort BIT3 = 0x0008;
        static public readonly ushort BIT4 = 0x0010;
        static public readonly ushort BIT5 = 0x0020;
        static public readonly ushort BIT6 = 0x0040;
        static public readonly ushort BIT7 = 0x0080;
        static public readonly ushort BIT8 = 0x0100;
        static public readonly ushort[] BITS = { BIT0, BIT1, BIT2, BIT3, BIT4, BIT5, BIT6, BIT7, BIT8 };
        static public int countSetBits(ushort n)
        {
            int count = 0;
            while (n > 0)
            {
                count += n & 1;
                n >>= 1;
            }
            return count;
        }

        static public int firstSetBits(ushort n)
        {
            int counter = 0;
            while (n > 0)
            {
                if((n & 1) != 0)
                {
                    return counter;
                }
                n >>= 1;
                counter++;
            }

            return -1;
        }

        static public List<int> SetBits(ushort n)
        {
            int counter = 0;
            List<int> setBits = new List<int>();
            while (n > 0)
            {
                if((n & 1) != 0)
                {
                    setBits.Add(counter);
                }
                n >>= 1;
                counter++;
            }

            return setBits;
        }

        private Bits()
        {

        }
    }
}
