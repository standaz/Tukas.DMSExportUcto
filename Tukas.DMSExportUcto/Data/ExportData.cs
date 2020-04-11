using System;

namespace Tukas.DMSExportUcto.Data
{
    /// <summary>
    /// Trida pro drzeni informace o exportu dat
    /// </summary>
    internal class ExportData
    {
        // Id exportu
        internal long ExportId { get; set; }
        // SQL prikaz exportu
        internal string ExportSelect { get; set; }
        // Nazev souboru pro export
        internal string ExportFilename { get; set; }
        // Typ exportu - nas zajimaji predevsim exporty typu 1
        internal long ExportDefaultType { get; set; }
        // Oddelovac znaku v souboru exportu
        internal string ExportDefaultDelim { get; set; }
        // Pripona v nazvu exportovaneho souboru
        internal string ExportSuffix { get; set; }
    }
}
