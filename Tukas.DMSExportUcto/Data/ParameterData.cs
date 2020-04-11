using Oracle.ManagedDataAccess.Client;
using System;

namespace Tukas.DMSExportUcto.Data
{
    /// <summary>
    /// Trida pro data parametru SQL prikazu
    /// </summary>
    public class ParameterData
    {
        // Nazev parametru
        public string Name { get; set; }
        // Hodnota parametru
        public object Value { get; set; }
        // Oracle DB typ parametru
        public OracleDbType DbType { get; set; }
    }
}
