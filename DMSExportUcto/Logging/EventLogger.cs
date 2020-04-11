using DMSExportUcto.Config;
using DMSExportUcto.XML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DMSExportUcto.Log
{
    /// <summary>
    /// Trida zapouzdrujici praci se souborem logu udalosti
    /// Soubor je ve formatu XML
    /// </summary>
    internal class EventLogger
    {
        // Nazvy elementu
        private const string ROOT = "events"; // korenovy element
        private const string EVENT = "event"; // element s daty exportu
        // Atributy elementu event
        private const string LOG_DATE = "log_date";
        private const string DATE = "date";
        private const string ID = "id";
        private const string RECORDS = "records";
        private const string STATE = "state";

        internal XmlFile LogFile { get; private set; }
        internal string Filename { get; private set; }

        internal EventLogger()
        {
            this.Filename = ExportConfigurationSection.Section.EventLogFilename;
            this.LogFile = new XmlFile(this.Filename, ROOT);
        }

        /// <summary>
        /// Metoda pro zapis dat do event logu
        /// </summary>
        /// <param name="data"></param>
        internal void AddElement(EventLogData data)
        {
            // konfigurace
            ExportConfigurationSection cfg = ExportConfigurationSection.Section;
            // Pokud nelogujeme, pak nic nezapisujeme
            if (cfg.LogLevel != LogLevel.None)
            {
                // Test proti konfiguraci logu, jsou-li data s timto statusem zapsatelna do logu
                if ((data.State < LogState.Info) || ((cfg.LogLevel == LogLevel.All) && (data.State == LogState.Info)))
                {
                    // Pridani elementu a ulozeni souboru
                    this.LogFile.AddElement(ToEventElement(data));
                    this.LogFile.Save();
                }
            }
        }

        /// <summary>
        /// Metoda slouzici pro prevod strutury na element event
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private XElement ToEventElement(EventLogData data)
        {
            return new XElement(EVENT,
                new XAttribute(LOG_DATE, DateTime.Now.ToString(DMSExportConst.DATE_FORMAT)),
                new XAttribute(DATE, data.Date.HasValue ? data.Date.Value.ToString(DMSExportConst.DATE_FORMAT) : string.Empty),
                new XAttribute(STATE, data.State.ToString()),
                new XAttribute(ID, GetDataValue(data.Id)),
                new XAttribute(RECORDS, GetDataValue(data.Records)),
                new XCData(data.Message));
        }

        /// <summary>
        /// Metoda, ktera vraci prazdny retezec na nedefinovana data (null)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetDataValue<T>(Nullable<T> data) where T : struct
        {
            return data.HasValue ? data.Value.ToString() : string.Empty;
        }
    }
}
