using System;
using System.Xml.XPath;
using System.Linq;
using System.IO;
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

			} catch {
				return false;
			}

		}

		public void Describe() {
			if (_htmlDoc != null) {
				using (HTMLElement rootElement = new HTMLElement()) {
					rootElement.AppendNode(_htmlDoc.DocumentNode.ChildNodes.FindFirst(HTMLElement.HtmlRootTag));
					rootElement.Describe();
				}
			}
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
					File.Copy(FilePath, backupFilePath);
				}
				_htmlDoc.Save(FilePath);
				return true;
			} else {
				return false;
			}
		}
	}
}

