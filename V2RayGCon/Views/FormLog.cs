﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace V2RayGCon.Views
{
    public partial class FormLog : Form
    {
        Service.Setting setting;

        int maxNumberLines;
        delegate void PushLogDelegate(string content);

        public FormLog()
        {
            setting = Service.Setting.Instance;
            maxNumberLines = setting.maxLogLines;

            InitializeComponent();

            this.FormClosed += (s, e) => setting.OnLog -= LogReceiver;
            this.Show();
            setting.OnLog += LogReceiver;
        }

        void LogReceiver(object sender, Model.DataEvent e)
        {
            PushLogDelegate pushLog = new PushLogDelegate(PushLog);
            textBoxLogger.Invoke(pushLog, e.Data);
        }

        public void PushLog(string content)
        {
            if (textBoxLogger.Lines.Length >= maxNumberLines - 1)
            {
                textBoxLogger.Lines = textBoxLogger.Lines.Skip(textBoxLogger.Lines.Length - maxNumberLines).ToArray();
            }
            textBoxLogger.AppendText(content + "\r\n");
        }
    }
}
