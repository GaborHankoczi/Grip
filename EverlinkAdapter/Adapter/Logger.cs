using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if(level >= _level){
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] {message}");
                File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] {message}\n");
            }
        }

        public void LogHex(byte[] bytes,LogLevel level, int bytesPerLine = 16){
            if(level >= _level){
                if (bytes == null) 
                    return;
                int bytesLength = bytes.Length;

                char[] HexChars = "0123456789ABCDEF".ToCharArray();

                int firstHexColumn =
                    8                   // 8 characters for the address
                    + 3;                  // 3 spaces

                int firstCharColumn = firstHexColumn
                    + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                    + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                    + 2;                  // 2 spaces 

                int lineLength = firstCharColumn
                    + bytesPerLine           // - characters to show the ascii value
                    + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

                char[] line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
                int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
                StringBuilder result = new StringBuilder(expectedLines * lineLength);

                for (int i = 0; i < bytesLength; i += bytesPerLine)
                {
                    line[0] = HexChars[(i >> 28) & 0xF];
                    line[1] = HexChars[(i >> 24) & 0xF];
                    line[2] = HexChars[(i >> 20) & 0xF];
                    line[3] = HexChars[(i >> 16) & 0xF];
                    line[4] = HexChars[(i >> 12) & 0xF];
                    line[5] = HexChars[(i >> 8) & 0xF];
                    line[6] = HexChars[(i >> 4) & 0xF];
                    line[7] = HexChars[(i >> 0) & 0xF];

                    int hexColumn = firstHexColumn;
                    int charColumn = firstCharColumn;

                    for (int j = 0; j < bytesPerLine; j++)
                    {
                        if (j > 0 && (j & 7) == 0) hexColumn++;
                        if (i + j >= bytesLength)
                        {
                            line[hexColumn] = ' ';
                            line[hexColumn + 1] = ' ';
                            line[charColumn] = ' ';
                        }
                        else
                        {
                            byte b = bytes[i + j];
                            line[hexColumn] = HexChars[(b >> 4) & 0xF];
                            line[hexColumn + 1] = HexChars[b & 0xF];
                            line[charColumn] = asciiSymbol( b );
                        }
                        hexColumn += 3;
                        charColumn++;
                    }
                    result.Append(line);
                }
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] HEX DUMP:");
                Console.WriteLine(result.ToString());
                File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}]  HEX DUMP:\n");
                File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{result.ToString()}\n");
            }
        }  
        static char asciiSymbol( byte val )
        {
            if( val < 32 ) return '.';  // Non-printable ASCII
            if( val < 127 ) return (char)val;   // Normal ASCII
            // Handle the hole in Latin-1
            if( val == 127 ) return '.';
            if( val < 0x90 ) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[ val & 0xF ];
            if( val < 0xA0 ) return ".‘’“”•–—˜™š›œ.žŸ"[ val & 0xF ];
            if( val == 0xAD ) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char)val;   // Normal Latin-1
        }

        public void LogHex(byte[] bytes,LogLevel level, int bytesPerLine = 16){
            if (bytes == null) 
                return;
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = asciiSymbol( b );
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}] HEX DUMP:");
            Console.WriteLine(result.ToString());
            File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{DateTime.Now.ToString("HH:mm:ss")} [{level.ToString()}]  HEX DUMP:\n");
            File.AppendAllText($"./data/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.log",$"{result.ToString()}\n");
        }  
        static char asciiSymbol( byte val )
        {
            if( val < 32 ) return '.';  // Non-printable ASCII
            if( val < 127 ) return (char)val;   // Normal ASCII
            // Handle the hole in Latin-1
            if( val == 127 ) return '.';
            if( val < 0x90 ) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[ val & 0xF ];
            if( val < 0xA0 ) return ".‘’“”•–—˜™š›œ.žŸ"[ val & 0xF ];
            if( val == 0xAD ) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char)val;   // Normal Latin-1
        }
    }
}