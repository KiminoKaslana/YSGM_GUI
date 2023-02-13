using Proto;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace YSGM_GUI.Handlers
{
    public class PullCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            // smh mihoyo...
            int UID = int.Parse(args[0]);
            var c = UID.ToString().Last();

            // Pull user data
            var user = SQLManager.Instance.Execute("hk4e_db_user_32live", $"SELECT * FROM t_player_data_{c} WHERE uid = '{UID}'");

            var binDataStr = user.SelectSingleNode("row/field[@name='bin_data']")?.InnerText;
            
            // Parse data

            var binData = FromHex(binDataStr?.Remove(0, 10) ?? ""); // Remove 0xZLIB

            byte[] decompressed;
            using (var zlib = new ZLibStream(new MemoryStream(binData), CompressionMode.Decompress))
            {
                var ms = new MemoryStream();
                zlib.CopyTo(ms);
                decompressed = ms.ToArray();
            }

            var parsedBin = PlayerDataBin.Parser.ParseFrom(decompressed);

            // Create folder and save
            var path = Path.GetFullPath($"./data_{UID}");
            Directory.CreateDirectory(path);
            File.WriteAllText($"{path}/user.xml", FormatXml(user.InnerXml));
            File.WriteAllText($"{path}/bin_data.json", Prettify(parsedBin?.ToString() ?? ""));

            return $"Saved to {path}";
        }

        private string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                // Handle and throw if fatal exception here; don't just ignore them
                return xml;
            }
        }
        
        private byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        private string Prettify(string unPrettyJson)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

            return JsonSerializer.Serialize(jsonElement, options);
        }
    }
}
