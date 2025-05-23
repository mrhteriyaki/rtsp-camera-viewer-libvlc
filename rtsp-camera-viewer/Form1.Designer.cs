﻿namespace rtsp_camera_viewer
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
            btnSettings = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnOff
            // 
            btnOff.BackColor = System.Drawing.Color.Black;
            btnOff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOff.ForeColor = System.Drawing.Color.White;
            btnOff.Location = new System.Drawing.Point(240, 0);
            btnOff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnOff.Name = "btnOff";
            btnOff.Size = new System.Drawing.Size(120, 35);
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
            btnRefresh.Location = new System.Drawing.Point(120, 0);
            btnRefresh.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(120, 35);
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
            btnAudio.Location = new System.Drawing.Point(0, 0);
            btnAudio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnAudio.Name = "btnAudio";
            btnAudio.Size = new System.Drawing.Size(120, 35);
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
            btnM.Location = new System.Drawing.Point(633, 0);
            btnM.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnM.Name = "btnM";
            btnM.Size = new System.Drawing.Size(35, 35);
            btnM.TabIndex = 10;
            btnM.Text = "⬜";
            btnM.UseVisualStyleBackColor = false;
            btnM.Click += btnM_Click;
            // 
            // tmrWatch
            // 
            tmrWatch.Interval = 15000;
            tmrWatch.Tick += tmrWatch_Tick;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSettings.BackColor = System.Drawing.Color.Black;
            btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSettings.ForeColor = System.Drawing.Color.White;
            btnSettings.Location = new System.Drawing.Point(571, 0);
            btnSettings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(61, 35);
            btnSettings.TabIndex = 11;
            btnSettings.Text = "Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(672, 281);
            Controls.Add(btnSettings);
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
        internal System.Windows.Forms.Button btnSettings;
    }
}

