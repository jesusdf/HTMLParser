using System;
using System.Xml.XPath;
using System.Linq;
using System.IO;
using System.Text;
using HtmlAgilityPack;

namespace HTMLParser
{
    /// <summary>
    /// Handles HTML file parsing
    /// </summary>
    public class HTMLFileParser
    {

        private HtmlDocument _htmlDoc = null;

        /// <summary>
        /// Indicates the file path of the working item.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath"></param>
        public HTMLFileParser(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Open an HTML file to work with it.
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            return Open(FilePath);
        }

        /// <summary>
        /// Open an HTML file to work with it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Open(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    _htmlDoc = new HtmlDocument();

                    // There are various options, set as needed
                    _htmlDoc.OptionFixNestedTags = true;
                    _htmlDoc.OptionOutputOriginalCase = true;
                    _htmlDoc.OptionWriteEmptyNodes = true;

                    // filePath is a path to a file containing the html
                    _htmlDoc.Load(filePath);
                    // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

                    // ParseErrors is an ArrayList containing any errors from the Load statement
                    return !(_htmlDoc.ParseErrors != null && _htmlDoc.ParseErrors.Count() > 0);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Write debug information to file.
        /// </summary>
        /// <param name="fileName"></param>
        public void Describe(string fileName)
        {
            File.WriteAllText(fileName, Describe());
        }

        /// <summary>
        /// Return debug information.
        /// </summary>
        public string Describe()
        {
            StringBuilder sb = new StringBuilder();
            if (_htmlDoc != null)
            {
                using (HTMLElement rootElement = new HTMLElement())
                {
                    rootElement.AppendNode(_htmlDoc.DocumentNode.Descendants(HTMLElement.HtmlRootTag).FirstOrDefault());
                    rootElement.Describe(sb);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Do HTML transformation based on the calculated data.
        /// </summary>
        /// <returns></returns>
        public bool Fix()
        {
            if (_htmlDoc != null)
            {
                using (HTMLElement rootElement = new HTMLElement())
                {
                    rootElement.AppendNode(_htmlDoc.DocumentNode.Descendants(HTMLElement.HtmlRootTag).FirstOrDefault());
                    rootElement.Fix();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Save file to disk.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return Save(makeBackup: false);
        }

        /// <summary>
        /// Save file to disk.
        /// </summary>
        /// <returns></returns>
        public bool Save(bool makeBackup)
        {
            if (_htmlDoc != null)
            {
                if (makeBackup)
                {
                    string backupFilePath = String.Format(@"{0}_backup.html", FilePath);
                    if (File.Exists(backupFilePath))
                    {
                        File.Delete(backupFilePath);
                    }
                    if (File.Exists(FilePath))
                    {
                        File.Copy(FilePath, backupFilePath);
                    }
                }
                _htmlDoc.Save(FilePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Return file as string.
        /// </summary>
        /// <returns></returns>
        public string SaveAsString()
        {
            if (_htmlDoc != null)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter();
                _htmlDoc.Save(sw);
                return sb.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

    }
}

