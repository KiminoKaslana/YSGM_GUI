using Google.Protobuf;
using Proto;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace YSGM_GUI.Handlers
{
    public class PushCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            int UID = int.Parse(args[0]);
            var c = UID.ToString().Last();

            // Parse back to bin_data
            var compressed = Compress(UID);
            var binData = $"{BitConverter.ToString(compressed.ToArray()).Replace("-", "")}";
            Console.WriteLine(binData);

            // Insert to DB
            byte[] ZLIB = Encoding.Default.GetBytes("ZLIB"); // Convert ZLIB to hex
            var xLIB = BitConverter.ToString(ZLIB);
            xLIB = xLIB.Replace("-", "");
            SQLManager.Instance.Execute("hk4e_db_user_32live", $"UPDATE t_player_data_{c} SET bin_data=UNHEX('{xLIB}{binData}') WHERE uid = '{UID}'");

            return $"Pushed ${UID} to DB";
        }

        private MemoryStream Compress(int UID)
        {
            var protoEncoded = PlayerDataBin.Parser.ParseJson(File.ReadAllText($"./data_{UID}/bin_data.json"));
            var arr = protoEncoded.ToByteArray();
            MemoryStream ms = new();
            using var compressor = new DeflateStream(ms, CompressionMode.Compress);
            compressor.Write(arr);
            return ms;
        }
    }
}
