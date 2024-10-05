using Avalonia.Controls;
using LibVLCSharp.Shared;
using System.IO;
using System;
using System.Collections.Generic;
using LibVLCSharp.Avalonia;

using Avalonia.Interactivity;
using System.Linq;

using System.Diagnostics;

using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Avalonia;
using Avalonia.Media;



namespace rtsp_viewer_avalonia
{
    public partial class MainWindow : Window
    {
        private List<CameraInfo> CameraSourceList;
        private VideoView[] vlc_list;
        private bool[] rotateList;

        int MaxColumns = 3;
        int currentCell = 0;
        List<int> skipCellIndex = [];

        bool AudioEnabled = false;
        bool FullScreenSize = false;

        class VideoViewLocal
        {
            public VideoView videoView;
            public bool rotated = false;
        }


        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            try
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
            catch (Exception ex)
            {
                return;
            }
            CameraSourceList = SQLFunctions.GetCameraList();
            vlc_list = new VideoView[CameraSourceList.Count];
            rotateList = new bool[CameraSourceList.Count];

            for (int i = 0; i < MaxColumns; i++)
            {
                MainPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }

            int rows = CameraSourceList.Count / MaxColumns;
            if ((CameraSourceList.Count % MaxColumns) > 0)
            {
                rows++;
            }
            for (int i = 0; i < rows; i++)
            {
                MainPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
            RefreshCameras();
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainPanel != null)
            {
                RestoreNormalGrid();
            }
        }

        void VlcResize()
        {
            for (int i = 0; i < vlc_list.Count(); i++)
            {
                vlc_list[i].Width = MainPanel.Width / MainPanel.ColumnDefinitions.Count;
                if (rotateList[i])
                {
                    vlc_list[i].Height = (MainPanel.Height / MainPanel.RowDefinitions.Count) * 3;
                }
                else
                {
                    vlc_list[i].Height = MainPanel.Height / MainPanel.RowDefinitions.Count;
                }
            }
        }


        public void RefreshCameras()
        {
            //CameraOff();
            //CamerasEnabled = true;

            MainPanel.Children.Clear();
            for (int Index = 0; Index < CameraSourceList.Count; Index++)
            {
                RefreshCamera(Index);
            }
            //ResizeVlcControls();
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

            rotateList[Index] = false;
            if (Camera.Rotate != 0) //Rotate in use, enable transform.
            {
                ArgsList.Add("--video-filter=transform");
                if (Camera.Rotate == 90)
                {
                    ArgsList.Add("--transform-type=90");
                    rotateList[Index] = true;
                }
                else if (Camera.Rotate == 180)
                {
                    ArgsList.Add("--transform-type=180");
                }
                else if (Camera.Rotate == 270)
                {
                    ArgsList.Add("--transform-type=270");
                    rotateList[Index] = true;
                }
            }


            LibVLC _LibVLC = new LibVLC(ArgsList.ToArray());
            vlc_list[Index] = new VideoView();
            vlc_list[Index].MediaPlayer = new MediaPlayer(_LibVLC);
            vlc_list[Index].MediaPlayer.Media = new Media(_LibVLC, Camera.Address, FromType.FromLocation);


            Panel clickPanel = new Panel();
            clickPanel.PointerPressed += (sender, e) =>
            {
                EnableFullScreen(vlc_list[Index]);
            };

            //set transparent background to allow mouse click trigger.
            clickPanel.Opacity = 0.0;
            clickPanel.Background = new SolidColorBrush(Colors.Gray);
            vlc_list[Index].Content = clickPanel;




            // Required for form click event to work.
            // vlccontrol.Video.IsMouseInputEnabled = False
            // vlccontrol.Video.IsKeyInputEnabled = False

            //Add control to window panel.


            int rowVal = currentCell / MaxColumns;

            Grid.SetRow(vlc_list[Index], rowVal);
            Grid.SetColumn(vlc_list[Index], currentCell % MaxColumns);

            if (rotateList[Index])
            {
                Grid.SetRowSpan(vlc_list[Index], 3);

                //+1 offsite for 0-based.
                skipCellIndex.Add(rowVal + MaxColumns + 1);
                skipCellIndex.Add(rowVal + (MaxColumns * 2) + 1);
            }

            MainPanel.Children.Add(vlc_list[Index]);

            vlc_list[Index].MediaPlayer.Volume = 0;
            vlc_list[Index].MediaPlayer.Play();

            currentCell++;
            foreach (int skipval in skipCellIndex)
            {
                if (skipval == currentCell)
                {
                    currentCell++;
                    break;
                }
            }

        }

        private void EnableFullScreen(VideoView vlc_control)
        {
            if (FullScreenSize)
            {
                //Restore normal view.
                RestoreNormalGrid();
                FullScreenSize = false;
            }
            else
            {
                //Fullscreen mode, hide other cameras.
                foreach (VideoView Cam in vlc_list)
                {
                    if (!ReferenceEquals(Cam, vlc_control))
                    {
                        Cam.IsVisible = false;
                        Cam.MediaPlayer.Volume = 0;

                    }
                }
                vlc_control.IsVisible = true;

                SetGridFullscreen(vlc_control);

                //vlc_control.Location = new Point(20, 50);
                //vlc_control.Size = new Size(this.Width - 70, this.Height - 90);

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

        }

        void SetGridFullscreen(VideoView control)
        {
            var rowIndex = Grid.GetRow(control);
            var colIndex = Grid.GetColumn(control);

            for (int i = 0; i < MainPanel.RowDefinitions.Count; i++)
            {
                MainPanel.RowDefinitions[i].Height = new GridLength(i == rowIndex ? 1 : 0, GridUnitType.Star);
            }

            for (int i = 0; i < MainPanel.ColumnDefinitions.Count; i++)
            {
                MainPanel.ColumnDefinitions[i].Width = new GridLength(i == colIndex ? 1 : 0, GridUnitType.Star);
            }

            control.Width = MainPanel.Width;
            control.Height = MainPanel.Height;
        }
        void RestoreNormalGrid()
        {
            
            foreach (VideoView Cam in vlc_list)
            {
                Cam.IsVisible = true;
                if (AudioEnabled == true)
                {
                    Cam.MediaPlayer.Volume = 100;
                }
                else
                {
                    Cam.MediaPlayer.Volume = 0;
                }
            }

            for (int i = 0; i < MainPanel.RowDefinitions.Count; i++)
            {
                MainPanel.RowDefinitions[i].Height = new GridLength(1, GridUnitType.Star);
            }

            for (int i = 0; i < MainPanel.ColumnDefinitions.Count; i++)
            {
                MainPanel.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
            }

            VlcResize();
            FullScreenSize = false;
        }

    }
}