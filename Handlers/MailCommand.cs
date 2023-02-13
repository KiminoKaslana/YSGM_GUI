using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Proto.WatcherBin.Types;

namespace YSGM_GUI.Handlers
{
    public class MailCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Redirect();
            }
            byte[] b = new byte[0];
            try
            {
                b = Convert.FromBase64String(args[0]);
            } catch
            {
                Redirect();
            }
            string MailReq = Encoding.UTF8.GetString(b);

            var dict = new Dictionary<string, string>();
            var data = JsonSerializer.Deserialize<MailReqI>(MailReq);
#if DEBUG
            Console.WriteLine(JsonSerializer.Serialize(data));
#endif
            foreach (var p in data!.GetType().GetProperties())
            {
                var key = p.Name;
                var val = p.GetValue(data, null);
                dict.Add(key, val?.ToString() ?? "");
            }

            // Add more required keys
            // dict.Add("effective_time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            // Not required
            dict["item_limit_type"] = "2"; // ITEM_LIMIT_GM
            return MUIPManager.Instance.GET(1005, dict);
        }

        private class MailReqI
        {
            public string? uid { get; set; }
            public string? title { get; set; }
            public string? content { get; set; }
            public string? sender { get; set; }
            public string? expire_time { get; set; }
            public bool? is_collectible { get; set; }
            public string? importance { get; set; }
            public string? config_id { get; set; }
            public string? item_limit_type { get; set; }
            public string? tag { get; set; }
            public string? source_type { get; set; }
            public string? item_list { get; set; } // Separated by ,
        }
        
        private void Redirect()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://memetrolls.net/miniprojects/mailparser/",
                UseShellExecute = true
            });
        }
    }
}
