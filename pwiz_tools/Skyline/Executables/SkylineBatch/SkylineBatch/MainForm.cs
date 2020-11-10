﻿/*
 * Original author: Ali Marsh <alimarsh .at. uw.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 * Copyright 2020 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkylineBatch.Properties;

namespace SkylineBatch
{
    public partial class MainForm : Form, IMainUiControl
    {

        private ConfigManager configManager;

        private ISkylineBatchLogger _skylineBatchLogger;
        

        public MainForm()
        {
            var skylineFileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
            var logFile = Path.Combine(skylineFileDir ?? string.Empty, "SkylineBatch.log");
            _skylineBatchLogger = new SkylineBatchLogger(logFile, this);
            InitializeComponent();

            btnRunOptions.Text = char.ConvertFromUtf32(0x2BC6);

            btnCopy.Enabled = false;
            btnDelete.Enabled = false;
            btnEdit.Enabled = false;
            btnUpArrow.Enabled = false;
            btnDownArrow.Enabled = false;
            btnCancel.Enabled = false;


            Program.LogInfo("Loading configurations from saved settings.");
            configManager = new ConfigManager(_skylineBatchLogger, this);
            UpdateUiConfigurations();
            UpdateLabelVisibility();
            
            
            
            UpdateUiLogFiles();
        }

        private void RunUi(Action action)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(action);
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                action();
            }
        }

        #region Manipulating configuration list

        private static void ShowConfigForm(SkylineBatchConfigForm configForm)
        {
            configForm.StartPosition = FormStartPosition.CenterParent;
            configForm.ShowDialog();
        }

        private void btnNewConfig_Click(object sender, EventArgs e)
        {
            var configForm = new SkylineBatchConfigForm(configManager.CreateConfiguration(), this, false);
            configForm.StartPosition = FormStartPosition.CenterParent;
            ShowConfigForm(configForm);
        }

        public void AddConfiguration(SkylineBatchConfig config)
        {
            configManager.AddConfiguration(config);
            UpdateUiConfigurations();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {   
            var configRunner = configManager.GetSelectedConfigRunner();
            // can edit if config is not busy running, otherwise is view only
            Program.LogInfo(string.Format("{0} configuration \"{1}\"",
                (!configRunner.IsRunning() ? "Editing" : "Viewing"),
                configRunner.GetConfigName()));
            var configForm = new SkylineBatchConfigForm(configRunner.Config, this, configRunner.IsBusy());
            ShowConfigForm(configForm);
        }

        public void EditConfiguration(SkylineBatchConfig oldVersion, SkylineBatchConfig newVersion)
        {
            configManager.ReplaceConfig(oldVersion, newVersion);
            UpdateUiConfigurations();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var configForm = new SkylineBatchConfigForm(configManager.CopySelectedConfig(), this, false);
            ShowConfigForm(configForm);
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            configManager.RemoveSelected();
            UpdateUiConfigurations();

            UpdateLabelVisibility();
        }
        
        private void listViewConfigs_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = listViewConfigs.SelectedItems.Count > 0 ? listViewConfigs.SelectedIndices[0] : -1;
            configManager.SelectConfig(selected);
            // update buttons enabled
            btnEdit.Enabled = selected >= 0;
            btnCopy.Enabled = selected >= 0;
            btnDelete.Enabled = selected >= 0;
            btnUpArrow.Enabled = selected > 0;
            btnDownArrow.Enabled = selected >= 0 && selected < listViewConfigs.Items.Count - 1;
        }

        private void btnUpArrow_Click(object sender, EventArgs e)
        {
            configManager.MoveSelectedConfig(true);
            UpdateUiConfigurations();
        }

        private void btnDownArrow_Click(object sender, EventArgs e)
        {
            configManager.MoveSelectedConfig(false);
            UpdateUiConfigurations();
        }

        #endregion


        #region Running configurations

        private void btnRunOptions_Click(object sender, EventArgs e)
        {
            batchRunDropDown.Show(btnRunBatch, new Point(0, btnRunBatch.Height));
        }

        private void batchRunDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int selectedIndex = 0;
            for (int i = 0; i < batchRunDropDown.Items.Count; i++)
            {
                if (batchRunDropDown.Items[i].Text == e.ClickedItem.Text)
                    selectedIndex = i;
                ((ToolStripMenuItem)batchRunDropDown.Items[i]).Checked = false;
            }
            ((ToolStripMenuItem)batchRunDropDown.Items[selectedIndex]).Checked = true;
            btnRunBatch.Text = string.Format(Resources.Pneumonic_on_first_letter, e.ClickedItem.Text);
            RunBatch();
        }

        private void btnRunBatch_Click(object sender, EventArgs e)
        {
            RunBatch();
        }

        private void RunBatch()
        {
            for (int i = 1; i <= batchRunDropDown.Items.Count; i++)
            {
                if (((ToolStripMenuItem)batchRunDropDown.Items[i - 1]).Checked)
                {
                    configManager.RunAll(i);
                    break;
                }
            }
            if (configManager.HasConfigs())
                btnCancel.Enabled = true;
            // update ui log and switch to log tab
            comboLogList.SelectedIndex = 0;
            tabMain.SelectTab(tabLog);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            configManager.CancelRunners();
            btnCancel.Enabled = false;
        }



        #endregion


        #region Update UI

        // Reload configurations from configManager
        public void UpdateUiConfigurations()
        {
            RunUi(() =>
            {
                Program.LogInfo("Updating configurations");
                listViewConfigs.Items.Clear();
                var listViewItems = configManager.ConfigsListViewItems();
                foreach (var lvi in listViewItems)
                    listViewConfigs.Items.Add(lvi);
                if (configManager.SelectedConfig >= 0)
                    listViewConfigs.Items[configManager.SelectedConfig].Selected = true;
                UpdateLabelVisibility();
            });

        }

        // Reload logs in comboLogList
        public void UpdateUiLogFiles()
        {
            RunUi(() =>
            {
                Program.LogInfo("Updating log files");
                comboLogList.Items.Clear();
                comboLogList.Items.AddRange(configManager.GetAllLogFiles());
                comboLogList.SelectedIndex = configManager.SelectedLog;
                btnDeleteLogs.Enabled = configManager.HasOldLogs();
            });

        }

        public void UpdateRunningButtons(bool isRunning)
        {
            RunUi(() =>
            {
                btnRunBatch.Enabled = !isRunning;
                btnRunOptions.Enabled = btnRunBatch.Enabled;
                btnCancel.Enabled = isRunning;
            });
        }

        // Toggle label if no configs
        private void UpdateLabelVisibility()
        {
            if (configManager.HasConfigs())
            {
                lblNoConfigs.Hide();
            }
            else
            {
                lblNoConfigs.Show();
            }
        }

        #endregion


        #region Import / export

        private void btnImport_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = Resources.XML_file_extension;
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var filePath = dialog.FileName;
            var importMessage = configManager.Import(filePath);

            UpdateUiConfigurations();

            if (!string.IsNullOrEmpty(importMessage))
                MessageBox.Show(importMessage, Resources.Import_configurations, MessageBoxButtons.OK);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {

            var dialog = new SaveFileDialog { Title = Resources.Save_configurations, Filter = Resources.XML_file_extension };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            configManager.ExportAll(dialog.FileName);
        }


        #endregion


        #region Logging
        // TODO: see if you can move some of this to skylineBatchLogger for better testing

        private void comboLogList_SelectedIndexChanged(object sender, EventArgs e)
        {
            configManager.SelectLog(comboLogList.SelectedIndex);
            SwitchLogger();
        }

        private async void SwitchLogger()
        {
            textBoxLog.Clear();

            var logger = configManager.GetSelectedLogger();
            try
            {
                await Task.Run(() =>
                {
                    // Read the log contents and display in the log tab.
                    logger.DisplayLog();
                });
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error Reading Log", ex.Message);
            }

            ScrollToLogEnd();
        }

        private void ScrollToLogEnd()
        {
            RunUi(() =>
            {
                textBoxLog.SelectionStart = textBoxLog.Text.Length;
                textBoxLog.ScrollToCaret();
            });
        }

        private void btnDeleteLogs_Click(object sender, EventArgs e)
        {
            var manageLogsForm = new LogForm(configManager);
            manageLogsForm.StartPosition = FormStartPosition.CenterParent;
            manageLogsForm.ShowDialog();
        }

        public void LogToUi(string text, bool scrollToEnd, bool trim)
        {

            RunUi(() =>
            {
                if (comboLogList.SelectedIndex != 0) return; // don't log if old log is displayed
                if (trim)
                {
                    TrimDisplayedLog();
                }

                textBoxLog.AppendText(text);
                textBoxLog.AppendText(Environment.NewLine);

                if (!scrollToEnd) return;

                ScrollToLogEnd();
            });

        }

        private void TrimDisplayedLog()
        {
            var numLines = textBoxLog.Lines.Length;
            const int buffer = SkylineBatchLogger.MaxLogLines / 10;
            if (numLines > SkylineBatchLogger.MaxLogLines + buffer)
            {
                textBoxLog.ReadOnly = false; // Make text box editable. This is required for the following to work
                textBoxLog.SelectionStart = 0;
                textBoxLog.SelectionLength =
                    textBoxLog.GetFirstCharIndexFromLine(numLines - SkylineBatchLogger.MaxLogLines);
                textBoxLog.SelectedText = string.Empty;

                var message = (_skylineBatchLogger != null)
                    ? string.Format(SkylineBatchLogger.LogTruncatedMessage, _skylineBatchLogger.GetFile())
                    : "... Log truncated ...";
                textBoxLog.Text = textBoxLog.Text.Insert(0, message + Environment.NewLine);
                textBoxLog.SelectionStart = 0;
                textBoxLog.SelectionLength = textBoxLog.GetFirstCharIndexFromLine(1); // 0-based index
                textBoxLog.SelectionColor = Color.Red;

                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.SelectionColor = textBoxLog.ForeColor;
                textBoxLog.ReadOnly = true; // Make text box read-only
            }
        }

        public void LogErrorToUi(string text, bool scrollToEnd, bool trim)
        {
            RunUi(() =>
            {
                if (trim)
                {
                    TrimDisplayedLog();
                }

                textBoxLog.SelectionStart = textBoxLog.TextLength;
                textBoxLog.SelectionLength = 0;
                textBoxLog.SelectionColor = Color.Red;
                LogToUi(text, scrollToEnd,
                    false); // Already trimmed
                textBoxLog.SelectionColor = textBoxLog.ForeColor;
            });
        }

        public void LogLinesToUi(List<string> lines)
        {
            RunUi(() =>
            {
                foreach (var line in lines)
                {
                    textBoxLog.AppendText(line);
                    textBoxLog.AppendText(Environment.NewLine);
                }
            });
        }

        public void LogErrorLinesToUi(List<string> lines)
        {
            RunUi(() =>
            {
                var selectionStart = textBoxLog.SelectionStart;
                foreach (var line in lines)
                {
                    textBoxLog.AppendText(line);
                    textBoxLog.AppendText(Environment.NewLine);
                }

                textBoxLog.Select(selectionStart, textBoxLog.TextLength);
                textBoxLog.SelectionColor = Color.Red;
            });
        }



        #endregion


        #region Install skyline

        private void buttonFileDialogSkylineInstall_click(object sender, EventArgs e)
        {
            using (var folderBrowserDlg = new FolderBrowserDialog())
            {
                folderBrowserDlg.Description = Resources.Select_the_skyline_installation_directory; //Select the Skyline installation directory.
                folderBrowserDlg.ShowNewFolderButton = false;
                folderBrowserDlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (folderBrowserDlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxSkylinePath.Text = folderBrowserDlg.SelectedPath;
                }
            }
        }

        private void WebBasedInstall_Click(object sender, EventArgs e)
        {
            textBoxSkylinePath.Enabled = false;
            buttonFileDialogSkylineInstall.Enabled = false;
        }

        private void SpecifyInstall_Click(object sender, EventArgs e)
        {
            textBoxSkylinePath.Enabled = true;
            buttonFileDialogSkylineInstall.Enabled = true;
        }

        #endregion


        #region Mainform event handlers and errors

        private void systray_icon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            systray_icon.Visible = false;
        }


        private void MainForm_Resize(object sender, EventArgs e)
        {
            //If the form is minimized hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (WindowState == FormWindowState.Minimized && Settings.Default.MinimizeToSystemTray)
            {
                Hide();
                systray_icon.Visible = true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            configManager.Close();
        }

        private void ShowErrorDialog(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void DisplayError(string title, string message)
        {
            RunUi(() => { ShowErrorDialog(title, message); });
        }

        #endregion



    }



    public interface IMainUiControl
    {
        //void ChangeConfigUiStatus(ConfigRunner configRunner);
        void AddConfiguration(SkylineBatchConfig config);
        void EditConfiguration(SkylineBatchConfig oldVersion, SkylineBatchConfig newVersion);
        void UpdateUiConfigurations();

        void UpdateUiLogFiles();
        void UpdateRunningButtons(bool isRunning);
        
        void LogToUi(string text, bool scrollToEnd = true, bool trim = true);
        void LogErrorToUi(string text, bool scrollToEnd = true, bool trim = true);
        void LogLinesToUi(List<string> lines);
        void LogErrorLinesToUi(List<string> lines);
        void DisplayError(string title, string message);
    }
}
