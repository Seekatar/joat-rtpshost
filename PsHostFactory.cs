using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtPsHost
{
    /// <summary>
    /// class to create a IPsHost
    /// </summary>
    public static class PsHostFactory
    {
        /// <summary>
        /// create a PsHost implementation
        /// </summary>
        /// <returns>IPsHost.  Dispose of it.</returns>
        static public IPsHost CreateHost() { return new PsHost();  }
    }
}
