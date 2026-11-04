using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Galio_Flash_Tool
{
    internal sealed class BoardMap
    {
        public int Start;
        public int End;
        public string Micro = "--";
        public string Section;

        public static BoardMap Read(string path, string banner)
        {
            var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> values = null;
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string section = line.Substring(1, line.Length - 2).Trim();
                    values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    sections.Add(section, values);
                    continue;
                }
                int split = line.IndexOf('=');
                if (values == null || split < 1) throw new InvalidDataException("Línea inválida en placas.ini: " + line);
                values.Add(line.Substring(0, split).Trim(), line.Substring(split + 1).Trim());
            }
            string[] fields = banner.Split(',');
            string sectionName = fields[0].Substring(5) + " | " + fields[1].Substring(8);
            if (!sections.TryGetValue(sectionName, out values))
            {
                sectionName = "General";
                if (!sections.TryGetValue(sectionName, out values))
                    throw new InvalidDataException("Falta [General] en placas.ini.");
            }
            var map = new BoardMap { Section = sectionName };
            string start, end, micro;
            if (!values.TryGetValue("Inicio", out start) || !values.TryGetValue("Fin", out end) ||
                !int.TryParse(start, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out map.Start) ||
                !int.TryParse(end, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out map.End) ||
                map.Start < 0 || map.End >= 0x300000 || map.End <= map.Start ||
                map.Start % 16 != 0 || map.End % 16 != 15)
                throw new InvalidDataException("Rango de aplicación inválido en placas.ini [" + sectionName + "]. Use bloques completos de 16 bytes.");
            if (values.TryGetValue("Micro", out micro)) map.Micro = micro;
            return map;
        }
    }
}
