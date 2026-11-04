using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Galio_Flash_Tool
{
    internal sealed class FirmwareImage
    {
        public readonly List<string> Records = new List<string>();
        public string Created { get; private set; }

        private static string CreatedFromComment(string line)
        {
            if (!line.StartsWith(";")) return null;
            var match = System.Text.RegularExpressions.Regex.Match(line,
                @"(?:^;|\s)CREATED\s*=\s*(?:""([^""]+)""|(.+))$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            return (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim();
        }

        public string Microcontroller { get; private set; }

        private void ReadComment(string line)
        {
            if (Created == null) Created = CreatedFromComment(line);
            if (Microcontroller == null && System.Text.RegularExpressions.Regex.IsMatch(line,
                @"^;\s*PIC[0-9][A-Za-z0-9-]*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                Microcontroller = line.Substring(1).Trim();
        }

        public static FirmwareImage ReadMetadata(string path)
        {
            var image = new FirmwareImage();
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.StartsWith(";")) image.ReadComment(line);
            }
            return image;
        }

        public string Sha256 { get; private set; }
        public int PayloadBytes { get; private set; }
        public int SkippedFuseBytes { get; private set; }

        public static FirmwareImage Load(string path, int first = 0x600, int last = 0x1FFFF)
        {
            if (!string.Equals(Path.GetExtension(path), ".hex", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Seleccione un archivo Intel HEX (.hex).");
            byte[] bytes = File.ReadAllBytes(path);
            var result = Parse(Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'), first, last);
            using (var sha = SHA256.Create())
                result.Sha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
            return result;
        }

        public static FirmwareImage Parse(string text, int first = 0x600, int last = 0x1FFFF)
        {
            var image = new FirmwareImage();
            var memory = new SortedDictionary<int, byte>();
            long upper = 0;
            bool eof = false;
            int number = 0;
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    number++;
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    if (line.StartsWith(";"))
                    {
                        image.ReadComment(line);
                        continue;
                    }
                    string error = "HEX línea " + number + ": ";
                    if (eof || !line.StartsWith(":" ) || line.Length < 11 || line.Length % 2 != 1)
                        throw new InvalidDataException(error + "formato o contenido después de EOF inválido.");
                    byte[] data = new byte[(line.Length - 1) / 2];
                    for (int i = 0; i < data.Length; i++)
                        if (!byte.TryParse(line.Substring(1 + 2 * i, 2), System.Globalization.NumberStyles.HexNumber,
                            System.Globalization.CultureInfo.InvariantCulture, out data[i]))
                            throw new InvalidDataException(error + "carácter hexadecimal inválido.");
                    int count = data[0], address = data[1] * 256 + data[2], kind = data[3];
                    if (data.Length != count + 5 || (data.Sum(b => (int)b) & 255) != 0)
                        throw new InvalidDataException(error + "longitud/checksum inválido.");
                    if (kind == 4 && count == 2 && address == 0)
                        upper = (long)(data[4] * 256 + data[5]) << 16;
                    else if (kind == 1 && count == 0 && address == 0) eof = true;
                    else if (kind == 0 && count > 0 && address + count <= 0x10000)
                    {
                        long start = upper + address;
                        if (start >= 0x300000 && start + count <= 0x30000E)
                        { image.SkippedFuseBytes += count; continue; }
                        if (start < first || start + count - 1 > last)
                            throw new InvalidDataException(error + "dirección fuera del rango de aplicación: 0x" + start.ToString("X"));
                        for (int i = 0; i < count; i++)
                        {
                            int location = (int)start + i;
                            if (memory.ContainsKey(location)) throw new InvalidDataException(error + "direcciones solapadas.");
                            memory.Add(location, data[4 + i]);
                        }
                    }
                    else throw new InvalidDataException(error + "tipo de registro no admitido.");
                }
            }
            if (!eof || Enumerable.Range(first, 4).Any(a => !memory.ContainsKey(a)))
                throw new InvalidDataException("Falta EOF o vector de aplicación en 0x" + first.ToString("X") + ".");
            image.PayloadBytes = memory.Count;
            int end = (memory.Keys.Max() + 16) & ~15, bank = -1;
            for (int start = first; start < end;)
            {
                if (start >> 16 != bank)
                {
                    bank = start >> 16;
                    image.Records.Add(Record(4, 0, new byte[] { (byte)(bank >> 8), (byte)bank }));
                }
                int count = Math.Min(26, Math.Min(128 - start % 128, end - start));
                var payload = new byte[count];
                for (int i = 0; i < count; i++)
                { byte value; payload[i] = memory.TryGetValue(start + i, out value) ? value : (byte)255; }
                image.Records.Add(Record(0, start & 65535, payload));
                start += count;
            }
            image.Records.Add(Record(1, 0, new byte[0]));
            return image;
        }

        internal static string Record(int kind, int address, byte[] payload)
        {
            var bytes = new List<byte> { (byte)payload.Length, (byte)(address >> 8), (byte)address, (byte)kind };
            bytes.AddRange(payload);
            bytes.Add((byte)(-bytes.Sum(b => (int)b) & 255));
            return ":" + BitConverter.ToString(bytes.ToArray()).Replace("-", "") + "\r";
        }
    }
}
