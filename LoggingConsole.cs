using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost
{
    /// <summary>
    /// Console IPsConsole implementation that logs all writes to a log file, no prompting
    /// </summary>
    public class LoggingConsole : RtPsHost.IPsConsole, IDisposable
    {
        private StreamWriter _transcriptFile;


        public virtual void WriteText(ConsoleColor foreground, ConsoleColor background, string msg, bool isLine, WriteType type)
        {
            if (_transcriptFile != null)
            {
                if (isLine)
                    _transcriptFile.WriteLine(msg);
                else
                    _transcriptFile.Write(msg);
            }
        }

        public virtual void WriteText(string msg, bool isLine, WriteType type)
        {
            WriteText(ForegroundColor, BackgroundColor, msg, isLine, type);
        }


        public LoggingConsole(string logFname)
        {
            if ( !String.IsNullOrWhiteSpace(logFname))
                _transcriptFile = File.CreateText(logFname);
        }

        public virtual void Dispose()
        {
            if (_transcriptFile != null)
            {
                _transcriptFile.Close();
                _transcriptFile.Dispose();
                _transcriptFile = null;
            }
        }

        public virtual bool ShouldExit { get; set; }

        public virtual int ExitCode { get; set; }

        public virtual ConsoleColor ForegroundColor
        {
            get
            {
                return ConsoleColor.White;
            }
            set
            {
            }
        }

        public virtual ConsoleColor BackgroundColor
        {
            get
            {
                return ConsoleColor.Black;
            }
            set
            {
            }
        }

        public virtual int WindowWidth
        {
            get { return 500; }
        }

        public virtual void Write(string msg, WriteType type)
        {
            WriteText(msg, false,type);
        }

        public virtual void WriteLine(string msg, WriteType type)
        {
            WriteText(msg, true, type);
        }

        public virtual void WriteSystemMessage(string msg, WriteType type)
        {
            WriteText(msg, true, type);
        }

        public virtual void Write(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type)
        {
            WriteText(foreground, background, msg, false, type);
        }

        public virtual void WriteLine(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type)
        {
            WriteText(foreground, background, msg, true, type);
        }

        public virtual int PromptForChoice(string caption, string message, IEnumerable<RtPsHost.PromptChoice> choices, int defaultChoice )
        {
            WriteText(String.Format("Returning default choice of {0} for prompt {1} {2} {3}", defaultChoice, caption, message, String.Join(", ", choices.Select(o => o.Name))), true,WriteType.System);
            return defaultChoice;
        }

        public virtual void WriteProgress(RtPsHost.ProgressInfo progressInfo)
        {
            WriteText(progressInfo.ToString(), true,WriteType.System);
        }


        public string PromptForString(string caption, string message, string description)
        {
            WriteText(String.Format("Returning empty string for prompt {0} {1} {2} {3}", caption, message, description), true, WriteType.System);
            return String.Empty;
        }

        public System.Security.SecureString PromptForSecureString(string caption, string message, string description)
        {
            WriteText(String.Format("Returning empty secure string for prompt {0} {1} {2} {3}", caption, message, description), true, WriteType.System);
            return new System.Security.SecureString();
        }
    }

}
