using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

namespace STARFIRE.FrontEnd
{
    public partial class Starfire : Form
    {
        Mem M = new Mem();
        bool IsProcOpen;
        int ProcessID;

        public Starfire()
        {
            InitializeComponent();
            Starfire_BGWorker.RunWorkerAsync();
            // Initialize the form, then set it's state to Normal so handheld devices work.
            this.WindowState = FormWindowState.Normal;
        }

        private void Starfire_Exit_Click(object sender, EventArgs e)
        {
            // Terminate the execution of the current application and exit with a success code (0).
            Environment.Exit(0);
        }

        private void Starfire_Load(object sender, EventArgs e)
        {
            // Set the form window state to normal again to insure it is the correct size while on PC or any other device.
            this.WindowState = FormWindowState.Normal;
            Starfire_Status.Text = "STATUS: N/A";
        }

        private void Starfire_BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessID = M.GetProcIdFromName("BrgGame-Steam.exe");
            if (ProcessID != 0)
            {
                IsProcOpen = M.OpenProcess(ProcessID);
                Thread.Sleep(100);
                Starfire_BGWorker.ReportProgress(0);
            }
        }

        private void Starfire_BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsProcOpen)
            {
                Starfire_Status.Text = "STATUS: GAME FOUND";
            }
            else if (!IsProcOpen)
            {
                Starfire_Status.Text = "STATUS: N/A";
            }
        }

        private void Starfire_BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Starfire_BGWorker.RunWorkerAsync();
        }
    }
}
