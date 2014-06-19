using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost.Sample
{
    class Sample 
    {
        static string _script = @"
            Write-Host 'Now in the script'
            Write-Warning ""x = $x""
            Write-Output ""s = $s""
            Write-Host ""Dict has: ""
            foreach ( $d in $dict.Keys )
            {
                Write-Host ""    dict[$d] = $($dict[$d])""
            }
Write-Error ""ow!""
";

        static void Main(string[] args)
        {
            bool step = false;

            // construct my host
            var console = new SampleConsoleHost();

            // construct the PsHost, and initialize it with my host
            var host = PsHostFactory.CreateHost();
            host.Initialize(console);

            // items to pass into host
            var items = new Dictionary<string, object>();
            items["x"] = 123;
            items["s"] = "this is a string";
            items["dict"] = items;

            try
            {
                // invoke the scripts
                console.WriteLine("\n\nTest: Invoking script\n\n", WriteType.Verbose);
                var task = host.InvokeAsync(_script, "Test", items);
                task.Wait();

                console.WriteLine("\n\nTest: Invoking script steps in XML file\n\n", WriteType.Verbose);
                task = host.InvokeAsync("SampleScripts.xml", step, null);
                task.Wait();
                            }
            catch (Exception e)
            {
                Console.WriteLine("Exception" + e);
            }

            Console.Write("All done.  Press any key to exit. ");
            Console.ReadKey();
        }

    }

}
