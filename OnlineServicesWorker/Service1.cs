using System.Messaging;
using System;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Threading;
using LNF;
using LNF.Impl.DependencyInjection.Default;
using LNF.Service;
using System.Diagnostics;

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

            _thread = new Thread(() =>
            {
                while (true)
                {
                    Message msg = msgq.Receive();
                    msg.Formatter = new XmlMessageFormatter(new[] { typeof(WorkerRequest) });
                    WorkerRequest req = (WorkerRequest)msg.Body;

                    try
                    {
                        Program.ConsoleWriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RECV: {req.Command}", ConsoleColor.White);
                        var handler = new RequestHandler(req);
                        var sw = Stopwatch.StartNew();
                        handler.Start();
                        sw.Stop();
                        Program.ConsoleWriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed in {sw.Elapsed}", ConsoleColor.White);
                    }
                    catch (Exception ex)
                    {
                        Program.ConsoleWriteLine(ex.Message, ConsoleColor.Red);
                    }
                }
            });

            _thread.Start();
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
