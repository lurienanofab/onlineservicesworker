using LNF.Models.Service;
using System;

namespace OnlineServicesExecute
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    string command = args[0];

                    if (command == "UpdateBilling")
                    {

                    }
                    else if (command == "RunTask")
                        RunTask(args);
                    else
                        throw new Exception($"Unknown command: {command}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void UpdateBilling(string[] args)
        {
            if (args.Length == 5)
            {
                string command = args[0];

                if (command == "UpdateBilling")
                {
                    if (DateTime.TryParse(args[1], out DateTime sd))
                    {
                        if (DateTime.TryParse(args[2], out DateTime ed))
                        {
                            if (int.TryParse(args[3], out int clientId))
                            { 
                                string billingCategories = args[3]
                            var body = new WorkerRequest()
                            {
                                Command = "UpdateBilling",
                                Args = new string[]
                                {
                            sd.ToString("yyyy-MM-dd"),
                            ed.ToString("yyyy-MM-dd"),
                            clientId.ToString(),
                            string.Join(",", billingCategories)
                                }
                            };

                            Message msg = GetMessage(body);

                            msgq.Send(msg);
                            }
                        }
                    }
                }
            }
        }

        private static void RunTask(string[] args)
        {
            if (args.Length > 0)
            {
                string command = args[0];

                if (args.Length > 1)
                {
                    string task = args[1];

                    bool noEmail = false;

                    WorkerRequest req = new WorkerRequest() { Command = command };

                    if (task == "5min")
                    {
                        req.Args = new[] { task };
                    }
                    else if (task == "daily" || task == "monthly")
                    {
                        if (args.Length > 2)
                            noEmail = bool.Parse(args[2]);

                        req.Args = new[] { task, noEmail.ToString().ToLower() };
                    }
                    else
                        throw new Exception($"Unknown task: {task}");

                    MessageManager.SendWorkerRequest(req);
                }
            }
        }
    }
}
