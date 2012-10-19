﻿/*
 * Original author: Alana Killeen <killea .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2010 University of Washington - Seattle, WA
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
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Alerts
{
    public sealed partial class MultiButtonMsgDlg : FormEx
    {
        public static string BUTTON_OK { get { return Resources.MultiButtonMsgDlg_BUTTON_OK_OK; } }
        public static string BUTTON_YES { get { return Resources.MultiButtonMsgDlg_BUTTON_YES__Yes; } }
        public static string BUTTON_NO { get { return Resources.MultiButtonMsgDlg_BUTTON_NO__No; } }

        private const int MAX_HEIGHT = 500;

        /// <summary>
        /// Show a message box with a Cancel button and one other button.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="btnText">The text to show in the non-Cancel button (DialogResult.OK)</param>
        public MultiButtonMsgDlg(string message, string btnText)
            : this(message, null, btnText, true)
        {
        }

        /// <summary>
        /// Show a message box with a Cancel button and two other buttons.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="btn0Text">The text to show in the left-most, default button (DialogResult.Yes)</param>
        /// <param name="btn1Text">The text to show in the second, non-default button (DialogResult.No)</param>
        /// <param name="allowCancel">When this is true a Cancel button is the button furthest to the
        /// right. Otherwise, only the two named buttons are visible.</param>
        public MultiButtonMsgDlg(string message, string btn0Text, string btn1Text, bool allowCancel)
        {
            InitializeComponent();

            Text = Program.Name;
            if (allowCancel)
                btn1.Text = btn1Text;
            else
            {
                btn1.Text = btn0Text;
                btnCancel.Text = btn1Text;
            }

            if (allowCancel && btn0Text != null)
                btn0.Text = btn0Text;
            else
            {
                btn0.Visible = false;
                AcceptButton = btn1;
                if (allowCancel)
                    btn1.DialogResult = DialogResult.OK;
                else
                {
                    btn1.DialogResult = DialogResult.Yes;
                    btnCancel.DialogResult = DialogResult.No;
                    CancelButton = null;
                }
            }
            int height = labelMessage.Height;
            labelMessage.Text = message;
            Height += Math.Min(MAX_HEIGHT, Math.Max(0, labelMessage.Height - height * 3));
        }

        /// <summary>
        /// Click the left-most button
        /// </summary>
        public void Btn0Click()
        {
            CheckDisposed();
            btn0.PerformClick();
        }

        /// <summary>
        /// Click the middle button
        /// </summary>
        public void Btn1Click()
        {
            CheckDisposed();
            btn1.PerformClick();
        }

        /// <summary>
        /// Click the right-most button
        /// </summary>
        public void BtnCancelClick()
        {
            CheckDisposed();
            btnCancel.PerformClick();
        }

        public string Message
        {
            get { return labelMessage.Text; }
            set { labelMessage.Text = value;}
        }
    }
}
