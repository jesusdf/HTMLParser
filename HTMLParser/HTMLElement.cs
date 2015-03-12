using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;
using HtmlAgilityPack;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace HTMLParser
{
    /// <summary>
    /// Handles HTML Element transformation
    /// </summary>
    public class HTMLElement : IDisposable
    {

        //#if DEBUG
        private const string TableTagReplacement = @"div";
        private const string TrTagReplacement = @"div";
        private const string TdTagReplacement = @"div";
        private const string BootStrapAttributeName = @"class";
        private const string BootStrapAttributeValue = @"col-xs-{0}";
        /// <summary>
        /// Indicates an HTML XPath selector for the root element
        /// </summary>
        public const string HtmlRootTagSelector = @"//body";
        /// <summary>
        /// Indicates the HTML Tag of the root element
        /// </summary>
        public const string HtmlRootTag = @"body";
        //#else
        //private const string TableTagReplacement = @"flex:Layout";
        //private const string TrTagReplacement = @"flex:Row";
        //private const string TdTagReplacement = @"flex:Col";
        //private const string BootStrapAttributeName = @"xs";
        //private const string BootStrapAttributeValue = @"{0}";
        ///// <summary>
        ///// Indicates an HTML XPath selector for the root element
        ///// </summary>
        //public const string HtmlRootTagSelector = @"//asp:Content";
        ///// <summary>
        ///// Indicates the HTML Tag of the root element
        ///// </summary>
        //public const string HtmlRootTag = @"asp:Content";
        //#endif
        #region Private variables
        // Should we reassign the available space to every column after first measurement?
        private const bool REASSIGN_AVAILABLE = true;
        // Should we calculate element values independently per row or as a whole table?
        private bool VALUES_BY_ROW = true;
        private const int MAX_WIDTH = 760;
        private const int DEFAULT_WIDTH = 50;
        private const int DEFAULT_CONTROL_WIDTH = 300;
        private const int DEFAULT_COLUMNS = 12;
        //private const decimal PER_COLUMN_WIDTH = (decimal)MAX_WIDTH / (decimal)DEFAULT_COLUMNS;
        private const string TableTag = @"table";
        private const string TrTag = @"tr";
        private const string TdTag = @"td";
        private const string InputTag = @"input";
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
        #region Helper Classes
        /// <summary>
        /// Stores column width information
        /// </summary>
        protected class ColumnInfo
        {
            /// <summary>
            /// Column list
            /// </summary>
            public List<int> Items;

            /// <summary>
            /// Number of columns
            /// </summary>
            public int Count
            {
                get
                {
                    return Items.Count();
                }
                set
                {
                    List<int> l = new List<int>(value);
                    for (int i = 0; i < value; i++)
                    {
                        if (i < Items.Count())
                        {
                            l.Add(Items[0]);
                        }
                        else
                        {
                            l.Add(0);
                        }
                    }
                    Items = l;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public ColumnInfo()
            {
                Items = new List<int>();
            }

            /// <summary>
            /// Gets debug information
            /// </summary>
            /// <returns></returns>
            public string Describe()
            {
                StringBuilder sb = new StringBuilder();
                foreach (int i in Items)
                {
                    sb.Append(String.Format("{0} ", i));
                }
                return sb.ToString().TrimEnd();
            }
        }
        #endregion
        #region Properties
        /// <summary>
        /// Indicates whether the tag has columns or not.
        /// </summary>
        protected bool hasColumns { get; set; }

        /// <summary>
        /// HTML original Tag
        /// </summary>
        protected string Tag { get; set; }

        /// <summary>
        /// Column list
        /// </summary>
        protected ColumnInfo Columns { get; set; }

        /// <summary>
        /// Nesting level (used to tabulate text in debug information)
        /// </summary>
        protected int NestingLevel { get; set; }

        /// <summary>
        /// Available space left on an element after first calculation
        /// </summary>
        protected int AvailableWidth { get; set; }

        /// <summary>
        /// Width in pixels
        /// </summary>
        protected int Width { get; set; }

        /// <summary>
        /// Width in BootStrap column number (usually 1 to 12)
        /// </summary>
        protected int Value { get; set; }

        /// <summary>
        /// HtmlAgilityPack original object (HtmlNode)
        /// </summary>
        protected HtmlNode AgilityNode { get; set; }

        /// <summary>
        /// Reference to the parent element
        /// </summary>
        protected HTMLElement Parent { get; set; }

        /// <summary>
        /// List of children elements
        /// </summary>
        protected List<HTMLElement> ChildElements { get; set; }
        #endregion
        #region Methods
        /// <summary>
        /// Constructor
        /// </summary>
        public HTMLElement()
        {
            ChildElements = new List<HTMLElement>();
            Parent = null;
            Value = DEFAULT_COLUMNS;
            Width = MAX_WIDTH;
            AvailableWidth = Width;
            Columns = new ColumnInfo();
            hasColumns = false;
            NestingLevel = -1;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public void Dispose()
        {
            ChildElements = null;
            Parent = null;
        }

        /// <summary>
        /// Appends an HTML node.
        /// </summary>
        /// <param name="node">Node</param>
        public void AppendNode(HtmlNode node)
        {
            if (IsValidNodeType(node))
            {
                ChildElements.Add(new HTMLElement());
                HTMLElement nodeElement = this.ChildElements.Last();
                nodeElement.Parent = this;
                nodeElement.NestingLevel = this.NestingLevel + 1;
                nodeElement.AgilityNode = node;
                nodeElement.ApendChildNodes();
                nodeElement.CalculateColumns();
            }
        }

        /// <summary>
        /// We should ignore certain node types.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsValidNodeType(HtmlNode node)
        {
            if (node != null)
            {
                return (
                        (node.GetType() != typeof(HtmlTextNode)) &&
                    (node.GetType() != typeof(HtmlCommentNode))
                       );
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Appends children nodes.
        /// </summary>
        private void ApendChildNodes()
        {
            HtmlNode n = this.AgilityNode;
            if (n != null)
            {
                if (String.IsNullOrWhiteSpace(n.Id))
                {
                    this.Tag = n.Name;
                }
                else
                {
                    this.Tag = String.Format("{0} ({1})", n.Name, n.Id);
                }
                foreach (HtmlNode c in n.ChildNodes)
                {
                    this.AppendNode(c);
                }
            }
        }

        /// <summary>
        /// Calculates sizes and values.
        /// </summary>
        protected void Process()
        {
            // Tries to read or guess actual size in pixels of every element
            this.CalculateSize();
            // Calculate each column width looking in every row
            this.CalculateColumnWidth();
            // Sets the corresponding BootStrap columns based on the previous data
            if (VALUES_BY_ROW)
            {
                this.CalculateValueByRow();
            }
            else
            {
                this.CalculateValueByContainer();
            }
        }

        /// <summary>
        /// Calculates the size in pixels, relative to it's parent.
        /// </summary>
        /// <returns>The size.</returns>
        protected int CalculateSize()
        {
            int calculatedWidth = 0;
            int value = 0;
            // Rows have the same width as its container
            value = this.GetWidth();
            if (value == 0)
            {
                if ((this.Parent != null) && (IsRow(this.AgilityNode)))
                {
                    value = this.Parent.Width;
                }
                else
                {
                    value = GetDefaultWidth(this);
                }
            }
            this.Width = value;
            this.AvailableWidth = this.Width;
            if (this.ChildElements.Any())
            {
                foreach (HTMLElement e in ChildElements)
                {
                    calculatedWidth += e.CalculateSize();
                    this.AvailableWidth = this.Width - calculatedWidth;
                }
            }
            if (calculatedWidth > 0 && calculatedWidth <= MAX_WIDTH && !(IsRow(this.AgilityNode)))
            {
                this.Width = calculatedWidth;
            }
            this.AvailableWidth = this.Width - calculatedWidth;
            if (REASSIGN_AVAILABLE && IsRow(this.AgilityNode))
            {
                // Now that we have an approximate idea of the whole element sizing, I need to reassign the space left.
                ReassignAvailableSpace(this);
            }
            if (this.AvailableWidth < 0)
            {
                this.AvailableWidth = 0;
            }
            return this.Width;
        }

        private int GetWidth()
        {
            HtmlNode n = this.AgilityNode;
            int value = 0;
            if (n != null)
            {
                if (n.Attributes[WidthAttribute] != null)
                {
                    value = ParseWidthAttribute(n.Attributes[WidthAttribute].Value);
                }
                if (n.Attributes[StyleAttribute] != null)
                {
                    value = ParseStyleAttribute(n.Attributes[StyleAttribute].Value);
                }
            }
            return value;
        }

        private void ReassignAvailableSpace(HTMLElement e)
        {
            HtmlNode n = e.AgilityNode;
            int numColumnsSpanned = 0;
            int colSpan = 0;
            int extraWidth = 0;
            int perColAvailableWidth = 0;
            if ((e.ChildElements.Any()) && (n != null))
            {
                numColumnsSpanned = GetNumColumnsSpanned(e);
                if (numColumnsSpanned > 0)
                {
                    perColAvailableWidth = (int)((decimal)e.AvailableWidth / (decimal)numColumnsSpanned);
                }
                else
                {
                    if (e.Columns.Count > 0)
                    {
                        perColAvailableWidth = (int)((decimal)e.AvailableWidth / (decimal)e.Columns.Count);
                    }
                }
                foreach (HTMLElement c in e.ChildElements)
                {
                    HtmlNode nc = c.AgilityNode;
                    if (IsColumn(nc))
                    {
                        if (nc.Attributes[ColSpanAttribute] != null)
                        {
                            colSpan = Int32.Parse(nc.Attributes[ColSpanAttribute].Value);
                        }
                        else
                        {
                            colSpan = (numColumnsSpanned > 0 ? 0 : 1);
                        }
                        extraWidth = (perColAvailableWidth * colSpan);
                        c.Width += extraWidth;
                        c.AvailableWidth -= extraWidth;
                        if (c.AvailableWidth < 0)
                        {
                            c.AvailableWidth = 0;
                        }
                        e.AvailableWidth -= extraWidth;
                        if (e.AvailableWidth < 0)
                        {
                            e.AvailableWidth = 0;
                        }
                    }
                    ReassignAvailableSpace(c);
                }
            }
        }

        private int GetNumColumnsSpanned(HTMLElement e)
        {
            int total = 0;
            foreach (HTMLElement c in e.ChildElements)
            {
                HtmlNode nc = c.AgilityNode;
                if (nc.Attributes[ColSpanAttribute] != null)
                {
                    total += Int32.Parse(nc.Attributes[ColSpanAttribute].Value);
                }
            }
            return total;
        }

        private int GetDefaultWidth(HTMLElement e)
        {
            HtmlNode n = e.AgilityNode;
            int value = 0;
            if (n != null)
            {
                switch (n.Name.ToLower())
                {
                    case HtmlRootTag:
                        value = MAX_WIDTH;
                        break;
                    case InputTag:
                        value = DEFAULT_CONTROL_WIDTH;
                        break;
                    default:
                        if (e.Parent != null)
                        {
                            if (e.Parent.AvailableWidth < DEFAULT_WIDTH)
                            {
                                value = e.Parent.AvailableWidth;
                            }
                            else
                            {
                                if (e.Parent.Width != 0 && !IsRow(e.Parent.AgilityNode) && !IsContainer(e.Parent.AgilityNode))
                                {
                                    value = e.Parent.Width;
                                }
                                else
                                {
                                    value = DEFAULT_WIDTH;
                                }
                            }
                        }
                        else
                        {
                            value = DEFAULT_WIDTH;
                        }
                        break;
                }
            }
            return value;
        }

        /// <summary>
        /// Parses the width attribute.
        /// </summary>
        /// <returns>The width in pixels</returns>
        /// <param name="s">Width attribute value</param>
        private int ParseWidthAttribute(string s)
        {
            int value = 0;
            if (s.ToLower().Contains(Percent))
            {
                value = (int)((decimal)this.Parent.Width * (decimal)(Decimal.Parse(s.Replace(Percent, String.Empty)) / (decimal)100));
            }
            else
            {
                value = Int32.Parse(s.Replace(Pixels, String.Empty));
            }
            return value;
        }

        /// <summary>
        /// Parses the style attribute.
        /// </summary>
        /// <returns>The width in pixels</returns>
        /// <param name="s">Style attribute value</param>
        private int ParseStyleAttribute(string s)
        {
            string style = s.ToLower().Replace(Espace, String.Empty).Replace(Tabulator, String.Empty);
            if (style.Contains(StyleWidth))
            {
                style = style.Substring(style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase) + StyleWidth.Length);
                if (style.Contains(Semicolon))
                {
                    style = style.Substring(0, style.IndexOf(Semicolon, StringComparison.OrdinalIgnoreCase));
                }
                return ParseWidthAttribute(style);
            }
            return 0;
        }

        /// <summary>
        /// Calculates the number of BootStrap columns per row
        /// </summary>
        protected void CalculateValueByRow()
        {
            HtmlNode n = this.AgilityNode;
            int value = 0;
            int calculatedValue = 0;
            if (this.ChildElements.Count > 0)
            {
                value = CalculateSingleValueByRow();
                this.Value = value;
                foreach (HTMLElement c in this.ChildElements)
                {
                    c.CalculateValueByRow();
                    if (IsColumn(c.AgilityNode))
                    {
                        calculatedValue += c.Value;
                    }
                }
                calculatedValue -= DEFAULT_COLUMNS;
                if (calculatedValue > 0)
                {
                    // We have exceeded the maximum number of columns, step back from the first column
                    for (int i = 0; i < this.ChildElements.Count && calculatedValue > 0; i++)
                    {
                        HTMLElement c = this.ChildElements[i];
                        if (c.Value > 1)
                        {
                            c.Value--;
                            calculatedValue--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the number of BootStrap columns per container
        /// </summary>
        protected void CalculateValueByContainer()
        {
            HtmlNode n = this.AgilityNode;
            int calculatedValue = 0;
            int i = 0;
            if (this.ChildElements.Any())
            {
                foreach (HTMLElement tr in this.ChildElements)
                {
                    if (IsContainer(n))
                    {
                        i = 0;
                        calculatedValue = 0;
                        HtmlNode nc = tr.AgilityNode;
                        if (nc != null)
                        {
                            if (IsRow(nc))
                            {
                                if (tr.ChildElements.Any())
                                {
                                    foreach (HTMLElement td in tr.ChildElements)
                                    {
                                        if (IsColumn(td.AgilityNode))
                                        {
                                            td.Value = CalculateSingleValueByContainer(td, i++);
                                            calculatedValue += td.Value;
                                        }
                                    }
                                    calculatedValue -= DEFAULT_COLUMNS;
                                    if (calculatedValue > 0)
                                    {
                                        // We have exceeded the maximum number of columns, step back from the last column
                                        for (int j = tr.ChildElements.Count - 1; j > 0 && calculatedValue > 0; j--)
                                        {
                                            HTMLElement c = tr.ChildElements[j];
                                            if (c.Value > 1)
                                            {
                                                c.Value--;
                                                calculatedValue--;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    tr.CalculateValueByContainer();
                }
            }
        }

        /// <summary>
        /// Calculates a single element Value per row.
        /// </summary>
        /// <returns>The single value.</returns>
        private int CalculateSingleValueByRow()
        {
            if (this.Parent != null)
            {
                int parentWidth = (this.Parent.Width > 0 ? this.Parent.Width : 1);
                decimal percent = ((decimal)this.Width / (decimal)parentWidth);
                decimal calculatedCols = (decimal)DEFAULT_COLUMNS * percent;
                //int parentAvailable = (this.Parent.AvailableValue > 0 ? this.Parent.AvailableValue : 1);
                //return ValueRange((int)Math.Round((decimal)this.Width / ((decimal)parentWidth / (decimal)parentAvailable), 0, MidpointRounding.AwayFromZero));
                return ValueRange((int)Math.Round(calculatedCols, 0, MidpointRounding.AwayFromZero));
            }
            else
            {
                //return ValueRange((int)Math.Round((decimal)this.Width / PER_COLUMN_WIDTH, 0, MidpointRounding.AwayFromZero));
                return DEFAULT_COLUMNS;
            }
        }

        /// <summary>
        /// Calculates a single element Value per container.
        /// </summary>
        /// <returns>The single value.</returns>
        private int CalculateSingleValueByContainer(HTMLElement e, int columnNumber)
        {
            int parentWidth = (this.Width > 0 ? this.Width : 1);
            decimal percent = ((decimal)this.Columns.Items[columnNumber] / (decimal)parentWidth);
            decimal calculatedCols = (decimal)DEFAULT_COLUMNS * percent;
            //int parentAvailable = (this.Parent.AvailableValue > 0 ? this.Parent.AvailableValue : 1);
            //return ValueRange((int)Math.Round((decimal)this.Width / ((decimal)parentWidth / (decimal)parentAvailable), 0, MidpointRounding.AwayFromZero));
            return ValueRange((int)Math.Round(calculatedCols, 0, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Bounds check to DEFAULT_COLUMNS.
        /// </summary>
        /// <returns>The range.</returns>
        /// <param name="value">Value.</param>
        private static int ValueRange(int value)
        {
            if (value >= DEFAULT_COLUMNS)
            {
                value = DEFAULT_COLUMNS;
            }
            if (value <= 0)
            {
                value = 1;
            }
            return value;
        }

        /// <summary>
        /// Looks for colspan attribute.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int GetColumns(HtmlNode n)
        {
            if (n != null)
            {
                foreach (HtmlAttribute a in n.Attributes)
                {
                    switch (a.Name.ToLower())
                    {
                        case ColSpanAttribute:
                            return Int32.Parse(a.Value);
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// Calculates the number of columns from its child elements
        /// </summary>
        private void CalculateColumns()
        {
            HtmlNode n = this.AgilityNode;
            int columnNumber = 0;
            if (n == null)
            {
                this.hasColumns = false;
                this.Columns.Count = 1;
            }
            else
            {
                this.hasColumns = HasColumns(n);
                if (this.hasColumns)
                {
                    if (this.ChildElements.Any())
                    {
                        foreach (HTMLElement e in this.ChildElements)
                        {
                            if (e.AgilityNode != null)
                            {
                                if (IsColumn(e.AgilityNode))
                                {
                                    columnNumber += e.Columns.Count;
                                }
                                else
                                {
                                    if (columnNumber < e.Columns.Count)
                                    {
                                        columnNumber = e.Columns.Count;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        columnNumber = GetColumns(this.AgilityNode);
                    }
                }
                else
                {
                    columnNumber = GetColumns(this.AgilityNode);
                }
                if (this.Columns.Count < columnNumber)
                {
                    this.Columns.Count = columnNumber;
                    if (this.Parent != null)
                    {
                        this.Parent.CalculateColumns();
                    }
                }
            }
        }

        /// <summary>
        /// Parses columns to get the real width based on maximum width per row.
        /// </summary>
        private void CalculateColumnWidth()
        {
            HtmlNode n = this.AgilityNode;
            int i = 0;
            if (this.ChildElements.Any())
            {
                foreach (HTMLElement tr in this.ChildElements)
                {
                    if (IsContainer(n))
                    {
                        i = 0;
                        HtmlNode nc = tr.AgilityNode;
                        if (nc != null)
                        {
                            if (IsRow(nc))
                            {
                                if (tr.ChildElements.Any())
                                {
                                    foreach (HTMLElement td in tr.ChildElements)
                                    {
                                        if (IsColumn(td.AgilityNode))
                                        {
                                            if (this.Columns.Items[i] < td.Width)
                                            {
                                                this.Columns.Items[i] = td.Width;
                                            }
                                            i += td.Columns.Count;
                                        }
                                    }
                                }
                            }
                        }
                        ScalateColumnWidths();
                    }
                    tr.CalculateColumnWidth();
                }
            }
        }

        /// <summary>
        /// Scalates the column widths if they are greater than the maximum.
        /// </summary>
        private void ScalateColumnWidths()
        {
            int currentSize = 0;
            decimal scaleFactor = 0;
            if (IsContainer(this.AgilityNode))
            {
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    currentSize += this.Columns.Items[i];
                }
                if (currentSize > MAX_WIDTH)
                {
                    scaleFactor = (decimal)MAX_WIDTH / (decimal)currentSize;
                    for (int i = 0; i < this.Columns.Count; i++)
                    {
                        this.Columns.Items[i] = (int)Math.Round((decimal)this.Columns.Items[i] * scaleFactor, 0, MidpointRounding.AwayFromZero);
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the node has columns or not.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private bool HasColumns(HtmlNode n)
        {
            if (n != null)
            {
                switch (n.Name.ToLower())
                {
                    case TableTag:
                    case TrTag:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the node is a row container or not.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private bool IsContainer(HtmlNode n)
        {
            if (n != null)
            {
                switch (n.Name.ToLower())
                {
                    case TableTag:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the node may ocuppy more than one column (colspan).
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private bool IsColumn(HtmlNode n)
        {
            if (n != null)
            {
                switch (n.Name.ToLower())
                {
                    case TdTag:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the node is a row or not.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private bool IsRow(HtmlNode n)
        {
            if (n != null)
            {
                switch (n.Name.ToLower())
                {
                    case TrTag:
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Describes all the elements in the console.
        /// </summary>
        public void Describe(StringBuilder sb)
        {
            if (this.Parent == null)
            {
                this.Process();
            }
            if (this.NestingLevel >= 0)
            {
                sb.AppendFormat("{0}{1} => {2}", new String('\t', this.NestingLevel), this.Tag, GetDescription());
            }
            foreach (HTMLElement e in ChildElements)
            {
                e.Describe(sb);
            }
        }

        private string GetDescription()
        {
            return String.Format("HasColumns: {0}, Columns: {1}{2}, Width: {3}, AvailableWidth: {4}, Value: {5}\r\n", this.hasColumns, this.Columns.Count, IsContainer(this.AgilityNode) ? String.Format(" ({0})", this.Columns.Describe()) : String.Empty, this.Width, this.AvailableWidth, this.Value);
        }

        /// <summary>
        /// Changes some of the original elements into others and ammends properties
        /// </summary>
        public void Fix()
        {
            if (this.Parent == null)
            {
                this.Process();
            }
            if (this.ChildElements.Any())
            {
                foreach (HTMLElement e in ChildElements)
                {
                    HtmlNode n = e.AgilityNode;
                    if (n != null)
                    {
                        switch (n.Name.ToLower())
                        {
                            case TableTag:
                                //#if DEBUG
                                // Add all its children to the parent node and remove it
                                foreach (HtmlNode c in n.ChildNodes)
                                {
                                    n.ParentNode.ChildNodes.Append(c);
                                }
                                n.Remove();
                                //#else
                                //n.Name = TableTagReplacement;
                                //CleanAttributes(n);
                                //#endif
                                break;
                            case TrTag:
                                n.Name = TrTagReplacement;
                                CleanAttributes(n);
                                //#if DEBUG
                                n.Attributes.Add("class", "row");
                                //#endif
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
        private static void CleanWidthAttributes(HTMLElement e)
        {
            HtmlNode n = e.AgilityNode;
            if (n != null)
            {
                HtmlAttribute a = null;
                int i = 0;
                while (i < n.Attributes.Count)
                {
                    a = n.Attributes[i];
                    switch (a.Name.ToLower())
                    {
                        case WidthAttribute:
                            a.Remove();
                            break;
                        case StyleAttribute:
                            string style = a.Value;
                            int s = 0;
                            s = style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase);
                            if (s >= 0)
                            {
                                int ss = 0;
                                ss = style.IndexOf(Semicolon, s, StringComparison.OrdinalIgnoreCase);
                                if (ss >= 0)
                                {
                                    style = String.Format("{0}{1}", style.Substring(0, s), style.Substring(style.IndexOf(Semicolon, s, StringComparison.OrdinalIgnoreCase) + 1));
                                }
                                else
                                {
                                    style = style.Substring(0, style.IndexOf(StyleWidth, StringComparison.OrdinalIgnoreCase));
                                }
                                if (String.IsNullOrWhiteSpace(style))
                                {
                                    a.Remove();
                                }
                                else
                                {
                                    a.Value = style;
                                    i++;
                                }
                            }
                            else
                            {
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
        private static void CleanAttributes(HtmlNode n)
        {
            while (n.Attributes.Any())
            {
                n.Attributes[0].Remove();
            }
        }

        /// <summary>
        /// Sets the BootStrap column number from the Element Value property
        /// </summary>
        /// <param name="e">Element</param>
        private static void SetColAttributes(HTMLElement e)
        {
            HtmlNode n = e.AgilityNode;
            if (n != null)
            {
                n.Attributes.Add(BootStrapAttributeName, String.Format(BootStrapAttributeValue, e.Value));
            }
        }
        #endregion
    }
}

