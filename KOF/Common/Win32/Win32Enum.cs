using System;
using System.Runtime.InteropServices;

namespace KOF.Common.Win32
{
    public class Win32Enum
    {
        public enum ObjectInformationClass : int
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_BASIC_INFORMATION
        { // Information Class 0
            public int Attributes;
            public int GrantedAccess;
            public int HandleCount;
            public int PointerCount;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int NameInformationLength;
            public int TypeInformationLength;
            public int SecurityDescriptorLength;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_TYPE_INFORMATION
        { // Information Class 2
            public UNICODE_STRING Name;
            public int ObjectCount;
            public int HandleCount;
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public int Reserved4;
            public int PeakObjectCount;
            public int PeakHandleCount;
            public int Reserved5;
            public int Reserved6;
            public int Reserved7;
            public int Reserved8;
            public int InvalidAttributes;
            public GENERIC_MAPPING GenericMapping;
            public int ValidAccess;
            public byte Unknown;
            public byte MaintainHandleDatabase;
            public int PoolType;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_NAME_INFORMATION
        { // Information Class 1
            public UNICODE_STRING Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GENERIC_MAPPING
        {
            public int GenericRead;
            public int GenericWrite;
            public int GenericExecute;
            public int GenericAll;
        }

        /// <summary>
        /// https://www.geoffchappell.com/studies/windows/km/ntoskrnl/api/ex/sysinfo/handle_table_entry.htm?ts=0,242
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_HANDLE_INFORMATION
        { // Information Class 16
            public ushort ProcessID;
            public ushort CreatorBackTrackIndex;
            public byte ObjectType;
            public byte HandleAttribute;
            public ushort Handle;
            public IntPtr Object_Pointer;
            public IntPtr AccessMask;
        }

        public static class Windows
        {
            public const int NORMAL = 1;
            public const int MINIMIZED = 2;
            public const int MAXIMIZED = 3;
            public const int HIDE = 0;
            public const int RESTORE = 9;
            public const int SHOW = 5;
            
        }

        public const int MAX_PATH = 260;

        public const int DUPLICATE_SAME_ACCESS = 0x2;
        public const int DUPLICATE_CLOSE_SOURCE = 0x1;

        public const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
        public const int CNST_SYSTEM_HANDLE_INFORMATION = 16;

        public const int MEM_COMMIT = 0x1000;
        public const int MEM_RESERVE = 0x2000;
        public const int MEM_DECOMMIT = 0x4000;
        public const int MEM_RELEASE = 0x8000;

        public const int PAGE_EXECUTE_READWRITE = 0x40;
        public const int PAGE_READWRITE = 0x4;

        public const int OBJECT_TYPE_FILE = 0x24;

        public const UInt32 StdOutputHandle = 0xFFFFFFF5;

        public const int MY_CODE_PAGE = 437;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_WRITE = 0x2;
        public const uint OPEN_EXISTING = 0x3;


    }
}
