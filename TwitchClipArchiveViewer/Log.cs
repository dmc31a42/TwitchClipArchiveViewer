using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClipArchiveViewer
{
#if False
    public class Log
    {
        private static Stream stream;
        private static StreamWriter streamWriter;
        static Log()
        {
            string Date = DateTime.Now.ToString("yyyy-MM-dd");
            int i = 0;
            while(File.Exists($"{Date}-{i}.log"))
            {
                i++;
            }
            stream = new FileStream($"{Date}-{i}.log", FileMode.Create);
            streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
        }
        public static Stream Stream
        {
            get
            {
                return stream;
            }
        }
        public static StreamWriter StreamWriter
        {
            get
            {
                return streamWriter;
            }
        }

    }
#endif
}
