using DMSExportUcto.Log;
using DMSExportUcto.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DMSExportUcto
{
    class Program
    {
        /// <summary>
        /// Vstupni bod aplikace
        /// </summary>
        /// <param name="args">parametry spusteni z prikazoveho radku</param>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(DMSExportUcto_UnhandledException);
            // Nastaveni narodniho prostredi dle konfigurace
            using (var cultureSwitch = new CultureSwitch())
            {
                ExportData();
            }
        }

        /// <summary>
        /// Obsluzna rutina pro nezachycene vyjimky v aplikaci
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DMSExportUcto_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                DMSExportUcto_HandleException(ex, e.IsTerminating);
            }
        }

        /// <summary>
        /// Zapis chyby do logu udalosti
        /// </summary>
        /// <param name="e">Neodchycena vyjimka</param>
        /// <param name="isTerminating"></param>
        private static void DMSExportUcto_HandleException(Exception e, bool isTerminating)
        {
            EventLogger logger = new EventLogger();
            logger.AddElement(new EventLogData() { Message = e.Message, State = LogState.Error });
        }

        /// <summary>
        /// Metoda pro export dat
        /// </summary>
        private static void ExportData()
        {
            // Logovani exportu a pripadnych chyb
            EventLogger eventLogger = new EventLogger();
            ExportLogger exportLogger = new ExportLogger();
            // Zjisteni data posledniho exportu
            DateTime lastRunParameter = exportLogger.GetLastExportDateTime();
            DateTime lastRun = lastRunParameter;
            // Inicializace tridy pro export dat
            DMSExport export = new DMSExport(lastRunParameter);
            try
            {
                // Spusteni exportu dat
                export.ExportData();
                // Pokud pri exportu nedoslo k chybe, oznacime aktualni den za uspesny den posledniho exportu dat
                lastRun = DateTime.Today;
                // Zaznamenani stavu exportu dat
                exportLogger.AddElement(new ExportLogData() { DateParameter = lastRunParameter, State = LogState.OK });
            }
            catch (Exception ex)
            {
                // Doslo-li k chybe, zaznamename do logu udalosti a chybu do logu exportu
                var now = DateTime.Now;
                exportLogger.AddElement(new ExportLogData() { DateParameter = lastRunParameter, State = LogState.Error });
                eventLogger.AddElement(new EventLogData() { Date = lastRunParameter, State = LogState.Error, Message = ex.ToMessage() });
            }
            // Aktualizace data posledniho exportu
            exportLogger.UpdateLastExportElement(lastRun);
        }
    }
}
