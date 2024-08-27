using System;
using System.Linq;
using System.Security;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace STARFIRE.BackEnd
{
    internal class Memory
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [Flags]
        public enum MemoryProtectionFlags : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public MemoryProtectionFlags AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);


        public bool bAttached;
        public int PID;
        public IntPtr hProc;
        public IntPtr dwBase;


        public static int GetProcID(string pName)
        {
            var processes = Process.GetProcessesByName(pName);
            if (processes.Length <= 0)
                return 0;

            return processes[0].Id;
        }

        public static IntPtr GetModuleBase(string pName, string mName = "")
        {
            IntPtr result = IntPtr.Zero;

            var processes = Process.GetProcessesByName(pName);
            if (processes.Length <= 0)
                return result;

            var proc = processes[0];
            if (mName.Count() > 0)
            {
                string key = pName.ToLower();

                foreach (ProcessModule module in proc.Modules)
                {
                    if (module.ToString().ToLower() != key)
                        continue;

                    return module.BaseAddress;
                }

                return result;
            }

            return proc.Modules[0].BaseAddress;
        }


        public bool Attach(string pName, ProcessAccessFlags flags)
        {
            if (bAttached)
                return false;

            PID = GetProcID(pName);
            if (PID <= 0)
                return false;

            hProc = OpenProcess(flags, false, PID);
            if (hProc.ToInt64() <= 0)
                return false;

            dwBase = GetModuleBase(pName);
            if (dwBase.ToInt64() <= 0)
                return false;

            bAttached = true;

            return true;
        }

        public void Detach()
        {
            //  winapi close handle to process
            CloseHandle(hProc);

            PID = 0;
            hProc = IntPtr.Zero;
            dwBase = IntPtr.Zero;
            bAttached = false;
        }

        public IntPtr GetAddr(int offset)
        {
            if (!bAttached || dwBase.Equals(0))
                return IntPtr.Zero;

            return dwBase + offset;
        }

        public byte[] ReadBytes(IntPtr addr, int dwSize)
        {
            int bytesRead;
            byte[] buffer = new byte[dwSize];

            if (ReadProcessMemory(hProc, addr, buffer, dwSize, out bytesRead) && dwSize == bytesRead)
                return buffer;

            throw new Win32Exception($"couldn't read {{{dwSize}}} byte(s) from 0x{{{addr:X8}}}.");
        }

        public bool WriteBytes(IntPtr addr, byte[] buffer, int dwSize)
        {
            int bytesWritten;
            if (WriteProcessMemory(hProc, addr, buffer, dwSize, out bytesWritten) && dwSize == bytesWritten)
                return true;

            throw new Win32Exception($"couldn't write {{{dwSize}}} byte(s) to 0x{addr:X8}.");
        }

        public T Read<T>(IntPtr addr) where T : struct
        {
            byte[] buffer = ReadBytes(addr, Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var m = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return m;
        }

        public IntPtr GetAddyChain(IntPtr addr, Int32[] offsets, int dwSize)
        {
            var p = addr;
            for (int i = 0; i < offsets.Length; i++)
            {
                p = Read<IntPtr>(p);
                p += offsets[i];
            }

            return p;
        }

        public T ReadChain<T>(IntPtr addr, Int32[] offsets, int dwSize) where T : struct
        {
            var p = GetAddyChain(addr, offsets, dwSize);
            return Read<T>(p);

            //  var p = addr;
            //  for (int i = 0; i < dwSize; i++)
            //  {
            //      p = Read<IntPtr>(p);
            //      p += offsets[i];
            //  }
            //  
            //  return Read<T>(p);
        }

        public bool Write<T>(IntPtr addr, T data) where T : struct
        {
            int size = Marshal.SizeOf(data);
            byte[] buffer = new byte[size];

            //  convert structure to bytes
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr<T>(data, ptr, false);
            handle.Free();


            //  write bytes
            return WriteBytes(addr, buffer, size);
        }

        public bool ReadBool(IntPtr addr)
        {
            return Read<byte>(addr) == 1;
        }

        public Int32 ReadInt32(IntPtr addr)
        {
            return Read<Int32>(addr);
        }

        public float ReadFloat(IntPtr addr)
        {
            return Read<float>(addr);
        }
    }
}
