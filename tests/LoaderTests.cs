using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Galio_Flash_Tool;

class LoaderTests
{
    static int passed;
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
    static void Reject(string text, string name) { try { FirmwareImage.Parse(text); } catch (InvalidDataException) { Check(true, name); return; } throw new Exception(name); }
    static string R(int kind, int address, params byte[] data) { return FirmwareImage.Record(kind, address, data) + "\n"; }
    static string App = R(0, 0x600, 1, 2, 3, 4);
    static string Eof = R(1, 0);
    static void Main()
    {
        var image = FirmwareImage.Parse("; comment\n\n" + App + Eof);
        Check(image.Created == null && FirmwareImage.Parse(App + Eof + ";CRC=2BC9  CREATED=\"02-nov.-26 21:56\"").Created == "02-nov.-26 21:56", "CCS build timestamp after EOF");
        Check(image.Microcontroller == null && FirmwareImage.Parse(App + Eof + ";PIC18F4550\n;CRC=1234").Microcontroller == "PIC18F4550", "CCS target device after EOF");
        Check(image.PayloadBytes == 4, "blank lines and comments");
        Check(image.Records.All(r => r.Length <= 64), "loader buffer limit");
        Reject(App, "missing EOF");
        Reject(Eof, "missing reset vector");
        Reject(App + App + Eof, "overlap");
        Reject(App + Eof + App, "content after EOF");
        Reject(R(0, 0, 1, 2, 3, 4) + Eof, "bootloader protected");
        Reject(R(4, 0, 0, 2) + App + Eof, "flash upper bound");
        Reject(":040600000102030400\n" + Eof, "bad checksum");
        Reject(":GG06000001020304EC\n" + Eof, "bad hex digits");
        Reject(App + R(2, 0, 0, 0) + Eof, "unsupported record");
        var fuses = FirmwareImage.Parse(App + R(4, 0, 0, 0x30) + R(0, 0, 1, 2) + Eof);
        Check(fuses.SkippedFuseBytes == 2 && fuses.PayloadBytes == 4, "fuses excluded");
        var banked = FirmwareImage.Parse(App + R(0, 0xFFFE, 5, 6) + R(4, 0, 0, 1) + R(0, 0, 7, 8) + Eof);
        var rebuilt = new Dictionary<int, byte>(); int bank = 0;
        foreach (string record in banked.Records)
        {
            int count = Convert.ToInt32(record.Substring(1, 2), 16), addr = Convert.ToInt32(record.Substring(3, 4), 16), kind = Convert.ToInt32(record.Substring(7, 2), 16);
            if (kind == 4) bank = Convert.ToInt32(record.Substring(9, 4), 16) << 16;
            if (kind != 0) continue;
            if (count > 26 || addr % 128 + count > 128) throw new Exception("row crossing");
            for (int i = 0; i < count; i++) rebuilt.Add(bank + addr + i, Convert.ToByte(record.Substring(9 + i * 2, 2), 16));
        }
        Check(rebuilt[0x600] == 1 && rebuilt[0x604] == 255 && rebuilt[0xFFFE] == 5 && rebuilt[0x10000] == 7, "payload reconstruction, gap fill, rows and banks");
        const string other = "name=Other,version=BOOT:1.2,date=2-Nov-26,time=21:14:39";
        Check(StationLoader.BootIdentification(other) == other && StationLoader.IsBanner(other), "accept identified customer hardware");
        Check(StationLoader.BootIdentification(other.Replace("name=Other", "name=")) == null, "reject missing hardware name");
        const string legacy = "name=EXCORE-V1,version=BOOT:1.1,date=2-Nov-26,time=21:14:39";
        Check(StationLoader.BootIdentification("debug" + legacy) == legacy, "legacy version following partial debug output");
        Check(StationLoader.IsBanner(legacy.Replace("EXCORE-V1", "Protolink-Station-Monitor")), "boot version is not hardcoded");
        var port = new FakePort();
        var loader = new StationLoader(port, CancellationToken.None);
        Check(loader.Upload(image, n => { }, s => { }), "complete upload and application startup");
        Check(port.Controls.SequenceEqual(new[] { "DFalse", "RFalse", "RTrue", "DTrue", "RFalse", "DFalse" }), "crossed autoboot sequence");
        Check(port.Writes == image.Records.Count, "all normalized records transmitted");
        var manual = new FakePort();
        Check(new StationLoader(manual, CancellationToken.None).Upload(image, n => { }, s => { }, loader.Banner)
            && manual.Controls.Count == 0 && manual.Writes == image.Records.Count, "manual session uploads without toggling DTR/RTS");
        var bad = new FakePort(); bad.Rx.Enqueue(0x15);
        try { new StationLoader(bad, CancellationToken.None).Expect(new byte[] { 0x13, 6, 0x11 }, 20); throw new Exception("accepted NAK"); }
        catch (IOException) { Check(true, "reject unexpected response"); }
        try { new StationLoader(new FakePort(), CancellationToken.None).Expect(new byte[] { 6 }, 20); throw new Exception("missing timeout"); }
        catch (TimeoutException) { Check(true, "response timeout"); }
        var cancel = new CancellationTokenSource(); cancel.Cancel();
        try { new StationLoader(new FakePort(), cancel.Token).Expect(new byte[] { 6 }, 20); throw new Exception("missing cancel"); }
        catch (OperationCanceledException) { Check(true, "cooperative cancellation"); }
        var missingFinal = new FakePort { MissingFinal = true };
        try { new StationLoader(missingFinal, CancellationToken.None).Upload(image, n => { }, s => { }); throw new Exception("missing EOF accepted"); }
        catch (TimeoutException) { Check(true, "missing final EOF confirmation fails"); }
        var wrongBoard = new FakePort { WrongBoard = true };
        Check(new StationLoader(wrongBoard, CancellationToken.None).Upload(image, n => { }, s => { }), "customer name can upload");
        var general = BoardMap.Read(Environment.GetCommandLineArgs()[1], other);
        var station = BoardMap.Read(Environment.GetCommandLineArgs()[1], loader.Banner);
        Check(general.Section == "General" && general.Start == 0x600 && station.Micro == "18F67K40", "INI default and board/boot match");
        Check(FirmwareImage.Parse(R(0, 0x800, 1, 2, 3, 4) + Eof, 0x800, 0x7FFF).PayloadBytes == 4, "custom application vector");
        var disconnected = new FakePort { Disconnect = true };
        try { new StationLoader(disconnected, CancellationToken.None).Upload(image, n => { }, s => { }); throw new Exception("disconnect ignored"); }
        catch (IOException) { Check(true, "disconnect propagates as failure"); }
        var noStartup = new FakePort { NoStartup = true };
        Check(!new StationLoader(noStartup, CancellationToken.None).Upload(image, n => { }, s => { }), "accepted upload without startup remains unconfirmed");
        Console.WriteLine(passed + " checks passed.");
    }
    class FakePort : ILoaderPort
    {
        public Queue<int> Rx = new Queue<int>();
        public List<string> Controls = new List<string>();
        public int Writes;
        public bool MissingFinal, WrongBoard, Disconnect, NoStartup;
        bool reset;
        readonly System.Diagnostics.Stopwatch timing = System.Diagnostics.Stopwatch.StartNew();
        double resetAt, bannerAt;
        public bool Dtr { set { Controls.Add("D" + value); } }
        public bool Rts { set {
            Controls.Add("R" + value);
            if (value) { reset = true; resetAt = timing.Elapsed.TotalMilliseconds; }
            else if (reset) {
                if ((timing.Elapsed.TotalMilliseconds - resetAt) < 200) throw new Exception("reset too short");
                reset = false; bannerAt = timing.Elapsed.TotalMilliseconds;
                Add(WrongBoard ? "name=Other,version=BOOT:1.2,date=2-Nov-26,time=21:14:39\r\n" : "name=Protolink-Station-Monitor,version=BOOT:1.2,date=2-Nov-26,time=21:14:39\r\n");
            }
        } }
        void Add(string text) { foreach (char ch in text) Rx.Enqueue(ch); }
        public int ReadByte() { if (Rx.Count == 0) { Thread.Sleep(1); return -1; } return Rx.Dequeue(); }
        public void DiscardInput() { Rx.Clear(); }
        public void Write(string record)
        {
            if ((timing.Elapsed.TotalMilliseconds - bannerAt) < 1050) throw new Exception("first write too early");
            if (Disconnect) throw new IOException("USB disconnected");
            Writes++; Rx.Enqueue(0x13); Rx.Enqueue(6); Rx.Enqueue(0x11);
            if (record == ":00000001FF\r" && !MissingFinal) { Rx.Enqueue(6); Rx.Enqueue(0x11); if (!NoStartup) Add("INFO app=station-client fw=0.1.1\r\n"); }
        }
    }
}
