using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;

namespace RtPsHost
{
    /// <summary>
    /// class to hold the details about a script step to run
    /// </summary>
    public class ScriptInfo
    {
        public enum ScriptType
        {
            normal,     // part of run
            preRun,     // run before the transforms
            postRun,    // run after all the scripts run and the log file has closed
            transform,  // run the transform.  Some environments run it differently.  Runs after pre, before main
            success,    // special post run step to run if successful
            fail        // special post run step to run if failed
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public ScriptInfo()
        {
        }

        /// <summary>
        /// the script itself
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// friendly name of it
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// brief description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Echo this script to the output window when running 
        /// </summary>
        public bool EchoScript { get; set; }

        /// <summary>
        /// Never prompt to run this script in step mode
        /// </summary>
        /// if true always runs it
        public bool NeverPrompt { get; set; }

        /// <summary>
        /// if the script gets an error, prompt the user to continue instead of halting
        /// </summary>
        public bool PromptOnError { get; set; }

        public ScriptType Type { get; set; }
    }

    internal static class ScriptInfoExtensions
    {
        /// <summary>
        /// load an array of scriptInfo from a file
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="xml">The XML file name.</param>
        /// <param name="scriptSet">The optional script set for filtering commands.</param>
        public static void LoadFromXmlFile(this List<ScriptInfo> me, string xml, string scriptSet )
        {
            XDocument doc = XDocument.Load(xml);
            me.LoadFromXml(doc, scriptSet);
        }

        /// <summary>
        /// load an array of scriptInfo from an XDocument
        /// </summary>
        /// <param name="me"></param>
        /// <param name="xml"></param>
        /// <param name="scriptSet">The optional script set for filtering commands.</param>
        public static void LoadFromXml(this List<ScriptInfo> me, XDocument doc, string scriptSet)
        {
            me.Clear();
            IList<string> steps = null;
            var allScripts = doc.Root.Elements("script");

            if ( !String.IsNullOrWhiteSpace(scriptSet))
            {
                var ss = doc.Root.XPathSelectElement(String.Format("/scripts/scriptSet[@name=\"{0}\"]", scriptSet));
                if ( ss != null )
                {
                    bool whiteList = true;
                    var listType = ss.Attribute("listType");
                    if ( listType != null )
                        whiteList = String.Equals( listType.Value, "white", StringComparison.CurrentCultureIgnoreCase );

                    steps =  doc.Root.XPathSelectElements(String.Format("/scripts/scriptSet[@name=\"{0}\"]/step", scriptSet)).Where( o => o.Attribute("id") != null ).Select( o => o.Attribute("id").Value).ToList();
                    if ( !whiteList )
                    {
                        steps = allScripts.Where(o => o.Attribute("id") != null ).Select( o => o.Attribute("id").Value).Except(steps).ToList();
                    }

                    if (steps != null && steps.Count == 0)
                    {
                        steps = null;
                    }
                }
            }
            foreach (var s in allScripts)
            {
                if (!String.IsNullOrWhiteSpace(s.Value))
                {
                    var id = (string)s.Attribute("id");
                    var si = new ScriptInfo()
                    {
                        Script = s.Value,
                        Name = (string)s.Attribute("name") ?? "<no name>",
                        Description = (string)s.Attribute("description") ?? String.Empty,
                        EchoScript = (bool?)s.Attribute("echoScript") ?? false,
                        NeverPrompt = (bool?)s.Attribute("neverPrompt") ?? false,
                        PromptOnError = (bool?)s.Attribute("promptOnError") ?? false,
                        Type = ScriptInfo.ScriptType.normal
                    };
                    ScriptInfo.ScriptType st;
                    if (Enum.TryParse<ScriptInfo.ScriptType>((string)s.Attribute("type") ?? ScriptInfo.ScriptType.normal.ToString(), out st))
                        si.Type = st;

                    if ( steps == null || steps.Contains(id))
                        me.Add(si);
                }
            }
        }
    }
}
