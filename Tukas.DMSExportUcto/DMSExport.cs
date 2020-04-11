using Tukas.DMSExportUcto.Config;
using Tukas.DMSExportUcto.Data;
using Tukas.DMSExportUcto.DB;
using Tukas.DMSExportUcto.Log;
using Tukas.DMSExportUcto.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace Tukas.DMSExportUcto
{
    /// <summary>
    /// Trida realizujici export dat
    /// </summary>
    internal class DMSExport
    {
        // Konstanty pro vytvoreni casti nazvu souboru dle data
        private const string DAY_BASIS = "ddMMyy";  // denni reporty
        private const string MONTH_BASIS = "MMyy";  // mesicni reporty

        // Datum posledniho uspesneho exportu
        internal DateTime LastRun { get; private set; }

        /// <summary>
        /// Konstruktor, kde dostane datum posledniho exportu
        /// </summary>
        /// <param name="lastRun">Datum posledniho exportu</param>
        internal DMSExport(DateTime lastRun)
        {
            this.LastRun = lastRun;
        }

        /// <summary>
        /// Metoda provadejici export dat
        /// </summary>
        internal void ExportData()
        {
            // Log udalosti
            EventLogger logger = new EventLogger();
            // Ziskani seznamu uzaverek od posledniho uspesneho exportu dat
            var exportUctoList = GetUctoToExport(this.LastRun);
            // Exportuj kazdou uzaverku
            foreach (var item in exportUctoList)
            {
                // Info element o uzaverce do logu udalosti
                logger.AddElement(new EventLogData() {
                    State = LogState.Info,
                    Date = item.UzavDoDat,
                    Message = string.Format("UzavTypId = {0}, UzavUctoId = {1}, UzavDoDat = {2}, ProvozovnaId = {3}, VstupDat = {4}",
                        item.UzavTypId, item.UzavUctoId, item.UzavDoDat, item.ProvozovnaId, item.VstupDat)
                });
                // Export dat uzaverky
                ExportUctoData(item.UzavTypId, item.UzavUctoId, item.UzavDoDat, item.ProvozovnaId);
            }
        }

        /// <summary>
        /// Export dat uzaverky
        /// </summary>
        /// <param name="uzavTypId">Id typu uzaverky</param>
        /// <param name="uzavUctoId">Id ucetni uzaverky</param>
        /// <param name="uzavDoDat">Datum, do kdy byla uzaverka delana</param>
        /// <param name="provozovnaId">Id provozovny</param>
        private void ExportUctoData(long uzavTypId, long uzavUctoId, DateTime uzavDoDat, long provozovnaId)
        {
            // Log udalosti
            EventLogger logger = new EventLogger();
            // Konfiguracni element
            ExportConfigurationSection cfg = ExportConfigurationSection.Section;
            // Data uzaverky do logu
            EventLogData logData = new EventLogData() { Id = uzavUctoId, Date = uzavDoDat };
            // Ziskani casti nazvu souboru dle data a export_flag uzaverky
            var dataFromTyp = GetTypParameters(uzavTypId, uzavDoDat);
            // Zalogovani ziskanych dat dle typu uzaverky
            logger.AddElement(new EventLogData()
            {
                State = LogState.Info,
                Message = string.Format("dataFromTyp: Item1 = {0}, Item2 = {1}", dataFromTyp.Item1, dataFromTyp.Item2)
            });

            // Ziskani dat pro provedeni exportu (text SQL prikazu, nazev souboru, oddelovac apod.)
            var exportParameters = GetExportParameters(dataFromTyp.Item2);
            // Log poctu ziskanych parametru exportu
            logger.AddElement(new EventLogData()
            {
                State = LogState.Info,
                Message = string.Format("exportParameters: Count = {0}", exportParameters.Count())
            });

            string filename;
            foreach (var item in exportParameters)
            {
                // Zalogujeme data pro export
                logger.AddElement(new EventLogData() {
                    State = LogState.Info,
                    Message = string.Format("ExportId = {2}, ExportDefaultType = {0}, ExportFilename = {1}", item.ExportDefaultType, item.ExportFilename, item.ExportId)
                });

                // Pokud je ExportDefaultType = 1, pak provedeme export, jinak preskocime
                if (item.ExportDefaultType == 1)
                {
                    // Vynulujeme pocitadlo zaznamu
                    logData.Records = 0;
                    try
                    {
                        // Sestavime nazev souboru
                        filename = string.Format("{0}{4}{1}{2}{3}.csv",
                            cfg.ExportDirectory,
                            item.ExportFilename,
                            dataFromTyp.Item1,
                            cfg.DefaultFileSuffix ? GetDefaultSuffix(item.ExportSuffix, uzavUctoId) : string.Format("_{0}", provozovnaId),
                            cfg.BranchPrefix ? string.Format("{0}_", provozovnaId) : string.Empty);
                        // Provedeme export dat s oddelovacem
                        logData.Records = ExportUctoDataWithDelimiter(item.ExportSelect, uzavUctoId, item.ExportDefaultDelim, filename);
                        // Zaevidujeme nazev souboru s exportovanymi daty
                        logData.Message = string.Format("Filename: {0}", filename);
                        // Export probehl v poradku
                        logData.State = LogState.OK;
                    }
                    catch (Exception ex)
                    {
                        // Chyba pri exportu
                        logData.State = LogState.Error;
                        // Zalogujeme chybu
                        logData.Message = ex.ToMessage();
                    }
                }
                // Data logu zapiseme do log souboru udalosti
                logger.AddElement(logData);
            }
        }

        /// <summary>
        /// Metoda provede export dat s oddelovacem do souboru
        /// </summary>
        /// <param name="exportSelect">Text SQL prikazu</param>
        /// <param name="uzavUctoId">Id ucetni uzaverky</param>
        /// <param name="delimiter">Oddelovac</param>
        /// <param name="filename">Nazev souboru s exportovanymi daty</param>
        /// <returns></returns>
        private long ExportUctoDataWithDelimiter(string exportSelect, long uzavUctoId, string delimiter, string filename)
        {
            // Pocitadlo zaznamu
            long result = 0;
            // Konfiguracni element
            ExportConfigurationSection cfg = ExportConfigurationSection.Section;
            // Buffer pro sestaveni exportni radky
            StringBuilder dataLine = new StringBuilder();
            // Parametr exportu - Id uzaverky
            List<ParameterData> parameters = new List<ParameterData>();
            parameters.Add(new ParameterData { Name = "UZAVUCTO_ID", Value = uzavUctoId, DbType = OracleDbType.Int64 });

            // Vytvorime soubor
            using (var stream = new StreamWriter(filename, false, Encoding.GetEncoding(cfg.DataFileEncoding)))
            {
                // Pripojime se k DB - transakce neni nutna, nemenime data
                using (var context = new OracleContext())
                {
                    // Vykoname SQL prikaz pro export dat
                    var reader = context.ExecuteReader(exportSelect, parameters);
                    // Pokud mame exportovat nazvy sloupcu, zapiseme je do souboru
                    if (cfg.DataHeaderRow)
                        stream.WriteLine(OracleContext.GetFieldNames(reader, delimiter));
                    // Exportujeme vlastni data
                    while (reader.Read())
                    {
                        // Data ze vsech sloupcu zapiseme do bufferu
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataLine.AppendFormat("{0}{1}", OracleContext.GetFieldValue(reader, i), (i < reader.FieldCount - 1 ? delimiter : string.Empty));
                        }
                        // Buffer zapiseme do souboru
                        stream.WriteLine(dataLine.ToString());
                        stream.Flush();
                        // Vyprazdnime buffer
                        dataLine.Clear();
                        // Zvysime pocitadlo exportovanych radku o 1
                        result++;
                    }
                    // Zavreme IDataReader
                    reader.Close();
                    // using uzavre pripojeni k DB - zaridi implementace IDisposable
                }
                // Zavreme stream souboru, do ktereho jsme exportovali data (using uklidi alokovane zdroje StreamWriter je IDisposable)
                stream.Close();
            }

            return result;
        }

        /// <summary>
        /// Metoda vrati seznam ucetnich uzaverek urcenych k exportu (zalozene po poslednim exportu dat)
        /// </summary>
        /// <param name="lastExportDate">Datum posledniho exportu uzaverek</param>
        /// <returns></returns>
        private IEnumerable<UctoData> GetUctoToExport(DateTime lastExportDate)
        {
            // Seznam uzaverek k exportu
            List<UctoData> result = new List<UctoData>();
            // Parametry dotazu
            List<ParameterData> parameters = new List<ParameterData>();
            parameters.Add(new ParameterData { Name = "vstup_dat", Value = lastExportDate, DbType = OracleDbType.TimeStamp });
            // Vytvorime pripojeni k DB
            using (var context = new OracleContext())
            {
                // Vratime si seznam uzaverek zalozenych od poledniho exportu
                var reader = context.ExecuteReader("SELECT uzav_typ_id, uzavucto_id, uzav_do_dat, vstup_dat, provozovna_id FROM uzavucto WHERE vstup_dat > :vstup_dat AND uzav_typ_id = 1 ORDER BY vstup_dat DESC", parameters);
                while (reader.Read())
                {
                    result.Add(new UctoData()
                    {
                        UzavTypId = Convert.ToInt64(reader.GetValue(0)),
                        UzavUctoId = Convert.ToInt64(reader.GetValue(1)),
                        UzavDoDat = Convert.ToDateTime(reader.GetValue(2)),
                        VstupDat = Convert.ToDateTime(reader.GetValue(3)),
                        ProvozovnaId = Convert.ToInt64(reader.GetValue(4))
                    });
                }
                // Zavreni IDataReader po precteni vsech dat
                reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Metoda vrati parametry exportu (text SQL prikazu, oddelovac, nazev souboru apod.)
        /// </summary>
        /// <param name="exportFlag"></param>
        /// <returns></returns>
        private IEnumerable<ExportData> GetExportParameters(long exportFlag)
        {
            // Parametry pro export dat
            List<ExportData> result = new List<ExportData>();
            // Parametr dotazu pro parametry exportu dat
            List<ParameterData> parameters = new List<ParameterData>();
            parameters.Add(new ParameterData { Name = "export_flag", Value = exportFlag, DbType = OracleDbType.Int32 });
            // Vytvorime pripojeni k DB
            using (var context = new OracleContext())
            {
                // SQL prikaz pro ziskani parametru exportu dat
                var reader = context.ExecuteReader("SELECT export_id, export_select, export_fixlength, export_filename,  export_defaulttype, export_defaultdelim, export_suffix FROM dms_export WHERE export_flag = :export_flag", parameters);
                while (reader.Read())
                {
                    result.Add(new ExportData()
                    {
                        ExportId = reader.GetInt64(0),
                        ExportSelect = reader.GetString(1),
                        ExportFilename = reader.GetString(3),
                        ExportDefaultType = reader.GetInt64(4),
                        ExportDefaultDelim = reader.GetString(5),
                        ExportSuffix = Convert.ToString(reader.GetValue(6))
                    });
                }
                reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Metoda vraci export_flag a cast nazvu souboru (udaje z data exportu) s exportovanymi daty
        /// </summary>
        /// <param name="uzavTypId">Typ uzaverky</param>
        /// <param name="uzavDoDat">Datum uzaverky exportu</param>
        /// <returns></returns>
        private Tuple<string, long> GetTypParameters(long uzavTypId, DateTime uzavDoDat)
        {
            Tuple<string, long> result = null;
            switch (uzavTypId)
            {
                case 1:
                    result = new Tuple<string, long>(uzavDoDat.ToString(DAY_BASIS), 1);
                    break;
                case 2:
                    result = new Tuple<string, long>(uzavDoDat.ToString(MONTH_BASIS), 1);
                    break;
                case 3:
                    result = new Tuple<string, long>(uzavDoDat.ToString(DAY_BASIS), 2);
                    break;
                case 4:
                    result = new Tuple<string, long>(uzavDoDat.ToString(MONTH_BASIS), 2);
                    break;
                case 5:
                    result = new Tuple<string, long>(uzavDoDat.ToString(DAY_BASIS), 32);
                    break;
                case 6:
                    result = new Tuple<string, long>(uzavDoDat.ToString(MONTH_BASIS), 32);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Metoda vraci priponu do nazvu souboru s exportovanymi daty
        /// </summary>
        /// <param name="suffixCommandText">SQL prikaz pro ziskani pripony</param>
        /// <param name="uzavUctoId">Id ucetni uzaverky</param>
        /// <returns></returns>
        private string GetDefaultSuffix(string suffixCommandText, long uzavUctoId)
        {
            // Pokud neni zadan SQL prikaz, neni co spoustet
            if (string.IsNullOrEmpty(suffixCommandText))
                return string.Empty;

            string result;
            try
            {
                // Parametr SQL prikazu - Id ucetni uzaverky
                List<ParameterData> parameters = new List<ParameterData>();
                parameters.Add(new ParameterData { Name = "UZAVUCTO_ID", Value = uzavUctoId, DbType = OracleDbType.Int64 });
                using (var context = new OracleContext())
                {
                    result = context.ExecuteScalar(suffixCommandText, parameters).ToString();
                }
            }
            catch //(Exception ex)
            {
                result = string.Empty;
            }
            return result;
        }
    }
}
