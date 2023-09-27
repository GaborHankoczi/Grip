using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Adapter.Logger;

namespace Adapter
{
    public class Config
    {
        public string SignalRURL { get; set; }
        public string EverlinkServer { get; set; }
        public int EverlinkPort { get; set; }
        public string EverlinkUsername { get; set; }
        public string EverlinkPassword { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
    }
}