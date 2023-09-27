using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adapter
{
    public class Logger
    {
        public enum LogLevel{
            Dump,
            Debug,
            Info,
            Warning,
            Error
        }
        private LogLevel _level;
        public Logger(Config config){
            _level = config.LogLevel;
        }
        public void Log(String message,LogLevel level){
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] {message}");
            File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] {message}\n");
        }
    }
}