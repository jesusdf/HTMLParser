using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;
using HtmlAgilityPack;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace HTMLParser
{
	public class HTMLElement : IDisposable
	{
		#region Private variables
		private const int DEFAULT_COLUMNS = 12;
		private HtmlNode _agilityNode = null;
		#endregion
		#region Public constants
		public const string HtmlRootTagSelector = @"//table";
		public const string HtmlRootTag = @"table";
		#endregion
		#region Properties
		public string Tag { get; set; }

		public string Value { get; set; }

		public int Width { get; set; }

		public HtmlNode AgilityNode { 
			get {
				return _agilityNode;
			}
			set {
				_agilityNode = value;
				ProcessNode(_agilityNode);
			}
		}

		public HTMLElement Parent { get; set; }

		public List<HTMLElement> ChildElements { get; set; }
		#endregion
		#region Methods
		public HTMLElement() {
			ChildElements = new List<HTMLElement>();
			Parent = null;
			Value = "0";
		}

		public void Dispose() {
			ChildElements = null;
			Parent = null;
		}

		public void AppendChildElement(HTMLElement child) {
			ChildElements.Add(child);
		}

		public void AppendNode(HtmlNode node) {
			HTMLElement nodeElement = new HTMLElement();
			nodeElement.Parent = this;
			nodeElement.AgilityNode = node;
			ChildElements.Add(nodeElement);
		}

		private void ProcessNode(HtmlNode n) {
			if (n != null) {
				foreach (HtmlNode c in n.ChildNodes) {
					this.AppendNode(c);
				}
				CalculateSize();
			}
		}

		private void CalculateSize() {
			if (Convert.ToInt32(this.Value) == 0) {
				this.Value = DEFAULT_COLUMNS.ToString();
			}
		}

		public void Fix() {
			// Change original elements and ammend properties
			if (ChildElements.Count() > 0) {
				foreach (HTMLElement e in ChildElements) {
					HtmlNode n = e.AgilityNode;
					if (n != null) {
						switch (n.Name.ToLower()) {
							case @"table":
								n.Name = @"flex:Layout";
								CleanAttributes(n);
								break;
							case @"tr":
								n.Name = @"flex:Row";
								CleanAttributes(n);
								break;
							case @"td":
								n.Name = @"flex:Col";
								CleanAttributes(n);
								SetColAttributes(e);
								break;
						}
					}
					e.Fix();
				}
			}
		}

		private static void CleanAttributes(HtmlNode n) {
			while (n.Attributes.Count() > 0) {
				n.Attributes[0].Remove();
			}
		}

		private static void SetColAttributes(HTMLElement e) {
			HtmlNode n = e.AgilityNode;
			if (n != null) {
				n.Attributes.Add("xs", e.Value);
			}
		}
		#endregion
	}
}

