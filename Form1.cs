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
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
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
        private bool CamerasEnabled = false;
        private bool FullScreenSize = false;

        private List<CameraInfo> CameraSourceList = new List<CameraInfo>();
        private VideoView[] vlc_list;
        WindowsAPI WPAI;

        const int LocXStart = 5;
        const int LocYStart = 45;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Failures here may indicate 32-bit build in use.
            Core.Initialize(@"C:\Program Files\VideoLAN\VLC");

            WindowState = FormWindowState.Maximized;

            LoadVideoStreams();
            WPAI = new WindowsAPI(this);
            WPAI.StartMouseHook();
            tmrWatch.Enabled = true;
            CamerasEnabled = true;
        }


        public void LoadVideoStreams()
        {
            CameraSourceList = SQLFunctions.GetCameraList();

            //Init objects.
            vlc_list = new VideoView[CameraSourceList.Count];
            for (int Index = 0; Index < CameraSourceList.Count; Index++)
            {
                RefreshCamera(Index);
            }
            ResizeVlcControls();

            //Optional add ways to load camera feeds.
        }

        public void RefreshCameras()
        {
            CameraOff();
            CamerasEnabled = true;
            for (int Index = 0; Index < CameraSourceList.Count; Index++)
            {
                RefreshCamera(Index);
            }
            ResizeVlcControls();
        }

        public void RefreshCamera(int Index)
        {
            CameraInfo Camera = CameraSourceList[Index];
            List<string> ArgsList = new List<string>
                {
                    "--rtsp-tcp",
                    //"--network-caching=1000",
                    "--aout=directsound" //stops working in vlc4, used to fix audio volume issue applying to all video views when set on 1.
                };

            if (Camera.Rotate != 0) //Rotate in use, enable transform.
            {
                ArgsList.Add("--video-filter=transform");
            }


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
            vlc_list[Index] = new VideoView();
            vlc_list[Index].MediaPlayer = new MediaPlayer(_LibVLC);
            vlc_list[Index].MediaPlayer.Media = new Media(_LibVLC, Camera.Address, FromType.FromLocation);
            // Required for form click event to work.
            // vlccontrol.Video.IsMouseInputEnabled = False
            // vlccontrol.Video.IsKeyInputEnabled = False

            
            Program.frm1.Invoke(new MethodInvoker(delegate
            {
                Controls.Add(vlc_list[Index]);
            }));

            vlc_list[Index].MediaPlayer.Volume = 0;
            vlc_list[Index].MediaPlayer.Play();

        }


        public void ResizeVlcControls()
        {
            if (vlc_list == null || vlc_list.Length == 0)
            {
                return; // skip if not active
            }

            // Re-enable audio if swapping from fullscreen.
            foreach (VideoView Cam in vlc_list)
            {
                Cam.Visible = true;
                if (AudioEnabled)
                {
                    Cam.MediaPlayer.Volume = 100;
                }
                else
                {
                    Cam.MediaPlayer.Volume = 0;
                }
                Cam.BringToFront();
            }
            FullScreenSize = false; // Clear fullscreen property.


            int LocX = LocXStart;
            int LocY = LocYStart;

            // Set number of columns for land/port.
            int MaxColumns = 2;
            if (Width > Height)
            {
                // Landscape
                MaxColumns = 3;
            }


            int MaxCells = vlc_list.Length;
            int MaxRows = (int)Math.Ceiling(MaxCells / (double)MaxColumns);

            // Camera Ratio 16:9 - Calculate ratio:
            double MaxHeight = (Height - (LocY + 50)) / MaxRows / 9d; // Divide height by number of rows (rounded up), total size minus start Y offset.
            double MaxWidth = (Width - (LocX + 50)) / (double)MaxColumns / 16d;

            int RatioMultiY = (int)Math.Round(MaxHeight); // Size by availble height.
            int RatioMultiX = (int)Math.Round(MaxWidth); // Size by available width.

            int RatioMulti = Math.Min(RatioMultiY, RatioMultiX); // Pick lowest ratio to ensure does not go over edge.
            Size ViewSize = new Size(RatioMulti * 16, RatioMulti * 9);

            int ColumnCount = 0;
            int RowCount = 1;
            List<int> SkipCells = new List<int>();
            int SkippedCells = 0;

            for (int i = 0; i < MaxCells; i++) //Loop through each cell position.
            {
                if (!SkipCells.Contains(i))
                {
                    int vi = i - SkippedCells;
                    if (vi >= CameraSourceList.Count)
                    {
                        continue; //avoid index exception.
                    }
                    if (CameraSourceList[vi].Rotate == 90 || CameraSourceList[vi].Rotate == 270)
                    {
                        //Span up to 3 rows when rotated.
                        int spanSize = 3;
                        if (MaxRows < spanSize)
                        {
                            spanSize = MaxRows;
                        }
                        //Console.WriteLine("Max rows:" + MaxRows + " max cols: " + MaxColumns);

                        if (spanSize == 2)
                        {
                            SkipCells.Add(i + MaxColumns);
                            MaxCells++;
                        }
                        else if (spanSize == 3)
                        {
                            SkipCells.Add(i + MaxColumns);
                            SkipCells.Add(i + (MaxColumns * 2));
                            MaxCells += 2;
                        }

                        vlc_list[vi].Size = new Size(ViewSize.Width, (ViewSize.Height * spanSize));
                    }
                    else
                    {
                        vlc_list[vi].Size = ViewSize;
                    }
                    vlc_list[vi].Location = new Point(LocX, LocY);
                }
                else
                {
                    SkippedCells++;
                }
                LocX += ViewSize.Width;
                ColumnCount++;
                if (ColumnCount == MaxColumns) //Next row.
                {
                    RowCount++;
                    LocY += ViewSize.Height;
                    ColumnCount = 0;
                    LocX = LocXStart;
                }
            }
        }

        private void EnableFullScreen(VideoView vlc_control)
        {
            foreach (VideoView Cam in vlc_list)
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
                if (FullScreenSize)
                {
                    vlc_list.FirstOrDefault(cam => cam.Visible).MediaPlayer.Volume = 100;
                }
                else
                {
                    foreach (VideoView cam in vlc_list)
                    {
                        cam.MediaPlayer.Volume = 100;
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
            CamerasEnabled = false;
            for (int Index = 0; Index < vlc_list.Length; Index++)
            {
                CameraOff(Index);
            }
        }
        public void CameraOff(int Index)
        {
            vlc_list[Index].MediaPlayer.Dispose();
            vlc_list[Index].Dispose();
            Controls.Remove(vlc_list[Index]);
        }



        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            RefreshCameras();
        }


        public void ClickCamera(Point RelativePoint)
        {
            if (CamerasEnabled)
            {
                if (FullScreenSize)
                {
                    //Return to normal layout view.
                    Rectangle CamPanelRec = new Rectangle(Location.X, Location.Y + LocYStart, Width, Height); // 80 offset to exclude buttons.
                    if (CamPanelRec.Contains(RelativePoint))
                    {
                        ResizeVlcControls();
                    }
                }
                else
                {
                    //Normal Mode - Show Selected camera.
                    for (int i = 0; i < vlc_list.Length; i++)
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


        bool FullScreenMode = false;
        private void btnM_Click(object sender, EventArgs e)
        {
            int taskBarHandle = (int)WindowsAPI.FindWindow("Shell_TrayWnd", "");

            if (FullScreenMode)
            {
                FullScreenMode = false;
                // Change to normal mode.
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowsAPI.ShowWindow(taskBarHandle, WindowsAPI.SW.SW_SHOW);
            }
            else
            {
                FullScreenMode = true;
                // Change to fullscreen.
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;

                this.Bounds = Screen.PrimaryScreen.Bounds;
                WindowsAPI.SetWindowPos(this.Handle, WindowsAPI.HWND_TOP, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, WindowsAPI.SWP_SHOWWINDOW);

                //Hide taskbar
                WindowsAPI.ShowWindow(taskBarHandle, WindowsAPI.SW.SW_HIDE);
            }
        }

        void Fullscreen()
        {

        }

        private void tmrWatch_Tick(object sender, EventArgs e)
        {
            if (!CamerasEnabled)
            {
                return;
            }
            for (int index = 0; index < vlc_list.Length; index++)
            {
                try
                {
                    if (!vlc_list[index].MediaPlayer.IsPlaying)
                    {
                        Console.WriteLine("Refreshing Camera " + index.ToString());
                        CameraOff(index);
                        RefreshCamera(index);
                        ResizeVlcControls();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to refresh camera: " + ex.Message);
                }

            }
        }
    }
}
