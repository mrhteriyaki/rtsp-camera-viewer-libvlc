using LibVLCSharp.Shared;
using System.Runtime.InteropServices;

namespace rtsp_camera_viewer_common
{
    public class ConfigLoader
    {
        public static List<CameraInfo> CameraSourceList = [];

        public static int MaxColumns = 3;
        public static bool AudioEnabled = false;
        public static bool FullScreenSize = false;
        

        public static void LoadStreamList()
        {
            if (File.Exists("sqlconfig.ini"))
            {
                //Use SQL lookup.
                CameraSourceList = SQLFunctions.GetCameraList();
                if (File.Exists("config.ini"))
                {
                    foreach (string cl in File.ReadLines("config.ini"))
                    {
                        //Get Camera Column setting.
                        if (cl.StartsWith("maxcols="))
                        {
                            MaxColumns = int.Parse(cl.Substring(8));
                        }
                    }
                }
            }
            else if (File.Exists("config.ini"))
            {
                //Load camera list from config.ini.

                foreach (string cl in File.ReadLines("config.ini"))
                {
                    if (cl.StartsWith("camera="))
                    {
                        CameraSourceList.Add(new CameraInfo(cl.Substring(7)));
                    }

                    if (cl.StartsWith("maxcols="))
                    {
                        MaxColumns = int.Parse(cl.Substring(8));
                    }
                }
            }
            else
            {
                throw new Exception("No config for video sources.");
            }
        }

        public static void VlcInit()
        {
            if (Directory.Exists(@"C:\Program Files\VideoLAN\VLC"))
            {
                Core.Initialize(@"C:\Program Files\VideoLAN\VLC");
            }
            else
            {
                Core.Initialize();
            }
        }


        public static int GetCellCount()
        {
            //Account for spaces used by rotate.
            int cellcount = CameraSourceList.Count;
            foreach (var cinfo in CameraSourceList)
            {
                if (cinfo.GetRotateBlock())
                {
                    cellcount = cellcount + 3; //Rotated feeds use 3 cells.
                }
            }
            return cellcount;
        }

        //Return rows for screen.
        public static int GetRowCount()
        {
            int rows = (int)Math.Ceiling(GetCellCount() / (double)MaxColumns);
            return rows;
        }

        
       

    }
}
