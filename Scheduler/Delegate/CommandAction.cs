using System.Diagnostics;
using System.Text;

namespace Scheduler.Delegate
{
    public class CommandAction : BaseAction
    {
        private readonly string _fileName;
        private readonly string _arguments;

        public CommandAction(string fileName) : this(fileName, string.Empty)
        {
        }

        public CommandAction(string fileName, string arguments)
        {
            _fileName = fileName;
            _arguments = arguments;
        }

        public CommandAction(string[] command) : this(command[0], BuildArguments(command))
        {
        }

        public override void Execute()
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _fileName, Arguments = _arguments,
                    RedirectStandardOutput = false, RedirectStandardError = false,
                    UseShellExecute = false, CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public static string BuildArguments(string[] arguments, int ignoreLeadingArguments = 1)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = ignoreLeadingArguments; i < arguments.Length; i++)
            {
                builder.Append(arguments[i]);
                if (i + 1 < arguments.Length)
                    builder.Append(" ");
            }

            return builder.ToString();
        }
    }
}
