namespace YSGM_GUI.Handlers
{
    public class ShellCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            string cmd = string.Join(" ", args);
            return SSHManager.Instance.Execute(cmd);
        }
    }
}
