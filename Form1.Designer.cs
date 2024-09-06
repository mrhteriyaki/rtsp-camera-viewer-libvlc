namespace rtsp_camera_viewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            btnOff = new System.Windows.Forms.Button();
            btnRefresh = new System.Windows.Forms.Button();
            btnAudio = new System.Windows.Forms.Button();
            btnM = new System.Windows.Forms.Button();
            tmrWatch = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // btnOff
            // 
            btnOff.BackColor = System.Drawing.Color.Black;
            btnOff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOff.ForeColor = System.Drawing.Color.White;
            btnOff.Location = new System.Drawing.Point(293, 5);
            btnOff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnOff.Name = "btnOff";
            btnOff.Size = new System.Drawing.Size(134, 35);
            btnOff.TabIndex = 9;
            btnOff.Text = "Off";
            btnOff.UseVisualStyleBackColor = false;
            btnOff.Click += btnOff_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = System.Drawing.Color.Black;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.ForeColor = System.Drawing.Color.White;
            btnRefresh.Location = new System.Drawing.Point(152, 5);
            btnRefresh.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(134, 35);
            btnRefresh.TabIndex = 8;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnAudio
            // 
            btnAudio.BackColor = System.Drawing.Color.Black;
            btnAudio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAudio.ForeColor = System.Drawing.Color.White;
            btnAudio.Location = new System.Drawing.Point(10, 5);
            btnAudio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnAudio.Name = "btnAudio";
            btnAudio.Size = new System.Drawing.Size(134, 35);
            btnAudio.TabIndex = 7;
            btnAudio.Text = "Enable Audio";
            btnAudio.UseVisualStyleBackColor = false;
            btnAudio.Click += btnAudio_Click;
            // 
            // btnM
            // 
            btnM.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnM.BackColor = System.Drawing.Color.Black;
            btnM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnM.ForeColor = System.Drawing.Color.White;
            btnM.Location = new System.Drawing.Point(1438, 12);
            btnM.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnM.Name = "btnM";
            btnM.Size = new System.Drawing.Size(47, 35);
            btnM.TabIndex = 10;
            btnM.Text = "M";
            btnM.UseVisualStyleBackColor = false;
            btnM.Click += btnM_Click;
            // 
            // tmrWatch
            // 
            tmrWatch.Interval = 15000;
            tmrWatch.Tick += tmrWatch_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(1498, 809);
            Controls.Add(btnM);
            Controls.Add(btnOff);
            Controls.Add(btnRefresh);
            Controls.Add(btnAudio);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Camera Viewer";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ResumeLayout(false);
        }

        #endregion

        internal System.Windows.Forms.Button btnOff;
        internal System.Windows.Forms.Button btnRefresh;
        internal System.Windows.Forms.Button btnAudio;
        internal System.Windows.Forms.Button btnM;
        private System.Windows.Forms.Timer tmrWatch;
    }
}

