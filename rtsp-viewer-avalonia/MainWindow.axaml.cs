using Avalonia.Controls;
using LibVLCSharp.Shared;
using LibVLCSharp.Avalonia;

using System.IO;
using System;
using System.Collections.Generic;

using Avalonia.Interactivity;
using System.Linq;

using System.Diagnostics;

using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Avalonia;
using Avalonia.Media;
using System.Runtime.InteropServices;
using rtsp_camera_viewer_common;
using System.Threading;


namespace rtsp_viewer_avalonia
{
    public partial class MainWindow : Window
    {
        private VideoView[] vlc_list;

        int currentCell = 0;
        List<int> skipCellIndex = [];

        class VideoViewLocal
        {
            public VideoView videoView;
            public bool rotated = false;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;

            //trigger load after window opens.
            this.Opened += InitCams;
        }

        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            ConfigLoader.OpenSettingsApp();
        }

        public void EnableAudioClicked(object sender, RoutedEventArgs args)
        {
            if (ConfigLoader.AudioEnabled == true)
            {
                //Disable Audio.
                ConfigLoader.AudioEnabled = false;
                btnAudio.Content = "Enable Audio";
                foreach (VideoView cam in vlc_list)
                {
                    cam.MediaPlayer.Volume = 0;
                }
            }
            else
            {
                //Enable Audio.
                ConfigLoader.AudioEnabled = true;
                btnAudio.Content = "Disable Audio";
                if (ConfigLoader.FullScreenSize)
                {
                    vlc_list.FirstOrDefault(cam => cam.IsVisible).MediaPlayer.Volume = 100;
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

        public void RefreshClicked(object sender, RoutedEventArgs args)
        {
            //RefreshCameras();
            //Problems disposing of object.
            for (int Index = 0; Index < vlc_list.Length; Index++)
            {
                vlc_list[Index].MediaPlayer.Dispose();
            }
            try
            {
                //error occurs here.
                MainPanel.Children.Clear();
            }
            catch
            {
                
            }
            
           //RefreshCameras();
        }

        public void OffClicked(object sender, RoutedEventArgs args)
        {
            //MainPanel.Children.Clear();
            
        }

        void InitCams(object sender, EventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Operating System: Windows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Operating System: Linux");
                Console.WriteLine("If vlc init fails - check package libvlc-dev (Debian) or vlc-devel (RHEL) is installed.");
            }

            Console.WriteLine("Init VLC");
            try
            {
                ConfigLoader.VlcInit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("VLC init faield." + ex.ToString());
                return;
            }


            Console.WriteLine("Getting source list.");

            ConfigLoader.LoadStreamList();

            Console.WriteLine("Setup vlc_list");

            vlc_list = new VideoView[ConfigLoader.CameraSourceList.Count];

            Console.WriteLine("Setup grid");


            for (int i = 0; i < ConfigLoader.MaxColumns; i++)
            {
                MainPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }

            int rows = ConfigLoader.GetRowCount();
            for (int i = 0; i < rows; i++)
            {
                MainPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
          

            //Add Buttons.

            if (ConfigLoader.LocYStart == 0) //No offset.
            {

            }

            
            Console.WriteLine("Start source refresh");
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
                if (ConfigLoader.CameraSourceList[i].GetRotateBlock())
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
            for (int Index = 0; Index < ConfigLoader.CameraSourceList.Count; Index++)
            {
                RefreshCamera(Index);
            }

            RestoreNormalGrid();
        }

        public void RefreshCamera(int Index)
        {
            CameraInfo Camera = ConfigLoader.CameraSourceList[Index];

            LibVLC _LibVLC = new LibVLC(Camera.GetVlcArgs().ToArray());
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


            int rowVal = currentCell / ConfigLoader.MaxColumns;

            Grid.SetRow(vlc_list[Index], rowVal);
            Grid.SetColumn(vlc_list[Index], currentCell % ConfigLoader.MaxColumns);

            if (ConfigLoader.CameraSourceList[Index].GetRotateBlock())
            {
                Grid.SetRowSpan(vlc_list[Index], 3);

                //+1 offsite for 0-based.
                skipCellIndex.Add(rowVal + ConfigLoader.MaxColumns + 1);
                skipCellIndex.Add(rowVal + (ConfigLoader.MaxColumns * 2) + 1);
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
            if (ConfigLoader.FullScreenSize)
            {
                //Restore normal view.
                RestoreNormalGrid();
                ConfigLoader.FullScreenSize = false;
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
                vlc_control.MediaPlayer.Volume = ConfigLoader.AudioEnabled ? 100 : 0;

                ConfigLoader.FullScreenSize = true;
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
            if (vlc_list == null)
            {
                return;
            }

            foreach (VideoView Cam in vlc_list)
            {
                Cam.IsVisible = true;
                Cam.MediaPlayer.Volume = ConfigLoader.AudioEnabled ? 100 : 0;
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
            ConfigLoader.FullScreenSize = false;
        }

    }
}