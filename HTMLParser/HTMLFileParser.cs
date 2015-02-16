using System;

namespace HTMLParser
{
	public class HTMLFileParser
	{
		public string FilePath { get; set; }

		public HTMLFileParser (string filePath)
		{
			FilePath = filePath;
		}

		public bool Open ()
		{
			return Open (FilePath);
		}

		public bool Open (string filePath)
		{
			return true;
		}

		public bool Fix ()
		{
			return true;
		}

		public bool Save ()
		{
			return Save (makeBackup: false);
		}

		public bool Save (bool makeBackup)
		{
			return true;
		}
	}
}

