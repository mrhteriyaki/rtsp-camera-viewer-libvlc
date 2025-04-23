using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rtsp_camera_viewer
{
    public static class Logger
    {
        static bool newLog = true;

        public static void LogMessage(string message)
        {
            if(newLog)
            {
                File.Delete("log.log");
                newLog = false;
            }
            StreamWriter SW = new StreamWriter("log.log",true);
            SW.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message);
            SW.Close();


        }



    }
}
