using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DMSExportUcto.XML
{
    /// <summary>
    /// Trida obalujici praci s XDocument
    /// </summary>
    public class XmlFile
    {
        public XDocument xmlDocument { get; private set; }
        public string Filename { get; private set; }

        /// <summary>
        /// Konstruktor pro vytvoreni / otevreni XML souboru
        /// </summary>
        /// <param name="filename">Nazev souboru</param>
        /// <param name="root">Nazev korenoveho elementu</param>
        public XmlFile(string filename, string root)
        {
            // Test, zda-li mame nazev souboru
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");

            this.Filename = filename;
            try
            {
                // Pokud soubor existuje, nacteme jej, jinak vytvorime novy s korenovym elementem
                xmlDocument = (File.Exists(this.Filename)) ? XDocument.Load(this.Filename) : CreateDocument(root);
            }
            catch //(Exception e)
            {
                // Pokud dojde k vyjimce, vytvorime novy dokument
                xmlDocument = CreateDocument(root);
            }
        }

        /// <summary>
        /// Pridani elementu pod korenovy element
        /// </summary>
        /// <param name="element">Pridavany element</param>
        public void AddElement(XElement element)
        {
            if (element != null)
                this.xmlDocument.Root.Add(element);
        }

        /// <summary>
        /// Pridani elementu na zacatek pod korenovym elementem
        /// </summary>
        /// <param name="element">Pridavany element</param>
        public void AddFirstElement(XElement element)
        {
            if (element != null)
                this.xmlDocument.Root.AddFirst(element);
        }

        /// <summary>
        /// Metoda najde element v korenovych elementech
        /// </summary>
        /// <param name="compareFn">Funkce pro zadani kriteria porovnani na vyhledani elementu</param>
        /// <returns></returns>
        public XElement FindElement(Func<XElement, bool> compareFn)
        {
            return this.xmlDocument.Root.Elements().Where(compareFn).SingleOrDefault();
        }

        /// <summary>
        /// Metoda ulozi data do souboru
        /// </summary>
        public void Save()
        {
            this.xmlDocument.Save(this.Filename);
        }

        /// <summary>
        /// Metoda vytvori XDocument s korenovym elementem
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private XDocument CreateDocument(string root)
        {
            return new XDocument(new XDeclaration("1.0", "utf-8", string.Empty), new XElement(root));
        }
    }
}
