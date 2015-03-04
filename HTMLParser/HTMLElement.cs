using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;
using HtmlAgilityPack;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace HTMLParser
{
	public class HTMLElement : IDisposable
	{
		#region Private variables
		private const int MAX_WIDTH = 760;
		private const int DEFAULT_COLUMNS = 12;
		private HtmlNode _agilityNode = null;
		private HTMLElement _parent = null;
		#endregion
		#region Public constants
		public const string HtmlRootTagSelector = @"//table";
		public const string HtmlRootTag = @"table";
		private const string TableTag = @"table";
		private const string TrTag = @"tr";
		private const string TdTag = @"td";
		#if DEBUG
		private const string TableTagReplacement = @"div";
		private const string TrTagReplacement = @"div";
		private const string TdTagReplacement = @"div";
		private const string BootStrapAttributeName = @"class";
		private const string BootStrapAttributeValue = @"col-xs-{0}";
		#else
		private const string TableTagReplacement = @"flex:Layout";
		private const string TrTagReplacement = @"flex:Row";
		private const string TdTagReplacement = @"flex:Col";
		private const string BootStrapAttributeName = @"xs";
		private const string BootStrapAttributeValue = @"{0}";
		
		
		
		#endif
		private const string StyleWidth = @"width:";
		private const string Semicolon = @";";
		private const string Espace = @" ";
		private const string Tabulator = "\t";
		private const string Percent = @"%";
		private const string Pixels = @"px";
		private const string WidthAttribute = @"width";
		private const string StyleAttribute = @"style";
		private const string ColSpanAttribute = @"colspan";
		#endregion
		#region Properties
		protected bool hasColumns { get; set; }

		protected string Tag { get; set; }

		protected int Columns { get; set; }

		protected int NestingLevel { get; set; }

		protected int Width { get; set; }

		protected string Value { get; set; }

		protected HtmlNode AgilityNode { 
			get {
				return _agilityNode;
			}
			set {
				_agilityNode = value;
				// Once this Element is linked with an HTML Node, I process it to extract all information I need.
				ProcessNode(_agilityNode);
			}
		}

		protected HTMLElement Parent { 
			get {
				return _parent;
			} 
			set {
				_parent = value;
			}
		}

		protected List<HTMLElement> ChildElements { get; set; }
		#endregion
		#region Methods
		public HTMLElement() {
			ChildElements = new List<HTMLElement>();
			Parent = null;
			Value = DEFAULT_COLUMNS.ToString();
			Width = MAX_WIDTH;
			Columns = 0;
			hasColumns = false;
			NestingLevel = 0;
		}

		public void Dispose() {
			ChildElements = null;
			Parent = null;
		}

		public void AppendChildElement(HTMLElement child) {
			Columns += child.Columns;
			ChildElements.Add(child);
		}

		/// <summary>
		/// Appends an HTML node.
		/// </summary>
		/// <param name="node">Node</param>
		public void AppendNode(HtmlNode node) {
			HTMLElement nodeElement = new HTMLElement();
			nodeElement.Parent = this;
			nodeElement.NestingLevel = this.NestingLevel + 1;
			nodeElement.CalculateColumns(node);
			RefreshColumns(nodeElement, node);
			nodeElement.AgilityNode = node;
			ChildElements.Add(nodeElement);
		}

		/// <summary>
		/// Calculates sizes, values and appends children nodes.
		/// </summary>
		/// <param name="n">Node to process</param>
		private void ProcessNode(HtmlNode n) {
			if (n != null) {
				this.Tag = n.Name;
				foreach (HtmlNode c in n.ChildNodes) {
					this.AppendNode(c);
				}
				CalculateSize();
				CalculateValue();
			}
		}

		/// <summary>
		/// Calculates the size in pixels, relative to it's parent.
		/// </summary>
		/// <returns>The size.</returns>
		protected int CalculateSize() {
			this.Width = 0;
			if (this.AgilityNode != null) {
				foreach (HtmlAttribute a in this.AgilityNode.Attributes) {
					switch (a.Name.ToLower()) {
						case WidthAttribute:
							// We have a tag with a raw width value (in percent or pixel)
							this.Width = ParseWidthAttribute(a.Value);
							break;
						case StyleAttribute:
							// We have a style tag that may (or may not) have a width value (in percent or pixel)
							int i = ParseStyleAttribute(a.Value);
							if (i != 0) {
								this.Width = i;
							}
							break;
						case ColSpanAttribute:
							// We have a colspan tag, we calculate the percent of space propotional to its parent

							break;
					}
				}
			}
			if (this.Width == 0) {
				this.Width = MAX_WIDTH;
			}
			if (this.ChildElements.Count > 0) {
				int calculatedWidth = 0;
				foreach (HTMLElement e in ChildElements) {
					calculatedWidth += e.CalculateSize();
				}
			}
			return this.Width;
		}

		/// <summary>
		/// Parses the width attribute.
		/// </summary>
		/// <returns>The width in pixels</returns>
		/// <param name="s">Width attribute value</param>
		private int ParseWidthAttribute(string s) {
			if (s.ToLower().Contains(Percent)) {
				return this.Parent.Width * (Int32.Parse(s.Replace(Percent, String.Empty)) / 100);
			} else {
				return Int32.Parse(s.Replace(Pixels, String.Empty));
			}
		}

		/// <summary>
		/// Parses the style attribute.
		/// </summary>
		/// <returns>The width in pixels</returns>
		/// <param name="s">Style attribute value</param>
		private int ParseStyleAttribute(string s) {
			string style = s.ToLower().Replace(Espace, String.Empty).Replace(Tabulator, String.Empty);
			if (style.Contains(StyleWidth)) {
				style = style.Substring(style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase) + StyleWidth.Length);
				if (style.Contains(Semicolon)) {
					style = style.Substring(0, style.IndexOf(Semicolon, StringComparison.OrdinalIgnoreCase));
				}
				return ParseWidthAttribute(style);
			}
			return 0;
		}

		/// <summary>
		/// Calculates the number of BootStrap columns
		/// </summary>
		protected void CalculateValue() {
			if (this.Parent.Width == 0) {
				this.Value = DEFAULT_COLUMNS.ToString();
			} else {
				this.Value = ((Int32)((decimal)DEFAULT_COLUMNS * ((decimal)this.Width / (decimal)this.Parent.Width))).ToString();
			}
			if (Int32.Parse(this.Value) == 0) {
				this.Value = DEFAULT_COLUMNS.ToString();
			}
		}

		/// <summary>
		/// Calculates the number of columns that this Element uses (think in <c><td colspan="2"></td></c>)
		/// </summary>
		protected void CalculateColumns(HtmlNode n) {
			this.Columns = GetColumns(n);
		}

		private static int GetColumns(HtmlNode n) {
			if (n != null) {
				foreach (HtmlAttribute a in n.Attributes) {
					switch (a.Name.ToLower()) {
						case ColSpanAttribute:
							return Int32.Parse(a.Value);
					}
				}
			}
			return 1;
		}

		/// <summary>
		/// Reloads the number of columns from its child elements
		/// </summary>
		private void RefreshColumns(HTMLElement el, HtmlNode node) {
			HtmlNode n = this.AgilityNode;
			if (n == null) {
				this.hasColumns = false;
				this.Columns = 1;
			} else {
				this.hasColumns = (n.Name.ToLower() == TrTag);
				if (this.hasColumns) {
					this.Columns = 0;
					if (node != null) {
						if (node.Name.ToLower() == TdTag) {
							this.Columns = el.Columns;
						}
					}
					foreach (HTMLElement e in ChildElements) {
						if (e.AgilityNode != null) {
							if (e.AgilityNode.Name.ToLower() == TdTag) {
								this.Columns += e.Columns;
							}
						}
					}
				} else {
					this.Columns = 1;
				}
			}
		}

		/// <summary>
		/// Describes all the elements in the console.
		/// </summary>
		public void Describe() {
			Console.WriteLine("{0}{1} => HasColumns: {2} Columns: {3}, Width: {4}, Value: {5}", new String('\t', this.NestingLevel), this.Tag, this.hasColumns, this.Columns, this.Width, this.Value);
			foreach (HTMLElement e in ChildElements) {
				e.Describe();
			}
		}

		/// <summary>
		/// Changes some of the original elements into others and ammends properties
		/// </summary>
		public void Fix() {
			if (ChildElements.Count() > 0) {
				foreach (HTMLElement e in ChildElements) {
					HtmlNode n = e.AgilityNode;
					if (n != null) {
						switch (n.Name.ToLower()) {
							case TableTag:
#if DEBUG
								foreach (HtmlNode c in n.ChildNodes) {
									n.ParentNode.ChildNodes.Append(c);
								}
								n.Remove();
#else
								n.Name = TableTagReplacement;
								CleanAttributes(n);
#endif
								break;
							case TrTag:
								n.Name = TrTagReplacement;
								CleanAttributes(n);
#if DEBUG
								n.Attributes.Add("class", "row");
#endif
								break;
							case TdTag:
								n.Name = TdTagReplacement;
								CleanAttributes(n);
								SetColAttributes(e);
								break;
						}
					}
					CleanWidthAttributes(e);
					e.Fix();
				}
			}
		}

		/// <summary>
		/// Cleans the width attributes
		/// </summary>
		/// <param name="e">Element to clean</param>
		private static void CleanWidthAttributes(HTMLElement e) {
			HtmlNode n = e.AgilityNode;
			if (n != null) {
				HtmlAttribute a = null;
				int i = 0;
				while (i < n.Attributes.Count) {
					a = n.Attributes[i];
					switch (a.Name.ToLower()) {
						case WidthAttribute:
							a.Remove();
							break;
						case StyleAttribute:
							string style = a.Value;
							int s = 0;
							s = style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase);
							if (s >= 0) {
								int ss = 0;
								ss = style.IndexOf(Semicolon, s, StringComparison.OrdinalIgnoreCase);
								if (ss >= 0) {
									style = String.Format("{0}{1}", style.Substring(0, s), style.Substring(style.IndexOf(Semicolon, s, StringComparison.OrdinalIgnoreCase) + 1));
								} else {
									style = style.Substring(0, style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase));
								}
								if (String.IsNullOrWhiteSpace(style)) {
									a.Remove();
								} else {
									a.Value = style;
									i++;
								}
							} else {
								i++;
							}
							break;
						default:
							i++;
							break;
					}
				}
			}
		}

		/// <summary>
		/// Removes all the attributes
		/// </summary>
		/// <param name="n">Node to remove all attributes</param>
		private static void CleanAttributes(HtmlNode n) {
			while (n.Attributes.Count() > 0) {
				n.Attributes[0].Remove();
			}
		}

		/// <summary>
		/// Sets the BootStrap column number from the Element Value property
		/// </summary>
		/// <param name="e">Element</param>
		private static void SetColAttributes(HTMLElement e) {
			HtmlNode n = e.AgilityNode;
			string value;
			if (n != null) {
				if (e.Parent.Columns > 1) {
					value = ((int)((decimal)DEFAULT_COLUMNS * ((decimal)e.Columns / (decimal)e.Parent.Columns))).ToString();
				} else {
					value = e.Value;
				}
				n.Attributes.Add(BootStrapAttributeName, String.Format(BootStrapAttributeValue, value));
			}
		}
		#endregion
	}
}

