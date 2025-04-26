using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using rtsp_camera_viewer_common;

namespace rtsp_camera_viewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private bool CamerasEnabled = false;



        private VideoView[] vlc_list;
        WindowsAPI WPAI;

        


        private void Form1_Load(object sender, EventArgs e)
        {
            //Failures here may indicate 32-bit build in use.

            try
            {
                ConfigLoader.VlcInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Check VLC is installed, or copy libvlccore and libvlc DLL files to the running directory. Exception intializing vlc core: " + ex.ToString());
                Application.Exit();
                return;

            }

            WindowState = FormWindowState.Maximized;

            try
            {
                LoadVideoStreams();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load a list of cameras, check config.ini or SQL. \n" + ex.Message);
                Application.Exit();
                return;
            }

            WPAI = new WindowsAPI(this);
            WPAI.StartMouseHook();
            tmrWatch.Enabled = true;
            CamerasEnabled = true;

        }


        public void LoadVideoStreams()
        {
            ConfigLoader.LoadStreamList();

            //Init objects.
            vlc_list = new VideoView[ConfigLoader.CameraSourceList.Count];
            RefreshCameras();
            VlcResize();

            //Optional add ways to load camera feeds.
        }

        public void RefreshCameras()
        {
            CameraOff();
            CamerasEnabled = true;
            for (int Index = 0; Index < ConfigLoader.CameraSourceList.Count; Index++)
            {
                RefreshCamera(Index);
            }
            VlcResize();
        }

        public void RefreshCamera(int Index)
        {
            CameraInfo Camera = ConfigLoader.CameraSourceList[Index];

            LibVLC _LibVLC = new LibVLC(Camera.GetVlcArgs().ToArray());
            vlc_list[Index] = new VideoView();
            vlc_list[Index].MediaPlayer = new MediaPlayer(_LibVLC);
            vlc_list[Index].MediaPlayer.Media = new Media(_LibVLC, Camera.Address, FromType.FromLocation);
            // Required for form click event to work.
            // vlccontrol.Video.IsMouseInputEnabled = False
            // vlccontrol.Video.IsKeyInputEnabled = False


            Program.frm1.Invoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                Controls.Add(vlc_list[Index]);
            }));

            vlc_list[Index].MediaPlayer.Volume = 0;
            vlc_list[Index].MediaPlayer.Play();

        }


        public void VlcResize()
        {
            if (vlc_list == null || vlc_list.Length == 0)
            {
                return; // skip if not active
            }

            // Re-enable audio if swapping from fullscreen.
            foreach (VideoView Cam in vlc_list)
            {
                Cam.Visible = true;
                Cam.MediaPlayer.Volume = ConfigLoader.AudioEnabled ? 100 : 0;
                Cam.BringToFront();
            }
            ConfigLoader.FullScreenSize = false; // Clear fullscreen property.

            int LocX = ConfigLoader.LocXStart;
            int LocY = ConfigLoader.LocYStart;

            // Set number of columns for land/port.
            //            int ConfigLoader.MaxColumns = MaxCols;
            /*
            if (Width > Height)
            {
                // Landscape
                ConfigLoader.MaxColumns = 3;
            }
            */

            int MaxRows = ConfigLoader.GetRowCount();
            int MaxCells = ConfigLoader.GetCellCount();

            // Camera Ratio 16:9 - Calculate ratio:
            double MaxHeight = (Height - (LocY + 50)) / MaxRows / 9d; // Divide height by number of rows (rounded up), total size minus start Y offset.
            double MaxWidth = (Width - (LocX + 50)) / (double)ConfigLoader.MaxColumns / 16d;

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
                    if (vi >= ConfigLoader.CameraSourceList.Count)
                    {
                        continue; //avoid index exception.
                    }
                    if (ConfigLoader.CameraSourceList[vi].Rotate == 90 || ConfigLoader.CameraSourceList[vi].Rotate == 270)
                    {
                        //Span up to 3 rows when rotated.
                        int spanSize = 3;
                        if (MaxRows < spanSize)
                        {
                            spanSize = MaxRows;
                        }


                        if (spanSize == 2)
                        {
                            SkipCells.Add(i + ConfigLoader.MaxColumns);
                            MaxCells++;
                        }
                        else if (spanSize == 3)
                        {
                            SkipCells.Add(i + ConfigLoader.MaxColumns);
                            SkipCells.Add(i + (ConfigLoader.MaxColumns * 2));
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
                if (ColumnCount == ConfigLoader.MaxColumns) //Next row.
                {
                    RowCount++;
                    LocY += ViewSize.Height + 5; //5px split between panels.
                    ColumnCount = 0;
                    LocX = ConfigLoader.LocXStart;
                }
            }

            btnAudio.BringToFront();
            btnRefresh.BringToFront();
            btnOff.BringToFront();
            btnSettings.BringToFront();
            btnM.BringToFront();

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
            vlc_control.MediaPlayer.Volume = ConfigLoader.AudioEnabled ? 100 : 0;
            ConfigLoader.FullScreenSize = true;
        }


        //Individual VideoView audio has known bug with VLC3.
        //https://code.videolan.org/videolan/vlc/-/issues/28194
        //Workaround to apply --aout=directsound will not work in VLC4.
        private void btnAudio_Click(object sender, EventArgs e)
        {
            if (ConfigLoader.AudioEnabled == true)
            {
                //Disable Audio.
                ConfigLoader.AudioEnabled = false;
                btnAudio.Text = "Enable Audio";
                foreach (VideoView cam in vlc_list)
                {
                    cam.MediaPlayer.Volume = 0;
                }
            }
            else
            {
                //Enable Audio.
                ConfigLoader.AudioEnabled = true;
                btnAudio.Text = "Disable Audio";
                if (ConfigLoader.FullScreenSize)
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
            if (vlc_list[Index] == null)
            {
                return;
            }
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
                if (ConfigLoader.FullScreenSize)
                {
                    Rectangle CamPanelRec = new Rectangle(0, ConfigLoader.LocYStart, Width, Height); // 80 offset to exclude buttons.
                    if (CamPanelRec.Contains(RelativePoint))
                    {
                        VlcResize();
                    }
                }
                else
                {
                    for (int i = 0; i < vlc_list.Length; i++)
                    {
                        Rectangle VlcRec = new Rectangle(vlc_list[i].Location.X, vlc_list[i].Location.Y + ConfigLoader.LocYStart, vlc_list[i].Width, vlc_list[i].Height);
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
            VlcResize();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WPAI != null)
            {
                WPAI.StopMouseHook();
            }
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
                        Logger.LogMessage("Camera not playing - refreshing source: " + index.ToString());
                        CameraOff(index);
                        RefreshCamera(index);
                        VlcResize();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Failed to refresh camera: " + ex.Message);
                }

            }
        }

        

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ConfigLoader.OpenSettingsApp();
        }
    }
}
