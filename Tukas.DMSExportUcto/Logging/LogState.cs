using System;

namespace Tukas.DMSExportUcto.Log
{
    /// <summary>
    /// Logovany status dat
    /// </summary>
    internal enum LogState : int
    {
        // Vse v poradku
        OK = 0,
        // Chyba
        Error = 1,
        // Informace
        Info = 2
    }
}
