using System;

namespace HTMLParser
{
	public class HTMLFileParser
	{
		public string FilePath { get; set; };

		public HTMLFileParser(){
			HTMLFileParser(String.Empty);
		}

		public HTMLFileParser(string filePath) {
			FilePath = filePath;
		}

		public bool Open(){
			Open(FilePath);
		}

		public bool Open(string filePath){

		}

		public bool Fix(){

		}

		public bool Save() {
			Save(makeBackup:false);
		}

		public bool Save(bool makeBackup) {

		}
	}
}

