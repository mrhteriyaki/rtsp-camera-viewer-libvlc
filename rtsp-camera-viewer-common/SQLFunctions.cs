using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace rtsp_camera_viewer_common
{
    public class SQLFunctions
    {
        public static List<CameraInfo> GetCameraList(string ConnString, string QueryString)
        {
       
            List<CameraInfo> CameraSourceList = new List<CameraInfo>();
            SqlConnection SQLConn = new SqlConnection(ConnString);
            SQLConn.Open();
            SqlCommand Sqlcmd = new SqlCommand(QueryString, SQLConn);
            SqlDataReader SR = Sqlcmd.ExecuteReader();
            while (SR.Read())
            {
                if (string.IsNullOrEmpty(SR.GetString(0)))
                {
                    Logger.LogMessage("RTSP Source Missing");
                }
                else
                {
                    CameraSourceList.Add(new CameraInfo(SR.GetString(0), SR.GetInt32(1)));
                }
            }
            SQLConn.Close();
            return CameraSourceList;
        }
    }
}
