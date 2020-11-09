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
using System.Windows.Forms;

namespace SkylineBatch
{
    public partial class LogForm : Form
    {
        private readonly ConfigManager _configManager;
        public LogForm(ConfigManager configManager)
        {
            _configManager = configManager;
            InitializeComponent();

            if (_configManager.Logs.Count > 0)
                checkedListLogs.Items.AddRange(_configManager.GetOldLogFiles());
        }
        

        private void btnOk_Click(object sender, EventArgs e)
        {
            //checkedListLogs.CheckedItems
            var deletingLogs = new object[checkedListLogs.CheckedItems.Count];
            checkedListLogs.CheckedItems.CopyTo(deletingLogs, 0);
            _configManager.DeleteLogs(deletingLogs);
            Close();
        }

        private void checkBoxSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListLogs.Items.Count; i++)
            {
                checkedListLogs.SetItemChecked(i, checkBoxSelectAll.Checked);
            }
        }

        private void checkedListLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxSelectAll.Checked = false;
        }
    }
}
