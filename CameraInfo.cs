using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rtsp_camera_viewer
{
    public class CameraInfo
    {
        public CameraInfo(string Addr, int Rtat)
        {
            Rotate = Rtat;
            Address = Addr;
        }
        public string Address;
        public int Rotate;
    }
}
