using LNF;
using LNF.CommonTools;
using LNF.DependencyInjection;
using LNF.Impl.DependencyInjection;
using LNF.Worker;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Configuration;
using System.Configuration.Install;
using System.Drawing;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace OnlineServicesWorker
{
    public partial class Service1 : ServiceBase
    {
        private const string QUEUE_PATH = @".\private$\osw";

        private IContainerContext _context;
        private IProvider _provider;
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
            ContainerContextFactory.Current.NewThreadScopedContext();
            _context = ContainerContextFactory.Current.GetContext();
            var cfg = new ThreadStaticContainerConfiguration(_context);
            cfg.RegisterAllTypes();
            _provider = _context.GetInstance<IProvider>();
            ServiceProvider.Setup(_provider);

            var msgq = GetMessageQueue();

            // When the service starts the queue should be cleared so we don't process a bunch of stale messages
            if (ConfigurationManager.AppSettings["PurgeQueueOnStart"] == "true")
                msgq.Purge();

            _thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Message msg = msgq.Receive();

                        msg.Formatter = new XmlMessageFormatter(new[]
                        {
                            typeof(WorkerRequest),
                            typeof(UpdateBillingWorkerRequest)
                        });

                        WorkerRequest req = (WorkerRequest)msg.Body;

                        using (_provider.DataAccess.StartUnitOfWork())
                        {
                            var handler = new RequestHandler(_provider);
                            DateTime start = DateTime.Now;

                            BroadcastLogMessage($"[{start:yyyy-MM-dd HH:mm:ss}] RECV: {req.Command}", ConsoleColor.White);

                            string message = handler.Start(req);

                            DateTime end = DateTime.Now;

                            BroadcastLogMessage(message.TrimEnd(Environment.NewLine.ToCharArray()), ConsoleColor.Yellow);
                            BroadcastLogMessage($"[{end:yyyy-MM-dd HH:mm:ss}] Completed in {(end - start).TotalSeconds:0.0000}", ConsoleColor.White);

                            SendSuccessEmail(start, end, req.Command, message);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Program.ConsoleWriteLine(ex.ToString(), ConsoleColor.Red);
                            BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}", ConsoleColor.Red);
                            SendErrorEmail(ex);
                        }
                        catch { }
                    }
                }
            });

            _thread.Start();

            BroadcastLogMessage($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Service started.");
        }

        protected override void OnStop() { }

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

            if (!_provider.IsProduction())
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

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

        private void SendErrorEmail(Exception ex)
        {
            string errorEmail = GlobalSettings.Current.GetGlobalSetting("OnlineServicesWorker_SendErrorEmail");
            if (!string.IsNullOrEmpty(errorEmail))
            {
                SendEmail.SendSystemEmail("OnlineServicesWorker.Service1.SendErrorEmail", $"OnlineServicesWorker *** ERROR *** [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]", ex.ToString(), errorEmail.Split(','), false);
            }
        }

        private void SendSuccessEmail(DateTime start, DateTime end, string command, string message)
        {
            string successEmail = GlobalSettings.Current.GetGlobalSetting("OnlineServicesWorker_SendSuccessEmail");
            if (!string.IsNullOrEmpty(successEmail))
            {
                var span = end - start;
                string body = $"Job: {command}\nStarted at {start:yyyy-MM-dd HH:mm:ss}\nEnded at {end:yyyy-MM-dd HH:mm:ss}\nCompleted in {span.TotalSeconds} seconds\n--------------------------------------------------\n{message}";
                SendEmail.SendSystemEmail("OnlineServicesWorker.Service1.SendSuccessEmail", $"OnlineServicesWorker Job Completed Successfully [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]", body, successEmail.Split(','), false);
            }
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
