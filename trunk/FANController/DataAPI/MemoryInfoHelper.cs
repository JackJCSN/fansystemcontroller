using System.Runtime.InteropServices;

namespace com.JackJCSN.DataAPI
{
    public static class MemoryInfoHelper
    {
        public static int GetMemoryLoad(this IMemoryInfo i)
        {
            MemoryStatusEx mse  = new MemoryStatusEx();

            mse.dwLength = (uint)Marshal.SizeOf(mse);
              
            if (GlobalMemoryStatusEx(ref mse))
            {
                return (int)mse.dwMemoryLoad;
            }
            MemoryStatus ms = new MemoryStatus();
            if(GlobalMemoryStatus(ref ms))
            {
                return (int)ms.dwMemoryLoad;
            }
            return -1;
        }

        public struct MemoryStatusEx
        {
            public uint  dwLength;
            public uint  dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        public struct MemoryStatus
        {
            public uint  dwLength;
            public uint  dwMemoryLoad;
            public ulong dwTotalPhys;
            public ulong dwAvailPhys;
            public ulong dwTotalPageFile;
            public ulong dwAvailPageFile;
            public ulong dwTotalVirtual;
            public ulong dwAvailVirtual;
        }

        public static bool GetMemoryStatus(this IMemoryInfo m,ref MemoryStatus ms)
        {
            return GlobalMemoryStatus(ref ms);
        }

        public static bool GetMemoryStatusEx(this IMemoryInfo m, ref MemoryStatusEx mse)
        {
            mse.dwLength = (uint)Marshal.SizeOf(mse);
            return GlobalMemoryStatusEx(ref mse);
        }

        [DllImport("Kernel32")]
        public static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

        [DllImport("Kernel32")]
        public static extern bool GlobalMemoryStatus(ref MemoryStatus lpBuffer);

    }
}
