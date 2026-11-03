using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Galio_Flash_Tool
{
    // Own the handle rather than reflecting into SerialPort internals. Configure
    // once, then toggle only the requested line, as pySerial does on Windows.
    internal sealed class NativeLoaderPort : ILoaderPort, IDisposable
    {
        private readonly SafeFileHandle handle;
        private readonly byte[] oneByte = new byte[1];
        public NativeLoaderPort(string name)
        {
            int number;
            if (name == null || !name.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                || !int.TryParse(name.Substring(3), out number) || number <= 0)
                throw new ArgumentException("Seleccione un puerto COM válido.");
            handle = CreateFile(@"\\.\COM" + number, 0xC0000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);
            if (handle.IsInvalid) { var error = Error(); handle.Dispose(); throw error; }
            try
            {
                var dcb = new Dcb { Length = (uint)Marshal.SizeOf(typeof(Dcb)) };
                Check(GetCommState(handle, ref dcb));
                dcb.Baud = 115200; dcb.Flags = 1; // fBinary only; no automatic handshake.
                dcb.ByteSize = 8; dcb.Parity = 0; dcb.StopBits = 0;
                dcb.XonChar = 0x11; dcb.XoffChar = 0x13;
                Check(SetCommState(handle, ref dcb));
                var timeouts = new Timeouts { ReadConstant = 10, WriteConstant = 2000 };
                Check(SetCommTimeouts(handle, ref timeouts));
                Dtr = false; Rts = false;
            }
            catch { handle.Dispose(); throw; }
        }
        public bool Dtr { set { Check(EscapeCommFunction(handle, value ? 5u : 6u)); } }
        public bool Rts { set { Check(EscapeCommFunction(handle, value ? 3u : 4u)); } }
        public int ReadByte()
        {
            uint read;
            Check(ReadFile(handle, oneByte, 1, out read, IntPtr.Zero));
            return read == 0 ? -1 : oneByte[0];
        }
        public void Write(string record)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(record);
            uint written;
            Check(WriteFile(handle, bytes, (uint)bytes.Length, out written, IntPtr.Zero));
            if (written != bytes.Length) throw new IOException("Escritura serie incompleta. Repita la carga completa.");
        }
        public void DiscardInput() { Check(PurgeComm(handle, 8)); }
        public void Dispose()
        {
            if (handle.IsClosed) return;
            // Always release the handle, even after a USB removal.
            try { EscapeCommFunction(handle, 6); EscapeCommFunction(handle, 4); }
            finally { handle.Dispose(); }
        }
        private static IOException Error() { return new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message); }
        private static void Check(bool success) { if (!success) throw Error(); }
        [StructLayout(LayoutKind.Sequential)]
        private struct Dcb
        {
            public uint Length, Baud, Flags;
            public ushort Reserved, XonLimit, XoffLimit;
            public byte ByteSize, Parity, StopBits, XonChar, XoffChar, ErrorChar, EofChar, EventChar;
            public ushort Reserved1;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Timeouts
        {
            public uint ReadInterval, ReadMultiplier, ReadConstant, WriteMultiplier, WriteConstant;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCommState(SafeFileHandle handle, ref Dcb dcb);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle handle, ref Dcb dcb);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommTimeouts(SafeFileHandle handle, ref Timeouts timeouts);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EscapeCommFunction(SafeFileHandle handle, uint function);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool PurgeComm(SafeFileHandle handle, uint flags);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle handle, byte[] buffer, uint count, out uint read, IntPtr overlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(SafeFileHandle handle, byte[] buffer, uint count, out uint written, IntPtr overlapped);
    }
}
