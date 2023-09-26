using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text;

namespace Adapter.EverlinkProtocol
{
    public class Table
    {
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
                var field_names_buffer = new byte[1024];
                await archive.Entries.Where(e=>e.FullName=="field_names").First().Open().ReadAsync(field_names_buffer,0,1024);                
                Encoding.UTF8.GetString(field_names_buffer).Trim('\0').Split('\0').ToList().ForEach(h=>Headers.Add(h));
            }
        }
    }
}