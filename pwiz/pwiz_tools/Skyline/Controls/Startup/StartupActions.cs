/*
 * Original author: Yuval Boss <yuval .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2014 University of Washington - Seattle, WA
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

using System.Windows.Forms;
using pwiz.Skyline.EditUI;
using pwiz.Skyline.SettingsUI;

namespace pwiz.Skyline.Controls.Startup
{
    public delegate void StartupAction(SkylineWindow skylineWindow);

    public class ActionImport
    {
        public enum DataType
        {
            peptide_search,
            fasta,
            transition_list,
            proteins,
            peptides
        }

        public DataType ImportType { get; set; }
        public string FilePath { get; set; }

        public ActionImport(DataType action)
        {
            ImportType = action;
        }


        public void DoStartupAction(SkylineWindow skylineWindow)
        {
            if (skylineWindow.Visible)
            {
                OpenSkylineStartupSettingsUI(skylineWindow);
            }
            else
            {
                skylineWindow.Shown += (sender, eventArgs) => OpenSkylineStartupSettingsUI(skylineWindow);
            }
        }

        private void OpenSkylineStartupSettingsUI(SkylineWindow skylineWindow)
        {
            if (ImportType == DataType.peptide_search)
            {
                if (FilePath != null)
                    skylineWindow.LoadFile(FilePath);
                skylineWindow.ResetDefaultSettings();
                skylineWindow.ShowImportPeptideSearchDlg();
                return;
            }

            using (var settingsUI = new StartPageSettingsUI(skylineWindow))
            {
                if (settingsUI.ShowDialog(skylineWindow) == DialogResult.OK)
                {
                    skylineWindow.SetIntegrateAll(settingsUI.IsIntegrateAll);

                    switch (ImportType)
                    {
                        case DataType.fasta:
                            skylineWindow.OpenPasteFileDlg(PasteFormat.fasta);
                            break;
                        case DataType.peptides:
                            skylineWindow.OpenPasteFileDlg(PasteFormat.peptide_list);
                            break;
                        case DataType.proteins:
                            skylineWindow.OpenPasteFileDlg(PasteFormat.protein_list);
                            break;
                        case DataType.transition_list:
                            skylineWindow.OpenPasteFileDlg(PasteFormat.transition_list);
                            break;
                    }
                }
            }
        }
    }

    public class ActionOpenDocument
    {
        public ActionOpenDocument(string path)
        {
             FilePath = path;
        }

        public string FilePath { get; private set; }

        public void DoStartupAction(SkylineWindow skylineWindow)
        {
            skylineWindow.LoadFile(FilePath);
        }
    }
}