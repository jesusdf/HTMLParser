using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTMLParser
{
	public class HTMLElement
	{
		public HTMLElement ()
		{
		}

		public string Tag { get; set; }

		public string Value { get; set; }

		public List<HTMLElement> Nodes { get; set; }

	}
}

