using System;
using System.Xml.XPath;
using System.Linq;
using System.IO;
using System.Text;
using HtmlAgilityPack;

namespace HTMLParser
{
	public class HTMLFileParser
	{

		private HtmlDocument _htmlDoc = null;

		public string FilePath { get; set; }

		public HTMLFileParser(string filePath) {
			FilePath = filePath;
		}

		public bool Open() {
			return Open(FilePath);
		}

		public bool Open(string filePath) {
			try {
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
                } else {
                    return false;
                }
			} catch {
				return false;
			}

		}

		public void Describe(string fileName) {
            StringBuilder sb = new StringBuilder();
			if (_htmlDoc != null) {
				using (HTMLElement rootElement = new HTMLElement()) {
					rootElement.AppendNode(_htmlDoc.DocumentNode.ChildNodes.FindFirst(HTMLElement.HtmlRootTag));
					rootElement.Describe(sb);
				}
			}
            File.WriteAllText(fileName, sb.ToString());
		}

		public bool Fix() {
			if (_htmlDoc != null) {
				using (HTMLElement rootElement = new HTMLElement()) {
					rootElement.AppendNode(_htmlDoc.DocumentNode.ChildNodes.FindFirst(HTMLElement.HtmlRootTag));
					rootElement.Fix();
				}
				return true;
			} else {
				return false;
			}
		}

		public bool Save() {
			return Save(makeBackup: false);
		}

		public bool Save(bool makeBackup) {
			if (_htmlDoc != null) {
				if (makeBackup) {
					string backupFilePath = String.Format(@"{0}_backup.html", FilePath);
					if (File.Exists(backupFilePath)) {
						File.Delete(backupFilePath);
					}
                    if (File.Exists(FilePath)) {
                        File.Copy(FilePath, backupFilePath);
                    }
				}
				_htmlDoc.Save(FilePath);
				return true;
			} else {
				return false;
			}
		}

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

