using System;
using System.Collections.Generic;
using System.Text;

// Author : Mustafa K. GILOR (PENTAGRAM)

namespace KOF.Core
{
    class PasswordHasher
    {
        private static readonly uint[] _encodingArray =
        {
            0x1a,
            0x1f, 0x11, 0x0a, 0x1e,
            0x10, 0x18, 0x02, 0x1d,
            0x08, 0x14, 0x0f, 0x1c,
            0x0b, 0x0d, 0x04, 0x13,
            0x17, 0x00, 0x0c, 0x0e,
            0x1b, 0x06, 0x12, 0x15,
            0x03, 0x09, 0x07, 0x16,
            0x01, 0x19, 0x05, 0x12,
            0x1d, 0x07, 0x19, 0x0f,
            0x1f, 0x16, 0x1b, 0x09,
            0x1a, 0x03, 0x0d, 0x13,
            0x0e, 0x14, 0x0b, 0x05,
            0x02, 0x17, 0x10, 0x0a,
            0x18, 0x1c, 0x11, 0x06,
            0x1e, 0x00, 0x15, 0x0c,
            0x08, 0x04, 0x01
        };

        private static readonly byte[] _alphabetArray =
        {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d,
            0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a
        };

        public static string HashPasswordString(string inputPassword)
        {

            const uint startKey = 0x03e8;

            var stringByteList = new List<byte>(Encoding.UTF8.GetBytes(inputPassword));
            while (stringByteList.Count % 4 != 0)
            {
                stringByteList.Add(0);
            }
            byte[] stringBytes = stringByteList.ToArray();
            int counter = 0;
            uint tmp = 0;
            uint inputKey = 0;
            uint outHash = 0;
            var outStringBytes = new List<byte>();
            for (int i = 0; i < stringBytes.Length; i += 4)
            {
                uint encoded = BitConverter.ToUInt32(stringBytes, i);
                byte bl = 0x01; //even/odd thing?

                tmp = encoded + startKey; //input
                inputKey = tmp;
                counter = 0;
                outHash = 0;
                do
                {
                    tmp = inputKey;

                    inputKey = inputKey >> 1;
                    if (tmp % 2 != 0)
                    {
                        tmp = bl == 0 ? _encodingArray[(counter / 4) + 32] : _encodingArray[counter / 4];
                        outHash += (uint)1 << (int)tmp;
                    }
                    counter += 4;
                } while (inputKey > 0);

                long tmpPut = outHash;
                for (int tmpInt = 0; tmpInt < 7; tmpInt++)
                {
                    long tmpProduct = tmpPut * 0x38e38e39;
                    var upper = (int)(tmpProduct >> 35);
                    var anotherTmp = (uint)((upper * 8) + upper);
                    anotherTmp <<= 2;
                    var difference = (uint)(tmpPut - anotherTmp);
                    outStringBytes.Add(_alphabetArray[difference]);
                    tmpPut = upper;
                }
            }

            return Encoding.UTF8.GetString(outStringBytes.ToArray());
        }
    }
}
