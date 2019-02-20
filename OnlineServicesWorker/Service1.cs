using LNF;
using LNF.CommonTools;
using LNF.Impl.DependencyInjection.Default;
using LNF.Models.Mail;
using LNF.Models.Worker;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace OnlineServicesWorker
{
    public partial class Service1 : ServiceBase
    {
        private const string QUEUE_PATH = @".\private$\osw";
        private Thread _thread;

        public static readonly string InstallServiceName = "OnlineServicesWorker";

        public Service1()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
            Program.ConsoleWriteLine("Press any key to exit.");
        }

        protected override void OnStart(string[] args)
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();

            var msgq = GetMessageQueue();

            // When the service starts the queue should be cleared so we don't process a bunch of stale messages
            msgq.Purge();

            _thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Message msg = msgq.Receive();
                        msg.Formatter = new XmlMessageFormatter(new[] { typeof(WorkerRequest) });
                        WorkerRequest req = (WorkerRequest)msg.Body;

                        BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RECV: {req.Command}", ConsoleColor.White);

                        var handler = new RequestHandler(req);
                        var sw = Stopwatch.StartNew();
                        string message = handler.Start();
                        sw.Stop();

                        BroadcastLogMessage(message.TrimEnd(Environment.NewLine.ToCharArray()), ConsoleColor.Yellow);
                        BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed in {sw.Elapsed.TotalSeconds:0.0000}", ConsoleColor.White);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Program.ConsoleWriteLine(ex.ToString(), ConsoleColor.Red);
                            BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.ToString()}", ConsoleColor.Red);

                            string errorEmail = Utility.GetGlobalSetting("OnlineServicesWorker_SendErrorEmail");
                            if (!string.IsNullOrEmpty(errorEmail))
                            {
                                SendEmail.SendSystemEmail("OnlineServicesWorker", $"OnlineServicesWorker *** ERROR *** [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]", ex.ToString(), errorEmail.Split(','), false);
                            }
                        }
                        catch { }
                    }
                }
            });

            _thread.Start();

            BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Service started.");
        }

        protected override void OnStop()
        {

        }

        private MessageQueue GetMessageQueue()
        {
            MessageQueue result;

            if (!MessageQueue.Exists(QUEUE_PATH))
            {
                result = MessageQueue.Create(QUEUE_PATH);
                result.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
            }
            else
                result = new MessageQueue(QUEUE_PATH);

            return result;
        }

        private void BroadcastLogMessage(string text, ConsoleColor clr = ConsoleColor.Gray)
        {
            Program.ConsoleWriteLine(text, clr);

            var host = Utility.GetRequiredAppSetting("ApiBaseUrl");
            var username = Utility.GetRequiredAppSetting("BasicAuthUsername");
            var password = Utility.GetRequiredAppSetting("BasicAuthPassword");

            LogMessage logMsg = new LogMessage()
            {
                Text = text,
                Color = GetHexColor(clr)
            };

            IRestClient client = new RestClient(host)
            {
                Authenticator = new HttpBasicAuthenticator(username, password)
            };

            IRestRequest req = new RestRequest("worker/api/broadcast", Method.POST);
            req.AddJsonBody(logMsg);

            var resp = client.Execute(req);

            EnsureSuccess(resp);
        }

        private void EnsureSuccess(IRestResponse resp)
        {
            if (!resp.IsSuccessful || resp.ErrorException != null)
            {
                if (resp.ErrorException != null)
                    throw resp.ErrorException;

                if (!string.IsNullOrEmpty(resp.ErrorMessage))
                    throw new Exception(resp.ErrorMessage);

                Exception ex = null;

                try
                {
                    var err = JsonConvert.DeserializeAnonymousType(resp.Content, new { Message = "" });
                    ex = new Exception(err.Message);
                }
                catch { }

                if (ex == null)
                    ex = new Exception($"[{(int)resp.StatusCode}] {resp.StatusDescription}");

                throw ex;
            }
        }

        private string GetHexColor(ConsoleColor clr)
        {
            var c = Color.FromName(clr.ToString());
            var result = string.Format("#{0:x6}", c.ToArgb() & 0xFFFFFF);
            return result;
        }

        public static bool IsServiceInstalled()
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == InstallServiceName);
        }

        public static void InstallService()
        {
            if (IsServiceInstalled())
            {
                UninstallService();
            }

            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        public static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
    }
}
