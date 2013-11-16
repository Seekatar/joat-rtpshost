
namespace RtPsHost
{
    public enum ProcessingResult
    {
        ok,         // completed ok
        canceled,   // completed, user-canceled
        failed,     // errored out
        close       // ok and Go & Close used or Cancel clicked
    }

}
