using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLParser
{
	public class HTMLElement
	{

        private const int DEFAULT_COLUMNS = 12;

		public HTMLElement() {
            constructor(parent: null, width: 0);
		}
        public HTMLElement(HTMLElement parent) {
            constructor(parent: parent, width: 0);
        }
        public HTMLElement(HTMLElement parent, int width) {
            constructor(parent: parent, width: width);
        }

        public void constructor(HTMLElement parent, int width)
        {
            Parent = parent;
            Width = width;
        }

		public string Tag { get; set; }
		public string Value { get; set; }
        public int Width { get; set; }
        public HTMLElement Parent { get; set; }
		public List<HTMLElement> Nodes { get; set; }

	}
}

