using System.Collections.Generic;
using YSGM_GUI.Handlers;

namespace YSGM_GUI
{
    public class CommandMap
    {
        public static Dictionary<string, BaseCommand> handlers = new();

        public static void RegisterAll()
        {
            handlers.Add("shell", new ShellCommand()); // SSH shell
            handlers.Add("sql", new SQLCommand()); // SQL query
            handlers.Add("gm", new GMCommand()); // GM command
            handlers.Add("mail", new MailCommand()); // Send mail
            handlers.Add("muip", new MUIPCommand()); // raw MUIP query

            // Exit app
            handlers.Add("exit", new ExitCommand());
            handlers.Add("quit", new ExitCommand());

            // Player data
            handlers.Add("pull", new PullCommand());
            handlers.Add("push", new PushCommand());
        }
    }
}
