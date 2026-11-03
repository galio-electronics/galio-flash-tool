using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Galio_Flash_Tool
{
    internal interface ILoaderPort
    {
        bool Dtr { set; }
        bool Rts { set; }
        int ReadByte();
        void Write(string record);
        void DiscardInput();
    }

    internal sealed class StationLoader
    {
        private readonly ILoaderPort port;
        private readonly CancellationToken token;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly StringBuilder line = new StringBuilder();
        private double bannerAt = -1;
        public string Banner { get; private set; }
        public Action<string> Log = delegate { };
        public StationLoader(ILoaderPort port, CancellationToken token) { this.port = port; this.token = token; }

        private string ReadLineByte()
        {
            token.ThrowIfCancellationRequested();
            int value = port.ReadByte();
            if (value < 0) return null;
            if (value == '\n') { string result = line.ToString().Trim(); line.Clear(); return result; }
            if (value != '\r') line.Append((char)value);
            if (line.Length > 1024) line.Clear();
            return null;
        }
        private void CaptureBanner()
        {
            string received = ReadLineByte();
            if (received == null) return;
            Log(received);
            if (received.StartsWith("name=Protolink-Station-Monitor,version=BOOT:1.2,date=", StringComparison.Ordinal)
                && received.Contains(",time=") && received.Substring(received.IndexOf(",time=") + 6).Length > 0)
            { if (bannerAt < 0) { Banner = received; bannerAt = clock.Elapsed.TotalMilliseconds; } }
        }
        private void CaptureFor(int milliseconds)
        {
            double end = clock.Elapsed.TotalMilliseconds + milliseconds;
            while (clock.Elapsed.TotalMilliseconds < end) CaptureBanner();
        }
        public void Enter()
        {
            port.Dtr = false; port.Rts = false;
            port.DiscardInput();
            CaptureFor(50);
            port.Rts = true; // Crossed NPN: RESET active.
            CaptureFor(200);
            // Only accept a fresh banner produced by releasing this reset.
            bannerAt = -1; Banner = null; line.Clear();
            port.Dtr = true;
            port.Rts = false; // No delay between these two writes: BOOT active.
            CaptureFor(200);
            port.Dtr = false;
            double deadline = clock.Elapsed.TotalMilliseconds + 3000;
            while (bannerAt < 0 && clock.Elapsed.TotalMilliseconds < deadline) CaptureBanner();
            if (bannerAt < 0) throw new TimeoutException("No se identificó Station/Monitor BOOT:1.2. Revise placa, COM y alimentación.");
            while (clock.Elapsed.TotalMilliseconds - bannerAt < 1050) CaptureBanner();
        }
        internal void Expect(byte[] expected, int timeoutMs)
        {
            double deadline = clock.Elapsed.TotalMilliseconds + timeoutMs;
            int index = 0;
            while (index < expected.Length)
            {
                token.ThrowIfCancellationRequested();
                if (clock.Elapsed.TotalMilliseconds >= deadline)
                    throw new TimeoutException("Sin confirmación del cargador. Repita la carga completa.");
                int value = port.ReadByte();
                if (value < 0) continue;
                if (value != expected[index++])
                    throw new IOException("Respuesta inesperada del cargador: 0x" + value.ToString("X2") + ". Repita la carga completa.");
            }
        }
        public bool Upload(FirmwareImage image, Action<int> progress, Action<string> stage)
        {
            stage("Autoboot"); Enter();
            stage("Cargando");
            for (int i = 0; i < image.Records.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                port.Write(image.Records[i]);
                Expect(new byte[] { 0x13, 0x06, 0x11 }, 3000);
                progress(i + 1);
            }
            Expect(new byte[] { 0x06, 0x11 }, 2000);
            Log("Carga aceptada; sin lectura de verificación de flash.");
            stage("Reinicio");
            line.Clear();
            double deadline = clock.Elapsed.TotalMilliseconds + 5000;
            while (clock.Elapsed.TotalMilliseconds < deadline)
            {
                string received = ReadLineByte();
                if (received == null) continue;
                Log(received);
                if (received.StartsWith("INFO app=", StringComparison.Ordinal) && received.Contains(" fw=")) return true;
            }
            return false;
        }
    }
}
