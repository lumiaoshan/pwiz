﻿namespace pwiz.Skyline.ToolsUI
{
    partial class RInstaller
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
            this.labelMessage = new System.Windows.Forms.Label();
            this.checkedListBoxPackages = new System.Windows.Forms.CheckedListBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(16, 11);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMessage.MaximumSize = new System.Drawing.Size(383, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(367, 51);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "This tool requires the use of R <Version Name> and the following packages. Select" +
                " which tools to install and then click Install to begin the installation process" +
                ". ";
            // 
            // checkedListBoxPackages
            // 
            this.checkedListBoxPackages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxPackages.CheckOnClick = true;
            this.checkedListBoxPackages.FormattingEnabled = true;
            this.checkedListBoxPackages.Location = new System.Drawing.Point(16, 76);
            this.checkedListBoxPackages.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkedListBoxPackages.Name = "checkedListBoxPackages";
            this.checkedListBoxPackages.Size = new System.Drawing.Size(381, 242);
            this.checkedListBoxPackages.TabIndex = 4;
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.Location = new System.Drawing.Point(191, 345);
            this.btnInstall.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(100, 28);
            this.btnInstall.TabIndex = 5;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(299, 345);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // RInstaller
            // 
            this.AcceptButton = this.btnInstall;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(415, 388);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.checkedListBoxPackages);
            this.Controls.Add(this.labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "RInstaller";
            this.Text = "R Installer";
            this.Load += new System.EventHandler(this.RInstaller_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.CheckedListBox checkedListBoxPackages;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnCancel;
    }
}