using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;
using HtmlAgilityPack;
using System.Text;

namespace HTMLParser
{
	public class HTMLElement : IDisposable
	{

		#region Private variables
		private const int DEFAULT_COLUMNS = 12;
		private HtmlNode _agilityNode = null;
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

		public List<HTMLElement> Nodes { get; set; }
		#endregion
		public HTMLElement() {
			Nodes = new List<HTMLElement>();
		}

		public void Dispose() {
			Nodes = null;
			Parent = null;
		}

		public void AppendChild(HTMLElement child) {
			Nodes.Add(child);
		}

		public void AppendNode(HtmlNode node) {
			HTMLElement nodeElement = new HTMLElement();
			nodeElement.Parent = this;
			nodeElement.AgilityNode = node;
			Nodes.Add(nodeElement);
		}

		private void ProcessNode(HtmlNode n) {
			// Do smart things

		}
	}
}

