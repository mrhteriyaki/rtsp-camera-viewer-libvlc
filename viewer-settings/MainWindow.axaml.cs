using Avalonia.Controls;
using Avalonia.Interactivity;
using rtsp_camera_viewer_common;
using SkiaSharp;
using System.IO;


namespace viewer_settings
{
    public partial class MainWindow : Window
    {
        static bool localSource = true;

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(ConfigConstant.str_cf_file))
            {
                foreach (string cLine in File.ReadAllLines(ConfigConstant.str_cf_file, System.Text.Encoding.UTF8))
                {
                    if (cLine.StartsWith($"{ConfigConstant.str_camera}="))
                    {
                        lbxStreams.Items.Add(cLine.Split("=")[1]);
                    }
                    else if (cLine.StartsWith($"{ConfigConstant.str_maxcols}="))
                    {
                        txtMaxCols.Text = cLine.Split("=")[1];
                    }
                    else if (cLine.StartsWith($"{ConfigConstant.str_overlap}="))
                    {
                        string overlap_val = cLine.Split("=")[1];
                        chkOffsetButtons.IsChecked = overlap_val.Equals("1") ? true : false;
                    }
                    else if (cLine.StartsWith($"{ConfigConstant.str_connstring}="))
                    {
                        txtSql.Text = ConfigLoader.GetValue(cLine);
                    }
                    else if (cLine.StartsWith($"{ConfigConstant.str_sqlquery}="))
                    {
                        txtQuery.Text = ConfigLoader.GetValue(cLine);
                    }
                }
            }
            UpdateHeight();

            if (string.IsNullOrWhiteSpace(txtMaxCols.Text))
            {
                txtMaxCols.Text = "3";
            }

            if(string.IsNullOrWhiteSpace(txtSql.Text))
            {
                rbtLocal.IsChecked = true;
            }
            else
            {  
                rbtSql.IsChecked = true;
            }
            
        }

        void SetSourceView(bool local)
        {
            spSql.IsVisible = !local;
            gridSources.IsVisible = local;
            spLocalSources.IsVisible = local;
        }

        void UpdateHeight()
        {
            Height = 300;
            foreach (var lxs in lbxStreams.Items)
            {
                this.Height += 40;
            }
        }

        void WriteConfig()
        {
            StreamWriter sw = new(ConfigConstant.str_cf_file, false);
            if (localSource)
            {
                foreach (var stream in lbxStreams.Items)
                {
                    sw.WriteLine($"{ConfigConstant.str_camera}={stream}");
                }
            }
            else
            {
                sw.WriteLine($"{ConfigConstant.str_connstring}={txtSql.Text}");
                sw.WriteLine($"{ConfigConstant.str_sqlquery}={txtQuery.Text}");
            }

            if (chkOffsetButtons.IsChecked == true)
            {
                sw.WriteLine($"{ConfigConstant.str_overlap}=1");
            }
            if (txtMaxCols.Text != "3" && !string.IsNullOrEmpty(txtMaxCols.Text))
            {
                int maxCol = int.Parse(txtMaxCols.Text);
                sw.WriteLine($"{ConfigConstant.str_maxcols}={maxCol.ToString()}");
            }

            sw.Close();
        }


        public void OnSaveClicked(object sender, RoutedEventArgs args)
        {
            WriteConfig();
        }

        public void OnAddClicked(object sender, RoutedEventArgs args)
        {
            lbxStreams.Items.Add(txtSource.Text);
            WriteConfig();
            UpdateHeight();
        }

        public void OnRemovedClicked(object sender, RoutedEventArgs args)
        {
            lbxStreams.Items.Remove(lbxStreams.SelectedItem);
            UpdateHeight();
        }

        private void Source_Changed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                string content = radioButton.Content.ToString();
                if (content.Equals(rbtSql.Content))
                {
                    SetSourceView(false);
                    localSource = false;
                }
                else if (content.Equals(rbtLocal.Content))
                {
                    SetSourceView(true);
                    localSource = true;
                }
            }
        }


    }
}