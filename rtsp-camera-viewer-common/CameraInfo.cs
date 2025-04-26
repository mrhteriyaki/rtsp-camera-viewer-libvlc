using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rtsp_camera_viewer_common
{
    public class CameraInfo
    {
        public CameraInfo(string Addr, int Rtat = 0)
        {
            Rotate = Rtat;
            Address = Addr;
        }
        public string Address;
        public int Rotate;

        //public static bool[] rotateList;

        public List<string> GetVlcArgs()
        {
            List<string> ArgsList = [];

            //set vlc attributes per os.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ArgsList.Add("--rtsp-tcp");
                ArgsList.Add("--aout=directsound");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //
            }

            if (Rotate != 0) //Rotate in use, enable transform.
            {
                ArgsList.Add("--video-filter=transform");
                if (Rotate == 90)
                {
                    ArgsList.Add("--transform-type=90");
                    //rotateList[Index] = true;
                }
                else if (Rotate == 180)
                {
                    ArgsList.Add("--transform-type=180");
                }
                else if (Rotate == 270)
                {
                    ArgsList.Add("--transform-type=270");
                    //rotateList[Index] = true;
                }
            }
            return ArgsList;
        }

        public bool GetRotateBlock()
        {
            if (Rotate == 90 || Rotate == 270)
            {
                return true;
            }
            return false;
        }
    }

}
