
namespace RtPsHost
{
    // class to hide the PS implementation of a ChoiceDescription
    public class PromptChoice
    {
        public PromptChoice( string name, string helpString = null )
        {
            Name = name;
            HelpString = helpString;
        }
        public string Name { get; set; }
        public string HelpString { get; set; }
    }
}
