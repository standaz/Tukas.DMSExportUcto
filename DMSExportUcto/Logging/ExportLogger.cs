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
    /// Trida zapouzdrujici praci se souborem logu exportu dat
    /// Soubor je ve formatu XML
    /// </summary>
    internal class ExportLogger
    {
        // Nazvy elementu
        private const string ROOT = "exports";
        private const string EXPORT = "export";
        // Atributy elementu export
        private const string LAST_EXPORT = "last_export";
        private const string DATE = "date";
        private const string DATE_PARAMETER = "date_parameter";
        private const string STATE = "state";

        internal XmlFile LogFile { get; private set; }
        internal string Filename { get; private set; }

        internal ExportLogger()
        {
            this.Filename = ExportConfigurationSection.Section.DataFilename;
            this.LogFile = new XmlFile(this.Filename, ROOT);
        }

        /// <summary>
        /// Metoda prida log exportu do souboru
        /// </summary>
        /// <param name="data">Data exportu</param>
        internal void AddElement(ExportLogData data)
        {
            this.LogFile.AddElement(ToExportElement(data));
            this.LogFile.Save();
        }

        /// <summary>
        /// Metoda vraci datum posledniho uspesneho exportu dat
        /// </summary>
        /// <returns></returns>
        internal DateTime GetLastExportDateTime()
        {
            XElement element = this.LogFile.FindElement(x => x.Name == LAST_EXPORT);
            return (element == null) ?
                DateTime.Today.AddDays(-7).Date :
                DateTime.ParseExact(element.Attribute(DATE_PARAMETER).Value, DMSExportConst.DATE_FORMAT, CultureInfo.InvariantCulture).Date;
        }

        /// <summary>
        /// Metoda aktualizuje datum posledniho uspesneho exportu
        /// </summary>
        /// <param name="parameter">Datum posledniho uspesneho exportu dat</param>
        internal void UpdateLastExportElement(DateTime parameter)
        {
            // Najdi element s datem posledniho exportu
            XElement element = this.LogFile.FindElement(x => x.Name == LAST_EXPORT);
            if (element == null)
            {
                // Pokud neexistuje, vytvor jej a napln daty
                element = new XElement(LAST_EXPORT,
                    new XAttribute(DATE, DateTime.Now.ToString(DMSExportConst.DATE_FORMAT)),
                    new XAttribute(DATE_PARAMETER, parameter.ToString(DMSExportConst.DATE_FORMAT)));
                this.LogFile.AddFirstElement(element);
            }
            else
            {
                // Pokud existuje, pouze aktualizuj atributy
                // date (datum exportu) - informace, kdy export probehl,
                // date_parameter (datum posledniho uspesneho exportu) - pouzije se pro zjisteni uzaverek 
                element.Attribute(DATE).SetValue(DateTime.Now.ToString(DMSExportConst.DATE_FORMAT));
                element.Attribute(DATE_PARAMETER).SetValue(parameter.ToString(DMSExportConst.DATE_FORMAT));
            }
            // Soubor uloz
            this.LogFile.Save();
        }

        /// <summary>
        /// Prevedeni tridy s daty exportu na element
        /// </summary>
        /// <param name="data">Data exportu</param>
        /// <returns></returns>
        private XElement ToExportElement(ExportLogData data)
        {
            XElement result = new XElement(EXPORT,
                new XAttribute(DATE, DateTime.Now.ToString(DMSExportConst.DATE_FORMAT)),
                new XAttribute(DATE_PARAMETER, data.DateParameter.ToString(DMSExportConst.DATE_FORMAT)),
                new XAttribute(STATE, data.State.ToString()));

            return result;
        }
    }
}
