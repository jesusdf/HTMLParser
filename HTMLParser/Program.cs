using System;

namespace HTMLParser
{
	class MainClass
	{
		public static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("You must provide a file to parse!");
			} else {
				HTMLFileParser f = new HTMLFileParser(args[0]);
				f.Open();
				f.Fix();
				f.Save(makeBackup: true);
				Console.WriteLine("Done.");
			}
		}
	}
}
