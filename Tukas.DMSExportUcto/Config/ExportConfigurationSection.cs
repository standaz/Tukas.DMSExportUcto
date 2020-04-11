using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tukas.DMSExportUcto.Config
{
    /// <summary>
    /// Trida zabezpecujici ziskani dat z konfiguracniho elementu
    /// </summary>
    public sealed class ExportConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Název klíče v konfiguračním souboru
        /// </summary>
        private const string CONFIGURATION_SECTION_NAME = "exportSettings";

        /// <summary>
        /// Názvy atributů klíče exportSettings v konfiguračním souboru
        /// </summary>
        private const string PROPERTY_COMMAND_TIMEOUT = "commandTimeout";
        private const string PROPERTY_EXPORT_DIRECTORY = "exportDirectory";
        private const string PROPERTY_DATA_FILENAME = "dataFilename";
        private const string PROPERTY_DATA_HEADER_ROW = "dataHeaderRow";
        private const string PROPERTY_EVENT_LOG_FILENAME = "eventLogFilename";
        private const string PROPERTY_LOG_LEVEL = "logLevel";
        private const string PROPERTY_DEFAULT_FILE_SUFFIX = "defaultFileSuffix";
        private const string PROPERTY_DATA_FILE_ENCODING = "dataFileEncoding";
        private const string PROPERTY_BRANCH_PREFIX = "branchPrefix";
        private const string PROPERTY_CULTURE_INFO = "cultureInfo";

        private static readonly ExportConfigurationSection section = ConfigurationManager.GetSection(CONFIGURATION_SECTION_NAME) as ExportConfigurationSection;

        public static ExportConfigurationSection Section
        {
            get
            {
                return section;
            }
        }

        /// <summary>
        /// Timeout pro vykonani SQL prikazu
        /// </summary>
        [ConfigurationProperty(PROPERTY_COMMAND_TIMEOUT, IsRequired = true, DefaultValue = 30)]
        [IntegerValidator(ExcludeRange = false, MaxValue = 600, MinValue = 1)]
        public int CommandTimeout
        {
            get { return Convert.ToInt32(base[PROPERTY_COMMAND_TIMEOUT].ToString()); }
        }

        /// <summary>
        /// Adresar pro export dat - MUSI koncit znakem '\'
        /// </summary>
        [ConfigurationProperty(PROPERTY_EXPORT_DIRECTORY, IsRequired = true)]
        public string ExportDirectory
        {
            get { return base[PROPERTY_EXPORT_DIRECTORY].ToString(); }
        }

        /// <summary>
        /// Soubor s daty provedenych exportu a datem posledniho uspesneho exportu
        /// </summary>
        [ConfigurationProperty(PROPERTY_DATA_FILENAME, IsRequired = true)]
        public string DataFilename
        {
            get { return base[PROPERTY_DATA_FILENAME].ToString(); }
        }

        /// <summary>
        /// Encoding pro exportni soubor - vychozi hodnota 'windows-1250'
        /// </summary>
        [ConfigurationProperty(PROPERTY_DATA_FILE_ENCODING, DefaultValue = "windows-1250", IsRequired = true)]
        public string DataFileEncoding
        {
            get { return base[PROPERTY_DATA_FILE_ENCODING].ToString(); }
        }

        /// <summary>
        /// Mame-li provadet export seznamu sloupcu
        /// </summary>
        [ConfigurationProperty(PROPERTY_DATA_HEADER_ROW, DefaultValue = true, IsRequired = true)]
        public bool DataHeaderRow
        {
            get { return Convert.ToBoolean(base[PROPERTY_DATA_HEADER_ROW].ToString()); }
        }

        /// <summary>
        /// Mame-li uzit priponu souboru z DB (true) nebo cislo provozovny (false)
        /// </summary>
        [ConfigurationProperty(PROPERTY_DEFAULT_FILE_SUFFIX, DefaultValue = true, IsRequired = true)]
        public bool DefaultFileSuffix
        {
            get { return Convert.ToBoolean(base[PROPERTY_DEFAULT_FILE_SUFFIX].ToString()); }
        }

        /// <summary>
        /// Soubor pro logovani udalosti - Nazev souboru pripadne vcetne cesty
        /// Ucet, pod nimz bezi aplikace, MUSI mit opravneni pro vytvoreni a zapis do prislusneho adresare
        /// </summary>
        [ConfigurationProperty(PROPERTY_EVENT_LOG_FILENAME, IsRequired = true)]
        public string EventLogFilename
        {
            get { return base[PROPERTY_EVENT_LOG_FILENAME].ToString(); }
        }

        /// <summary>
        /// Uroven logovani None | Export | All
        /// </summary>
        [ConfigurationProperty(PROPERTY_LOG_LEVEL, DefaultValue = LogLevel.All, IsRequired = true)]
        public LogLevel LogLevel
        {
            get { return (LogLevel)Enum.Parse(typeof(LogLevel), base[PROPERTY_LOG_LEVEL].ToString()); }
        }

        /// <summary>
        /// Mame-li pouzit cislo provozovny jako prefix nazvu souboru
        /// </summary>
        [ConfigurationProperty(PROPERTY_BRANCH_PREFIX, DefaultValue = true, IsRequired = true)]
        public bool BranchPrefix
        {
            get { return Convert.ToBoolean(base[PROPERTY_BRANCH_PREFIX].ToString()); }
        }

        /// <summary>
        /// Nastaveni narodniho prostredi
        /// </summary>
        [ConfigurationProperty(PROPERTY_CULTURE_INFO, DefaultValue = "cs-CZ", IsRequired = true)]
        public string CultureInfo
        {
            get { return base[PROPERTY_CULTURE_INFO].ToString(); }
        }
    }
}
