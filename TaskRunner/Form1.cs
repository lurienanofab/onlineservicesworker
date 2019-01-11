using LNF.Models.Worker;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskRunner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnRunFiveMinuteTask_Click(object sender, EventArgs e)
        {
            WorkerRequest req = RequestManager.CreateRequest("5min");
            RequestManager.SendWorkerRequest(req);
            DisplayTaskOutput("5min");
        }

        private void BtnRunDailyTask_Click(object sender, EventArgs e)
        {
            WorkerRequest req = RequestManager.CreateRequest("daily", ChkNoEmail.Checked);
            RequestManager.SendWorkerRequest(req);
            DisplayTaskOutput("daily");
        }

        private void BtnRunMonthlyTask_Click(object sender, EventArgs e)
        {
            WorkerRequest req = RequestManager.CreateRequest("monthly", ChkNoEmail.Checked);
            RequestManager.SendWorkerRequest(req);
            DisplayTaskOutput("monthly");
        }

        private void BtnRunTestTask_Click(object sender, EventArgs e)
        {
            WorkerRequest req = RequestManager.CreateRequest("test");
            RequestManager.SendWorkerRequest(req);
            DisplayTaskOutput("test");
        }

        private void DisplayTaskOutput(string task)
        {
            DisplayOutput($"Enqueued {task} task at {DateTime.Now:h:mm:ss tt}");
        }

        private void BtnSendTestEmail_Click(object sender, EventArgs e)
        {
            try
            {
                WorkerRequest req = new WorkerRequest()
                {
                    Command = "SendEmail",
                    Args = new[]
                      {
                          "taskrunner@lnf.umich.edu",
                          "jgett@umich.edu",
                          "",
                          "",
                          $"Test email from TaskRunner sent {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                          "This is a test.",
                          "false"
                      }
                };

                RequestManager.SendWorkerRequest(req);

                DisplayOutput($"Enqueued test email at {DateTime.Now:h:mm:ss tt}");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private void BtnRunCommand_Click(object sender, EventArgs e)
        {
            string[] args = txtCommand.Text.Split(' ');

            try
            {
                RequestManager.SendWorkerRequest(args);
                DisplayOutput($"Enqueued command {args[0]} at {DateTime.Now:h:mm:ss tt}");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private void DisplayOutput(string msg)
        {
            lblOutput.ForeColor = SystemColors.ControlText;
            lblOutput.Text = msg;
        }

        private void DisplayError(string msg)
        {
            lblOutput.ForeColor = Color.Red;
            lblOutput.Text = msg;
        }
    }
}
