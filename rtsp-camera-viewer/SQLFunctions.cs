using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace rtsp_camera_viewer
{
    public class SQLFunctions
    {
        //Example config for sqlconfig.ini
        
        //Data Source=192.168.88.3,1433;Initial Catalog=HomeAutomation;Integrated Security=true;TrustServerCertificate=True;
        //SELECT [rtsp],rotate from tblCameras order by displayorder_console ASC;


        public static List<CameraInfo> GetCameraList()
        {
            string sqlconn = "";
            string selectquery = "";

            if(File.Exists("sqlconfig.ini"))
            { 
                foreach(string line in File.ReadAllLines("sqlconfig.ini"))
                {
                    if(line.ToLower().StartsWith("data source="))
                    {
                        sqlconn = line;
                    }
                    else if(line.ToLower().StartsWith("select"))
                    {
                        selectquery = line;
                    }    
                }
            }
            else
            {
                throw new Exception("No sqlconfig.ini file found.");
            }
            if(string.IsNullOrEmpty(sqlconn))
            {
                throw new Exception("Missing sql connection string.");
            }
            if (string.IsNullOrEmpty(selectquery))
            {
                throw new Exception("Missing sql select statement string.");
            }

            List<CameraInfo> CameraSourceList = new List<CameraInfo>();
            SqlConnection SQLConn = new SqlConnection(sqlconn);
            SQLConn.Open();
            SqlCommand Sqlcmd = new SqlCommand(selectquery, SQLConn);
            SqlDataReader SR = Sqlcmd.ExecuteReader();
            while (SR.Read())
            {
                if (string.IsNullOrEmpty(SR[0].ToString()))
                {
                    Logger.LogMessage("RTSP Source Missing");
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
