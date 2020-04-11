using Tukas.DMSExportUcto.Config;
using Tukas.DMSExportUcto.Data;
using Tukas.DMSExportUcto.Log;
using Tukas.DMSExportUcto.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Tukas.DMSExportUcto.DB
{
    /// <summary>
    /// Trida obalujici praci s DB Oracle
    /// </summary>
    public class OracleContext : IDisposable
    {
        public OracleConnection Connection { get; private set; }
        public OracleTransaction Transaction { get; private set; }
        private bool disposed;

        /// <summary>
        /// Konstruktor pro vychozi pripojeni k DB bez zahajeni transakce
        /// </summary>
        public OracleContext()
            : this(false)
        {
        }

        /// <summary>
        /// Konstruktor pro pripojeni k DB dle predaneho pripojovaciho retezce bez zahajeni transakce
        /// </summary>
        /// <param name="connectionString">Pripojovaci retezec</param>
        public OracleContext(string connectionString)
            : this(connectionString, false)
        {
        }

        /// <summary>
        /// Konstruktor pro vychozi pripojeni k DB s moznosti zahajeni transakce
        /// </summary>
        /// <param name="startTransaction">Zahajeni(true) / Nezahajeni(false) transakce pri vytvoreni pripojeni</param>
        public OracleContext(bool startTransaction)
            : this(null, startTransaction)
        {
        }

        /// <summary>
        /// Konstruktor pro pripojeni k DB dle predaneho pripojovaciho retezce s moznosti zahajeni transakce
        /// </summary>
        /// <param name="connectionString">Pripojovaci retezec</param>
        /// <param name="startTransaction">Zahajeni(true) / Nezahajeni(false) transakce pri vytvoreni pripojeni</param>
        public OracleContext(string connectionString, bool startTransaction)
        {
            CreateConnection(connectionString, startTransaction);
        }

        /// <summary>
        /// Metoda pro vytvoreni pripojeni k DB a pripadne zahajeni transakce
        /// </summary>
        /// <param name="connectionString">Pripojovaci retezec</param>
        /// <param name="startTransaction">Zahajeni(true) / Nezahajeni(false) transakce pri vytvoreni pripojeni</param>
        private void CreateConnection(string connectionString, bool startTransaction)
        {
            // Pokud je pripojovaci retezec prazdny, vezmi jej z konfigurace
            string connStr = (string.IsNullOrWhiteSpace(connectionString)) ? GetDefaultConnectionString() : connectionString;
            // Vytvoreni a otevreni pripojeni
            this.Connection = new OracleConnection(connStr);
            this.Connection.Open();
            // Specificke DMS-CZ nastaveni session
            SetSession();
            // Zahajeni transakce, pokud je vyzadovano
            if (startTransaction)
                BeginTransaction();
        }

        /// <summary>
        /// Metoda vraci pripojovaci retezec z konfiguracniho souboru
        /// </summary>
        /// <returns></returns>
        private string GetDefaultConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["dataConnection"].ConnectionString;
        }

        /*
        /// <summary>
        /// Metoda pro prevod .NET typu parametru na typ Oracle DB provideru
        /// </summary>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        private static OracleDbType GetOracleDbType(Type parameterType)
        {
            switch (parameterType.Name)
            {
                case "Int16":
                    return OracleDbType.Int16;
                case "Int32":
                    return OracleDbType.Int32;
                case "Int64":
                    return OracleDbType.Int64;
                case "Single":
                    return OracleDbType.Single;
                case "Double":
                    return OracleDbType.Double;
                case "Decimal":
                    return OracleDbType.Decimal;
                case "DateTime":
                    return OracleDbType.TimeStamp;
            }
            return OracleDbType.Varchar2;
        }
        */ 

        /// <summary>
        /// Nastaveni Session dle specifik DMS-CZ
        /// </summary>
        private void SetSession()
        {
            try
            {
                ExecuteNonQuery("ALTER SESSION SET NLS_LANGUAGE = 'CZECH'");
                ExecuteNonQuery("ALTER SESSION SET NLS_TERRITORY = 'CZECH REPUBLIC'");
                ExecuteNonQuery("ALTER SESSION SET NLS_DATE_FORMAT = 'dd.mm.yyyy hh24:mi:ss'");
                ExecuteNonQuery("ALTER SESSION SET NLS_NUMERIC_CHARACTERS = '.,'");
                ExecuteNonQuery("ALTER SESSION SET NLS_SORT = 'XCZECH'");
                ExecuteNonQuery("BEGIN DMSENV.SETENV('1',NULL,NULL,NULL,NULL,NULL,NULL,'0'); END;");
            }
            catch (Exception ex)
            {
                EventLogger logger = new EventLogger();
                logger.AddElement(new EventLogData() { Date = DateTime.Now, Message = ex.Message, State = LogState.Error });
            }
        }

        /// <summary>
        /// Metoda vytvori OracleCommand pro provedeni SQL prikazu
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <param name="parameters">Seznam potrebnych parametru pro provedeni SQL prikazu</param>
        /// <returns></returns>
        private OracleCommand CreateCommand(string commandText, IEnumerable<ParameterData> parameters)
        {
            OracleCommand command = new OracleCommand(commandText, this.Connection);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = ExportConfigurationSection.Section.CommandTimeout;
            AddParameters(command, parameters);
            return command;
        }

        /// <summary>
        /// Pridani parametru k OracleCommand pro spravne vykonani SQL prikazu
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        private void AddParameters(OracleCommand command, IEnumerable<ParameterData> parameters)
        {
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    command.Parameters.Add(item.Name, item.DbType);
                    command.Parameters[item.Name].Value = item.Value;
                }
            }
        }

        /// <summary>
        /// Metoda pro explicitni zahajeni transakce
        /// </summary>
        public void BeginTransaction()
        {
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction();
        }

        /// <summary>
        /// Metoda pro potvrzeni transakce
        /// </summary>
        public void CommitTransaction()
        {
            if (this.Transaction != null)
                this.Transaction.Commit();
        }

        /// <summary>
        /// Metoda pro zruseni transakce
        /// </summary>
        public void RollbackTransaction()
        {
            if (this.Transaction != null)
                this.Transaction.Rollback();
        }

        #region IDisposable Members

        // Zde jsou metody pro uvolneni a uklid zdroju (pripojeni k DB apod.) - viz. IDisposable

        ~OracleContext()
        {
            CleanUp(false);
        }

        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.Transaction != null)
                    {
                        this.Transaction = null;
                    }

                    if (this.Connection != null)
                    {
                        this.Connection.Close();
                        this.Connection.Dispose();
                        this.Connection = null;
                    }
                }
                this.disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Metoda vraci seznam sloupcu z vykonaneho SQL prikazu
        /// </summary>
        /// <param name="reader">ADO.NET IDataReader</param>
        /// <param name="delimiter">oddelovac seznamu poli (strednik, carka, tabulator apod.)</param>
        /// <returns></returns>
        public static string GetFieldNames(IDataReader reader, string delimiter)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < reader.FieldCount; i++)
                result.AppendFormat("{0}{1}", reader.GetName(i), (i < reader.FieldCount - 1) ? delimiter : string.Empty);
            return result.ToString();
        }

        /// <summary>
        /// Metoda vraci obsah sloupce z vykonaneho SQL prikazu pro export do souboru
        /// </summary>
        /// <param name="reader">ADO.NET IDataReader</param>
        /// <param name="index">Index sloupce, jehoz hodnotu chceme zjistit</param>
        /// <returns></returns>
        public static string GetFieldValue(IDataReader reader, int index)
        {
            string result = string.Empty;
            var dbValue = reader.GetValue(index);
            if ((dbValue != null) && (dbValue != DBNull.Value))
            {
                switch (reader.GetFieldType(index).Name)
                {
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Single":
                    case "Double":
                    case "Decimal":
                        result = dbValue.ToString();
                        break;
                    case "DateTime":
                        DateTime dateTime = Convert.ToDateTime(dbValue);
                        //result = string.Format("'{0}'", (dateTime.TimeOfDay != TimeSpan.Zero) ? dateTime.ToString() : dateTime.ToString(DMSExportConst.EXPORT_DATE_FORMAT));
                        result = string.Format("'{0}'", (dateTime.TimeOfDay != TimeSpan.Zero) ? dateTime.ToString(DMSExportConst.EXPORT_DATETIME_FORMAT) : dateTime.ToString(DMSExportConst.EXPORT_DATE_FORMAT));
                        break;
                    default:
                        result = string.Format("'{0}'", dbValue.ToString());
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery vraci data
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(commandText, null);
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery vraci data
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <param name="parameters">Parametry SQL prikazu</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string commandText, IEnumerable<ParameterData> parameters)
        {
            OracleCommand command = CreateCommand(commandText, parameters);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery vraci jednu hodnotu
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(commandText, null);
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery vraci jednu hodnotu
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <param name="parameters">Parametry SQL prikazu</param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText, IEnumerable<ParameterData> parameters)
        {
            OracleCommand command = CreateCommand(commandText, parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery nevraci hodnotu - napr. volani ulozene procedury
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, null);
        }

        /// <summary>
        /// Metoda pro vykonani prikazu, ktery nevraci hodnotu - napr. volani ulozene procedury
        /// </summary>
        /// <param name="commandText">Text SQL prikazu</param>
        /// <param name="parameters">Parametry SQL prikazu</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, IEnumerable<ParameterData> parameters)
        {
            OracleCommand command = CreateCommand(commandText, parameters);
            return command.ExecuteNonQuery();
        }
    }
}
