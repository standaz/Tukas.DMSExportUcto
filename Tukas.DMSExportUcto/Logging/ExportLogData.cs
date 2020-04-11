using System;

namespace Tukas.DMSExportUcto.Log
{
    /// <summary>
    /// Trida pro data o provedenem exportu
    /// </summary>
    internal class ExportLogData
    {
        //internal DateTime Date { get; set; }
        // Parametr exportu - datum posledniho exportu
        internal DateTime DateParameter { get; set; }
        // Log status
        internal LogState State { get; set; }
    }
}
