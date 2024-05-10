using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rtsp_camera_viewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool AudioEnabled = false;
        private bool CamerasOff = true;
        private bool FullScreenSize = false;

        private List<CameraInfo> CameraSourceList = new List<CameraInfo>();
        private List<VideoView> vlc_list = new List<VideoView>();
        WindowsAPI WPAI;

        const int LocXStart = 5;
        const int LocYStart = 45;

        private void Form1_Load(object sender, EventArgs e)
        {           
            //Failures here may indicate 32-bit build in use.
            Core.Initialize(@"C:\Program Files\VideoLAN\VLC");
            
            LoadVideoStreams();
            RefreshCameras();
            WPAI = new WindowsAPI(this);
            WPAI.StartMouseHook();
        }


        public void LoadVideoStreams()
        {
            CameraSourceList = SQLFunctions.GetCameraList();

            //Optional add ways to load camera feeds.
        }

        public void RefreshCameras()
        {
            CameraOff();
            CamerasOff = false;
            foreach (var Camera in CameraSourceList)
            {
                List<string> ArgsList = new List<string>
                {
                    "--rtsp-tcp",
                    ":network-caching=1000",
                    "--aout=directsound" //stops working in vlc4, used to fix audio volume issue applying to all video views when set on 1.
                };

                if (Camera.Rotate == 90)
                {
                    ArgsList.Add("--transform-type=90");
                }
                else if (Camera.Rotate == 180)
                {
                    ArgsList.Add("--transform-type=180");
                }
                else if (Camera.Rotate == 270)
                {
                    ArgsList.Add("--transform-type=270");
                }

                LibVLC _LibVLC = new LibVLC(ArgsList.ToArray());
                VideoView _VideoView = new VideoView();
                vlc_list.Add(_VideoView);
                _VideoView.MediaPlayer = new MediaPlayer(_LibVLC);
                _VideoView.MediaPlayer.Media = new Media(_LibVLC, Camera.Address, FromType.FromLocation);
                // Required for form click event to work.
                // vlccontrol.Video.IsMouseInputEnabled = False
                // vlccontrol.Video.IsKeyInputEnabled = False

                Controls.Add(_VideoView);

                // Use thread to trigger load of each camera.
                ThreadPool.QueueUserWorkItem(state =>
                {
                    StartwithVol0(ref _VideoView, Camera.Address);
                });

            }

            ResizeVlcControls();
        }

        public void StartwithVol0(ref VideoView VLCControl, string Address)
        {
            VLCControl.MediaPlayer.Volume = 0;
            VLCControl.MediaPlayer.Play();
        }

        public void ResizeVlcControls()
        {
            if (vlc_list.Count == 0)
            {
                return; // skip if not active
            }

            // Re-enable audio if swapping from fullscreen.
            foreach (var Cam in vlc_list)
            {
                Cam.Visible = true;
                if (AudioEnabled == true)
                {
                    Cam.MediaPlayer.Volume = 100;
                }
                else
                {
                    Cam.MediaPlayer.Volume = 0;
                }
            }
            FullScreenSize = false; // Clear fullscreen property.


            int LocX = LocXStart;
            int LocY = LocYStart;

            // Set number of columns for land/port.
            int Columns = 2;
            if (Width > Height)
            {
                // Landscape
                Columns = 3;
            }

            double RowCount = Math.Ceiling(vlc_list.Count / (double)Columns);
            // Camera Ratio 16:9 - Calculate ratio:
            double MaxHeight = (Height - (LocY + 50)) / RowCount / 9d; // Divide height by number of rows (rounded up), total size minus start Y offset.
            double MaxWidth = (Width - (LocX + 50)) / (double)Columns / 16d;

            int RatioMultiY = (int)Math.Round(Math.Round(MaxHeight) - 1d); // Size by availble height.
            double RatioMultiX = Math.Round(MaxWidth); // Size by available width.

            int RatioMulti = (int)Math.Round(Math.Min(RatioMultiY, RatioMultiX)); // Pick lowest ratio to ensure does not go over edge.


            // Dim ViewSizeMulti As Integer = (Me.Width * 0.06) / Columns
            var ViewSize = new Size(RatioMulti * 16, RatioMulti * 9);

            int Counter = 0;
            int ColumnCount = 0;

            foreach (var VlcItem in vlc_list)
            {
                if (ColumnCount == Columns)
                {
                    LocX = LocXStart;
                    LocY += ViewSize.Height;
                    ColumnCount = 0;
                }
                VlcItem.Size = ViewSize;
                VlcItem.Location = new Point(LocX, LocY);
                Counter = +1;
                ColumnCount += 1;
                LocX += ViewSize.Width;
            }




        }

        private void EnableFullScreen(VideoView vlc_control)
        {
            foreach (var Cam in vlc_list)
            {
                if (!ReferenceEquals(Cam, vlc_control))
                {
                    Cam.Visible = false;
                    Cam.MediaPlayer.Volume = 0;
                }
            }
            vlc_control.Visible = true;
            vlc_control.Location = new Point(20, 50);
            vlc_control.Size = new Size(this.Width - 70, this.Height - 90);
            if (AudioEnabled == true)
            {
                vlc_control.MediaPlayer.Volume = 100;
            }
            else
            {
                vlc_control.MediaPlayer.Volume = 0;
            }
            FullScreenSize = true;
        }


        //Individual VideoView audio has known bug with VLC3.
        //https://code.videolan.org/videolan/vlc/-/issues/28194
        //Workaround to apply --aout=directsound will not work in VLC4.
        private void btnAudio_Click(object sender, EventArgs e)
        {
            if (AudioEnabled == true)
            {
                //Disable Audio.
                AudioEnabled = false;
                btnAudio.Text = "Enable Audio";
                foreach (VideoView cam in vlc_list)
                {
                    cam.MediaPlayer.Volume = 0;
                }
            }
            else
            {
                //Enable Audio.
                AudioEnabled = true;
                btnAudio.Text = "Disable Audio";
                if (FullScreenSize == false)
                {
                    foreach (VideoView cam in vlc_list)
                    {
                        cam.MediaPlayer.Volume = 100;
                    }
                }
                else
                {
                    foreach (VideoView cam in vlc_list)
                    {
                        if (cam.Visible == true)
                        {
                            cam.MediaPlayer.Volume = 100;
                        }
                    }
                }
            }

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCameras();
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            CameraOff();
        }

        
        public void CameraOff()
        {
            CamerasOff = true;

            foreach (VideoView vlc_videoview in vlc_list)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    RemoveVLC(vlc_videoview);
                });
                Controls.Remove(vlc_videoview);
            }

            vlc_list.Clear();
        }

        public void RemoveVLC(VideoView vlc_videoview)
        {
            vlc_videoview.MediaPlayer.Stop();
            vlc_videoview.MediaPlayer.Media.Dispose();
            vlc_videoview.MediaPlayer.Dispose();
            vlc_videoview.Dispose();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            RefreshCameras();
        }

        
        public void ClickCamera(Point RelativePoint)
        {
            if (CamerasOff == false)
            {
                if (FullScreenSize)
                {
                    Rectangle CamPanelRec = new Rectangle(Location.X, Location.Y + LocYStart, Width, Height); // 80 offset to exclude buttons.
                    if (CamPanelRec.Contains(RelativePoint))
                    {
                        ResizeVlcControls();
                    }
                }
                else
                {
                    for (int i = 0; i < vlc_list.Count; i++)
                    {
                        Rectangle VlcRec = new Rectangle(vlc_list[i].Location.X, vlc_list[i].Location.Y + LocYStart, vlc_list[i].Width, vlc_list[i].Height);
                        if (VlcRec.Contains(RelativePoint))
                        {
                            EnableFullScreen(vlc_list[i]);
                        }
                    }
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizeVlcControls();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WPAI.StopMouseHook();
        }
    }
}
