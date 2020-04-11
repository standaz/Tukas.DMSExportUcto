using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSExportUcto.Config
{
    /// <summary>
    /// Uroven event logu
    /// </summary>
    public enum LogLevel : int
    {
        None = 0,   // nelogujeme nic
        Export = 1, // logujeme pouze status exportu
        All = 2     // logujeme vse (status exportu, vyjimky, info)
    }
}
