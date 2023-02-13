using System.Configuration;
using System;
using System.Windows;
using YSGM_GUI;
using System.Linq;

static class Execute
{
    public static string ExecuteCommand(string userInput)
    {
        string[] split = userInput!.Split(' ');
        string cmd = split[0];
        // If the user entered a valid command, execute it.
        if (CommandMap.handlers.ContainsKey(cmd))
        {
            var handler = CommandMap.handlers[cmd];
            var arguments = split.Skip(1).ToArray();

            try
            {
                return handler.Execute(arguments);
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
        else
        {
            return "Invalid command.";//
        }
    }

}


