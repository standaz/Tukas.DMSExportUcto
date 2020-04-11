using Tukas.DMSExportUcto.Config;
using System;
using System.Globalization;
using System.Threading;

namespace Tukas.DMSExportUcto
{
    internal class CultureSwitch : IDisposable
    {
        internal CultureInfo SavedCultureInfo { get; private set; }
        internal CultureInfo SavedUICultureInfo { get; private set; }
        internal CultureInfo CfgCultureInfo { get; private set; }
        private bool disposed;

        internal CultureSwitch()
        {
            this.disposed = false;
            this.SavedCultureInfo = Thread.CurrentThread.CurrentCulture;
            this.SavedUICultureInfo = Thread.CurrentThread.CurrentUICulture;
            this.CfgCultureInfo = CultureInfo.GetCultureInfo(ExportConfigurationSection.Section.CultureInfo);
            Thread.CurrentThread.CurrentCulture = CfgCultureInfo;
            Thread.CurrentThread.CurrentUICulture = CfgCultureInfo;
        }

        ~CultureSwitch()
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
                    Thread.CurrentThread.CurrentCulture = SavedCultureInfo;
                    Thread.CurrentThread.CurrentUICulture = SavedUICultureInfo;
                }
                this.disposed = true;
            }
        }
    }
}
