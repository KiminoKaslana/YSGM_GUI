using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YSGM_GUI
{
    public class SSHManager
    {
        string host;
        string user;
        string exePath;
        public static SSHManager Instance = new();

        private SSHManager()
        {
#if DEBUG
            host = "192.168.1.11";
            user = ConfigurationManager.AppSettings.Get("SSH_USER")!;
#else
            string? host = ConfigurationManager.AppSettings.Get("SSH_HOST")!;
            string? user = ConfigurationManager.AppSettings.Get("SSH_USER")!;
#endif
            // I can't use SSH.NET...
            // FINE. I'll just make a child process

            var enviromentPath = Environment.GetEnvironmentVariable("PATH");
            if (enviromentPath == null) throw new Exception("PATH is null");

            var paths = enviromentPath.Split(';');
            exePath = paths.Select(x => Path.Combine(x, "ssh.exe"))
                               .Where(x => File.Exists(x))
                               .FirstOrDefault()!;
            
            if (string.IsNullOrWhiteSpace(exePath) == true) throw new Exception("SSH not found");
        }

        public string Execute(string cmd)
        {
            Process p = new Process();
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
            p.StartInfo.FileName = exePath;
            p.StartInfo.Arguments = $"{user}@{host} -o LogLevel=error -q /bin/bash";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.Write(cmd);
            p.StandardInput.Flush();
            p.StandardInput.Close();
            var output = "";
            while (p.StandardOutput.EndOfStream == false)
            {
                string? line = p.StandardOutput.ReadLine();
                if (line == null) break;
                output += line;
#if DEBUG
                Console.WriteLine($"[SSH] {line}");
#endif
            }

            return output!;
        }
    }
}
