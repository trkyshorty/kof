using System;
using System.Text;
using KOF.Common.Win32;
using System.IO;
using System.Diagnostics;

namespace KOF.Core
{
    public class Helper : Win32Api
    {
        public byte[] ReadByteArray(IntPtr Handle, int address, int length)
        {
            var Buffer = new byte[length];
            ReadProcessMemory(Handle, new IntPtr(address), Buffer, length, 0);
            return Buffer;
        }

        public Int32 Read4Byte(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[4];
            ReadProcessMemory(Handle, Address, Buffer, 4, 0);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public Int32 Read4Byte(IntPtr Handle, long Address)
        {
            return Read4Byte(Handle, new IntPtr(Address));
        }

        public Int16 ReadByte(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[2];
            ReadProcessMemory(Handle, Address, Buffer, 1, 0);
            return BitConverter.ToInt16(Buffer, 0);
        }

        public Single ReadFloat(IntPtr Handle, IntPtr Address)
        {
            byte[] Buffer = new byte[4];
            ReadProcessMemory(Handle, Address, Buffer, 4, 0);
            return BitConverter.ToSingle(Buffer, 0);
        }

        public String ReadString(IntPtr Handle, IntPtr Address, Int32 Size)
        {
            byte[] Buffer = new byte[Size];
            ReadProcessMemory(Handle, Address, Buffer, Size, 0);
            return Encoding.Default.GetString(Buffer);
        }

        public String ReadString(IntPtr Handle, long Address, Int32 Size)
        {
            return ReadString(Handle, new IntPtr(Address), Size);
        }

        public void WriteFloat(IntPtr Handle, IntPtr Address, float Value)
        {
            WriteProcessMemory(Handle, Address, BitConverter.GetBytes(Value), 4, 0);
        }

        public void Write4Byte(IntPtr Handle, IntPtr Address, Int32 Value)
        {
            WriteProcessMemory(Handle, Address, BitConverter.GetBytes(Value), 4, 0);
        }

        public void WriteByte(IntPtr Handle, IntPtr Address, Int32 Value)
        {
            WriteProcessMemory(Handle, Address, BitConverter.GetBytes(Value), 1, 0);
        }

        public void Patch(IntPtr Handle, IntPtr Addr, String Code)
        {
            byte[] PatchByte = StringToByte(Code.ToUpper());
            WriteProcessMemory(Handle, Addr, PatchByte, PatchByte.Length, 0);
        }

        public int FindAddress(IntPtr Handle, string hexAddress, int start, int length)
        {
            if (string.IsNullOrEmpty(hexAddress) || start <= 0 || length <= 0)
                return 0;

            var hexAddresses = StringToByte(hexAddress);

            for (int k = start; k < (start + length); k += 0x1000)
            {
                var addresses = ReadByteArray(Handle, k, 0x1000);

                for (int i = 0; i < addresses.Length; i++)
                {

                    if (addresses[i] == hexAddresses[0])
                    {
                        var matchAddress = true;

                        for (int j = 0; j < hexAddresses.Length; j++)
                        {
                            var key = hexAddress.Substring(j * 2, 2);

                            if (key != "XX" && (i + j >= addresses.Length || addresses[i + j] != hexAddresses[j]))
                            {
                                matchAddress = false;
                                break;
                            }
                        }

                        if (matchAddress)
                            return k + i;
                    }
                }
            }
            return 0;
        }

        public long AddressDistance(IntPtr Address, IntPtr TargetAddress)
        {
            return AddressDistance(Address.ToInt32(), TargetAddress.ToInt32());
        }

        public long AddressDistance(long Address, long TargetAddress)
        {
            long Diff = Address - TargetAddress;

            if(Diff > 0)
                return (0xFFFFFFFB - Diff);
            else
                return TargetAddress - Address - 5;
        }

        public String AlignDWORD(IntPtr Value)
        {
            return AlignDWORD(Value.ToInt32());
        }

        public String AlignDWORD(long Value)
        {
            String ADpStr, ADpStr2, ADresultStr;

            ADpStr = Convert.ToString(Value, 16);
            ADpStr2 = "";

            Int32 ADpStrLength = ADpStr.Length;

            int i = 0;
            for (i = 0; i < 8 - ADpStrLength; i++)
            {
                ADpStr2 = ADpStr2.Insert(i, "0");
            }

            int j = 0;
            int t = i;
            for (i = t; i < 8; i++)
            {
                ADpStr2 = ADpStr2.Insert(i, ADpStr[j].ToString());
                j++;
            }

            ADresultStr = "";

            ADresultStr = ADresultStr.Insert(0, ADpStr2[6].ToString());
            ADresultStr = ADresultStr.Insert(1, ADpStr2[7].ToString());
            ADresultStr = ADresultStr.Insert(2, ADpStr2[4].ToString());
            ADresultStr = ADresultStr.Insert(3, ADpStr2[5].ToString());
            ADresultStr = ADresultStr.Insert(4, ADpStr2[2].ToString());
            ADresultStr = ADresultStr.Insert(5, ADpStr2[3].ToString());
            ADresultStr = ADresultStr.Insert(6, ADpStr2[0].ToString());
            ADresultStr = ADresultStr.Insert(7, ADpStr2[1].ToString());

            return ADresultStr.ToUpper();
        }

        public byte[] StringToByte(string text)
        {
            var tmpbyte = new byte[text.Length / 2];
            var count = 0;
            for (int i = 0; i < text.Length; i += 2)
            {
                var val = byte.MinValue;
                try 
                { 
                    if(text.Substring(i, 2) != "XX")
                        val = byte.Parse(text.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

                    tmpbyte[count] = val;
                    count++;
                } 
                catch (Exception) 
                { 
                }
            }
            return tmpbyte;
        }

        public String ByteToHex(byte[] pByte)
        {
            return BitConverter.ToString(pByte).Replace("-", "");

            /*String Str = "";
            for (Int32 i = 0; i < pByte.Length; i++)
                Str += pByte[i].ToString("x2").ToUpper();
            return Str;*/
        }

        public string AddressToHex(int Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2").ToUpper();
            return retval;
        }

        public string StringToHex(string Value)
        {
            byte[] bytes = Encoding.Default.GetBytes(Value);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2").ToUpper();
            return retval;
        }

        public string DecimalToHex(int Value)
        {
            return Value.ToString("x2").ToUpper();
        }

        public int CoordinateDistance(int StartX, int StartY, int TargetX, int TargetY)
        {
            return Convert.ToInt32(Math.Sqrt(Math.Pow((TargetX - StartX), 2) + Math.Pow((TargetY - StartY), 2)));
        }

        public long GetFileSize(string FilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Debug.WriteLine("Permission denied");
                return -1;
            }
            else if (File.Exists(FilePath))
            {
                return new FileInfo(FilePath).Length;
            }

            return 0;
        }
    }
}
