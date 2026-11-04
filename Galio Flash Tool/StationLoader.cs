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

        internal string ReadLineByte()
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
            if (IsBanner(received) && bannerAt < 0)
            { Banner = received; bannerAt = clock.Elapsed.TotalMilliseconds; }
        }
        internal static string BootIdentification(string received)
        {
            if (received == null) return null;
            int start = received.LastIndexOf("name=", StringComparison.Ordinal);
            if (start < 0) return null;
            string banner = received.Substring(start);
            string[] fields = banner.Split(',');
            string[] keys = { "name=", "version=BOOT:", "date=", "time=" };
            if (fields.Length != keys.Length) return null;
            for (int i = 0; i < keys.Length; i++)
                if (!fields[i].StartsWith(keys[i], StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(fields[i].Substring(keys[i].Length))) return null;
            return banner;
        }

        internal static bool IsBanner(string received)
        {
            return received != null && BootIdentification(received) == received;
        }
        private void CaptureFor(int milliseconds)
        {
            double end = clock.Elapsed.TotalMilliseconds + milliseconds;
            while (clock.Elapsed.TotalMilliseconds < end) CaptureBanner();
        }
        internal void PulseBoot(Action<string> afterReset = null)
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
            if (afterReset != null) Log = afterReset;
            CaptureFor(200);
            port.Dtr = false;
        }

        public void Enter()
        {
            PulseBoot();
            double deadline = clock.Elapsed.TotalMilliseconds + 3000;
            while (bannerAt < 0 && clock.Elapsed.TotalMilliseconds < deadline) CaptureBanner();
            if (bannerAt < 0) throw new TimeoutException("No se identificó el bootloader. Revise placa, COM y alimentación.");
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
        public bool Upload(FirmwareImage image, Action<int> progress, Action<string> stage, string manualBanner = null)
        {
            if (manualBanner == null) { stage("Entering bootloader"); Enter(); }
            else
            {
                if (!IsBanner(manualBanner)) throw new InvalidOperationException("Bootloader sin identificar.");
                Banner = manualBanner;
                CaptureFor(1050);
            }
            stage("Uploading");
            for (int i = 0; i < image.Records.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    port.Write(image.Records[i]);
                    Expect(new byte[] { 0x13, 0x06, 0x11 }, 3000);
                }
                catch (TimeoutException ex) { throw new TimeoutException("Registro " + (i + 1) + "/" + image.Records.Count + ": " + ex.Message, ex); }
                catch (IOException ex)
                { throw new IOException("Registro " + (i + 1) + "/" + image.Records.Count + ": " + ex.Message, ex); }
                if (i + 1 < image.Records.Count) progress(i + 1);
            }
            stage("Confirming transfer");
            try { Expect(new byte[] { 0x06, 0x11 }, 2000); }
            catch (TimeoutException ex) { throw new TimeoutException("Confirmación final EOF: " + ex.Message, ex); }
            catch (IOException ex) { throw new IOException("Confirmación final EOF: " + ex.Message, ex); }
            progress(image.Records.Count);
            Log("Registros confirmados: " + image.Records.Count);
            Log("Carga aceptada; sin lectura de verificación de flash.");
            stage("Waiting for restart");
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
