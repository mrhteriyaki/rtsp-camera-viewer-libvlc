using LibVLCSharp.Shared;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace rtsp_camera_viewer_common
{
    public class ConfigLoader
    {
        public static List<CameraInfo> CameraSourceList = [];

        public static int MaxColumns = 3;
        public static bool AudioEnabled = false;
        public static bool FullScreenSize = false;

        public static int LocXStart = 0;
        public static int LocYStart = 35;

        public static void LoadStreamList()
        {
            if (File.Exists(ConfigConstant.str_cf_file))
            {
                //Load camera list from config.ini.
                bool sqlusage = false;
                string sqlconn = "";
                string sqlquery = "";

                foreach (string cl in File.ReadLines(ConfigConstant.str_cf_file))
                {
                    if (cl.StartsWith(ConfigConstant.str_camera))
                    {
                        CameraSourceList.Add(new CameraInfo(GetValue(cl)));
                    }
                    else if (cl.StartsWith(ConfigConstant.str_maxcols))
                    {
                        MaxColumns = int.Parse(GetValue(cl));
                    }
                    else if (cl.StartsWith(ConfigConstant.str_connstring))
                    {
                        sqlusage = true;
                        sqlconn = GetValue(cl);
                    }
                    else if (cl.StartsWith(ConfigConstant.str_sqlquery))
                    {
                        sqlquery = GetValue(cl);
                    }
                    else if (cl.StartsWith(ConfigConstant.str_overlap))
                    {
                        if(GetValue(cl) == "1")
                        {
                            LocYStart = 0;
                        }
                    }
                }

                if (sqlusage)
                {
                    CameraSourceList = SQLFunctions.GetCameraList(sqlconn, sqlquery);
                }
            }
            else
            {
                ConfigLoader.OpenSettingsApp();
            }
        }

        public static string GetValue(string raw)
        {
            return raw.Substring(raw.IndexOf("=") + 1);
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

        public static void OpenSettingsApp()
        {
            string runningPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string exePath = Path.Join(Path.GetDirectoryName(runningPath), "viewer-settings.exe");

            if (File.Exists(exePath))
            {
                Process.Start(exePath);
            }
            else
            {
                throw new Exception($"Missing File at {exePath}");
            }
        }


    }
}
