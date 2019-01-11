namespace TaskRunner
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
            this.BtnRunFiveMinuteTask = new System.Windows.Forms.Button();
            this.BtnRunDailyTask = new System.Windows.Forms.Button();
            this.BtnRunMonthlyTask = new System.Windows.Forms.Button();
            this.ChkNoEmail = new System.Windows.Forms.CheckBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnSendTestEmail = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BtnRunCommand = new System.Windows.Forms.Button();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnRunFiveMinuteTask
            // 
            this.BtnRunFiveMinuteTask.Location = new System.Drawing.Point(17, 42);
            this.BtnRunFiveMinuteTask.Name = "BtnRunFiveMinuteTask";
            this.BtnRunFiveMinuteTask.Size = new System.Drawing.Size(200, 23);
            this.BtnRunFiveMinuteTask.TabIndex = 0;
            this.BtnRunFiveMinuteTask.Text = "Run Five Minute Task";
            this.BtnRunFiveMinuteTask.UseVisualStyleBackColor = true;
            this.BtnRunFiveMinuteTask.Click += new System.EventHandler(this.BtnRunFiveMinuteTask_Click);
            // 
            // BtnRunDailyTask
            // 
            this.BtnRunDailyTask.Location = new System.Drawing.Point(223, 42);
            this.BtnRunDailyTask.Name = "BtnRunDailyTask";
            this.BtnRunDailyTask.Size = new System.Drawing.Size(200, 23);
            this.BtnRunDailyTask.TabIndex = 1;
            this.BtnRunDailyTask.Text = "Run Daily Task";
            this.BtnRunDailyTask.UseVisualStyleBackColor = true;
            this.BtnRunDailyTask.Click += new System.EventHandler(this.BtnRunDailyTask_Click);
            // 
            // BtnRunMonthlyTask
            // 
            this.BtnRunMonthlyTask.Location = new System.Drawing.Point(429, 42);
            this.BtnRunMonthlyTask.Name = "BtnRunMonthlyTask";
            this.BtnRunMonthlyTask.Size = new System.Drawing.Size(200, 23);
            this.BtnRunMonthlyTask.TabIndex = 2;
            this.BtnRunMonthlyTask.Text = "Run Monthly Task";
            this.BtnRunMonthlyTask.UseVisualStyleBackColor = true;
            this.BtnRunMonthlyTask.Click += new System.EventHandler(this.BtnRunMonthlyTask_Click);
            // 
            // ChkNoEmail
            // 
            this.ChkNoEmail.AutoSize = true;
            this.ChkNoEmail.Location = new System.Drawing.Point(17, 19);
            this.ChkNoEmail.Name = "ChkNoEmail";
            this.ChkNoEmail.Size = new System.Drawing.Size(180, 17);
            this.ChkNoEmail.TabIndex = 3;
            this.ChkNoEmail.Text = "No Email (daily and monthly only)";
            this.ChkNoEmail.UseVisualStyleBackColor = true;
            // 
            // lblOutput
            // 
            this.lblOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblOutput.Location = new System.Drawing.Point(12, 201);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(643, 23);
            this.lblOutput.TabIndex = 6;
            this.lblOutput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnSendTestEmail);
            this.groupBox1.Controls.Add(this.BtnRunFiveMinuteTask);
            this.groupBox1.Controls.Add(this.ChkNoEmail);
            this.groupBox1.Controls.Add(this.BtnRunDailyTask);
            this.groupBox1.Controls.Add(this.BtnRunMonthlyTask);
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(641, 106);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tasks";
            // 
            // BtnSendTestEmail
            // 
            this.BtnSendTestEmail.Location = new System.Drawing.Point(17, 71);
            this.BtnSendTestEmail.Name = "BtnSendTestEmail";
            this.BtnSendTestEmail.Size = new System.Drawing.Size(200, 23);
            this.BtnSendTestEmail.TabIndex = 4;
            this.BtnSendTestEmail.Text = "Send Test Email";
            this.BtnSendTestEmail.UseVisualStyleBackColor = true;
            this.BtnSendTestEmail.Click += new System.EventHandler(this.BtnSendTestEmail_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BtnRunCommand);
            this.groupBox2.Controls.Add(this.txtCommand);
            this.groupBox2.Location = new System.Drawing.Point(12, 115);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(641, 83);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Command Line";
            // 
            // BtnRunCommand
            // 
            this.BtnRunCommand.Location = new System.Drawing.Point(7, 45);
            this.BtnRunCommand.Name = "BtnRunCommand";
            this.BtnRunCommand.Size = new System.Drawing.Size(112, 23);
            this.BtnRunCommand.TabIndex = 1;
            this.BtnRunCommand.Text = "Run Command";
            this.BtnRunCommand.UseVisualStyleBackColor = true;
            this.BtnRunCommand.Click += new System.EventHandler(this.BtnRunCommand_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.Location = new System.Drawing.Point(7, 19);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(622, 20);
            this.txtCommand.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 233);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.Text = "TaskRunner";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnRunFiveMinuteTask;
        private System.Windows.Forms.Button BtnRunDailyTask;
        private System.Windows.Forms.Button BtnRunMonthlyTask;
        private System.Windows.Forms.CheckBox ChkNoEmail;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BtnRunCommand;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button BtnSendTestEmail;
    }
}

