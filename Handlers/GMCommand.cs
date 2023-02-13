using System.Linq;

namespace YSGM_GUI.Handlers
{
    public class GMCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: gm <uid> <command>";
            }
            string uid = args[0];
            string cmd = string.Join(" ", args.Skip(1));
            return MUIPManager.Instance.GM(uid, cmd);
        }
    }
}
