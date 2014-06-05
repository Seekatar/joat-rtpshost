
namespace RtPsHost
{
    /// <summary>
    /// class to hide the PS implementation of a ChoiceDescription
    /// </summary>
    public class PromptChoice
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">name of choice to display</param>
        /// <param name="helpString">help to show</param>
        public PromptChoice( string name, string helpString = null )
        {
            Name = name;
            HelpString = helpString;
        }

        /// <summary>
        /// name of choice to display
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// help to show
        /// </summary>
        public string HelpString { get; set; }
    }
}
