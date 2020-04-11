using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSExportUcto.Data
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
