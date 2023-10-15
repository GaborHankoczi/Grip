using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text;

namespace Grip.Bll.Everlink
{
    public class Table
    {
        private const string COLUMN_SEPARATOR = "\0";
        private const string ROW_SEPARATOR = "\0";

        public List<string> Headers { get; private set; }
        public List<List<string>> Rows { get; private set; }

        public Table(){
            Headers = new List<string>();
            Rows = new List<List<string>>();
        }


        public async Task LoadFromZip(byte[] zip){
            await ParseZipFile(zip);
        }

        private async Task ParseZipFile(byte[] zip){
            using(ZipArchive archive = new ZipArchive(new System.IO.MemoryStream(zip))){
                StreamReader fieldNamesReader = new StreamReader(archive.Entries.Where(e=>e.FullName=="field_names").First().Open());
                string[] field_names = (await fieldNamesReader.ReadToEndAsync()).Trim('\0').Split('\0');
                foreach(string fieldName in field_names){
                    Headers.Add(fieldName);
                }
                using(var memoryStream = new MemoryStream()){
                    archive.Entries.Where(e=>e.FullName=="result_rows").First().Open().CopyTo(memoryStream);
                    var results = Encoding.GetEncoding("ISO-8859-1").GetString(memoryStream.ToArray());
                    string[] fields = (results).Trim('\0').Split('\0');
                    Rows = Enumerable.Range(0, (int)Math.Ceiling((double)fields.Length / Headers.Count))
                        .Select(i => fields.Skip(i * Headers.Count).Take(Headers.Count).ToList())
                        .ToList();
                }
                
            }
        }
    }
}