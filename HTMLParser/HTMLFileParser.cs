using System;
using System.Xml.XPath;
using System.Linq;
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

				// filePath is a path to a file containing the html
				_htmlDoc.Load(filePath);
				// Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

				// ParseErrors is an ArrayList containing any errors from the Load statement
				return !(_htmlDoc.ParseErrors != null && _htmlDoc.ParseErrors.Count() > 0);

			} catch {
				return false;
			}

		}

		public bool Fix() {
			if (_htmlDoc != null) {
				HtmlNodeCollection tableNodes = _htmlDoc.DocumentNode.SelectNodes("//table");

				using (HTMLElement _rootElement = new HTMLElement()) {
					foreach (HtmlNode tableNode in tableNodes) {
						_rootElement.AppendNode(tableNode);
					}
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
					System.IO.File.Copy(FilePath, String.Format(@"{0}.bak", FilePath));
				}
				_htmlDoc.Save(FilePath);
				return true;
			} else {
				return false;
			}
		}
	}
}

