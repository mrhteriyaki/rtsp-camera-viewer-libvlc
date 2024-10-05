using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;


namespace rtsp_viewer_avalonia
{
    public class SQLFunctions
    {
        public static string SQLConnectionString = "Data Source=192.168.88.3,1433;Initial Catalog=HomeAutomation;Integrated Security=true;TrustServerCertificate=True;";


        public static string SqlCommandText = "SELECT [rtsp],rotate from tblCameras order by displayorder_console ASC";


        public static List<CameraInfo> GetCameraList()
        {
            List<CameraInfo> CameraSourceList = new List<CameraInfo>();
            SqlConnection SQLConn = new SqlConnection(SQLConnectionString);
            SQLConn.Open();
            SqlCommand Sqlcmd = new SqlCommand(SqlCommandText, SQLConn);
            SqlDataReader SR = Sqlcmd.ExecuteReader();
            while (SR.Read())
            {
                if (string.IsNullOrEmpty(SR[0].ToString()))
                {
                    //MessageBox.Show("RTSP Source Missing");
                }
                else
                {
                    CameraSourceList.Add(new CameraInfo(SR[0].ToString(), (int)SR[1]));
                }
            }
            SQLConn.Close();
            return CameraSourceList;
        }
    }
}
