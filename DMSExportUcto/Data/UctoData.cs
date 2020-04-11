using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSExportUcto.Data
{
    /// <summary>
    /// Trida pro drzeni informaci o ucetnich uzaverkach
    /// Slouzi jen pro nacteni informaci o provedenych uzaverkach, abychom nemeli zaraz rozbehlych nekolik SQL dotazu
    /// </summary>
    internal class UctoData
    {
        // Typ uzaverky
        internal long UzavTypId { get; set; }
        // Id uzaverky
        internal long UzavUctoId { get; set; }
        // Datum, do ktereho je uzaverka delana
        internal DateTime UzavDoDat { get; set; }
        // Datum vytvoreni uzaverky
        internal DateTime VstupDat { get; set; }
        // Id provozovny
        internal long ProvozovnaId { get; set; }
    }
}
