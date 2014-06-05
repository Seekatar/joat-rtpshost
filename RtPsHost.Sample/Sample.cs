using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost.Sample
{
    class SampleHost : RtPsHost.IPsConsole
    {
        static void Main(string[] args)
        {
            var console = new SampleHost();
            var host = new PsHost();
            host.Initialize(console);

            bool step = false;

            try
            {
                var task = host.InvokeAsync("SampleScripts.xml", step, null);
                task.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception" + e);
            }

            Console.Write("All done.  Press any key to exit. ");
            Console.ReadKey();
        }

#region IPsConsole implementation
        public bool ShouldExit {get;set;}
        

        public int ExitCode {get;set;}

        public ConsoleColor ForegroundColor
        {
            get
            {
                return Console.ForegroundColor;
            }
            set
            {
                Console.ForegroundColor = value;
            }
        }

        public ConsoleColor BackgroundColor
        {
            get
            {
                return Console.BackgroundColor;
            }
            set
            {
                Console.BackgroundColor = value;
            }
        }

        public int WindowWidth
        {
            get { return Console.WindowWidth; }
        }

        public void Write(string msg, WriteType type)
        {
            Console.Write(msg); 
        }

        public void WriteLine(string msg, WriteType type)
        {
            Console.WriteLine(msg); 

        }

        public void Write(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type)
        {
            pushColor();
            ForegroundColor = foreground;
            BackgroundColor = background;
            Write(msg, type);
            popColor();
        }

        public void WriteLine(ConsoleColor foreground, ConsoleColor background, string msg, WriteType type)
        {
            pushColor();
            ForegroundColor = foreground;
            BackgroundColor = background;
            Write(msg, type);
            popColor();
        }

        public int PromptForChoice(string caption, string message, IEnumerable<PromptChoice> choices, int defaultChoice)
        {
            if (!String.IsNullOrWhiteSpace(caption))
                Console.WriteLine(caption);

            if (!String.IsNullOrWhiteSpace(message))
                Console.WriteLine(message);

            int ret = -1;

            int i = 0;
            foreach( var c in choices)
            {
                Console.WriteLine( "{0} {1}", i++, c.Name.Replace("&",""));
            }
            Console.Write("Chose: (default is {0}) ", defaultChoice);
            var s = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(s))
                return defaultChoice;
            else
            {
                Int16 r;
                if (Int16.TryParse(s, out r))
                {
                    if ( r < i )
                    {
                        ret = r;
                    }
                }
            }
            return ret;
        }

        public void WriteProgress(ProgressInfo progressInfo)
        {
            int count = 0;
            int width = WindowWidth - (1+progressInfo.Activity.Length);
            if (progressInfo.PercentComplete > 0)
                count = (int)(width/(100.0/ progressInfo.PercentComplete));
            
            var s = new String('*', count);
            s += new String('_', width - count);
            Console.WriteLine(progressInfo.Activity+" "+s);
        }

        public string PromptForString(string caption, string message, string description)
        {
            if (!String.IsNullOrWhiteSpace(caption))
                Console.WriteLine(caption);

            if (!String.IsNullOrWhiteSpace(message))
                Console.WriteLine(message);

            if (!String.IsNullOrWhiteSpace(description))
                Console.WriteLine(description);

            return Console.ReadLine();
        }

        public System.Security.SecureString PromptForSecureString(string caption, string message, string description)
        {
            if (!String.IsNullOrWhiteSpace(caption))
                Console.WriteLine(caption);

            if (!String.IsNullOrWhiteSpace(message))
                Console.WriteLine(message);

            if (!String.IsNullOrWhiteSpace(description))
                Console.WriteLine(description);

            ConsoleKeyInfo k;
            var ss = new System.Security.SecureString();
            while ( true )
            {
                k = Console.ReadKey(true);
                if (k.Key == ConsoleKey.Enter || k.Key == ConsoleKey.Escape )
                    break;
                Write("*", WriteType.Host);
                ss.AppendChar(k.KeyChar);
            }
            Console.WriteLine();
            return ss;
        }
#endregion

        ConsoleColor _fg;
        ConsoleColor _bg;

        private void pushColor()
        {
            _fg = ForegroundColor;
            _bg = BackgroundColor;
        }

        private void popColor()
        {
            ForegroundColor = _fg;
            BackgroundColor = _bg;
        }
    }

}
