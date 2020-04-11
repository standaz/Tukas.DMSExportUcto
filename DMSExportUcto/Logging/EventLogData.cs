using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSExportUcto.Log
{
    /// <summary>
    /// Trida pro data do zapis do logu udalosti
    /// </summary>
    internal class EventLogData
    {
        // Id - uzaverky
        internal long? Id { get; set; }
        // Datum uzaverky / posledniho exportu
        internal DateTime? Date { get; set; }
        // Pocet zaznamu
        internal long? Records { get; set; }
        // Log status
        internal LogState State { get; set; }
        // Logovana zprava (text vyjimky, informace apod.)
        internal string Message { get; set; }
    }
}
