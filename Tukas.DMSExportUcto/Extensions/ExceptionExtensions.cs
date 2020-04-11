using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tukas.DMSExportUcto.Extensions
{
    /// <summary>
    /// Extension metody pro tridu Exception
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Metoda vrati text vyjimky vcetne textu vnitrnich vyjimek a s vypisem zasobniku
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string ToMessage(this Exception ex)
        {
            StringBuilder result = new StringBuilder();
            // Text vyjimky
            result.AppendLine(string.Format("Message: {0}", ex.Message));
            // Pripojeni textu vnitrnich vyjimek - rekurzivne
            Exception innerException = ex.InnerException;
            while (innerException != null)
            {
                result.AppendLine(string.Format("Inner exception message: {0}", innerException.Message));
                innerException = innerException.InnerException;
            }
            // Pripojeni vypisu zasobniku
            result.AppendLine(string.Format("Stack trace: {0}", ex.StackTrace));
            return result.ToString();
        }
    }
}
