using LNF.Worker;
using System;
using System.Linq;
using System.Messaging;

namespace TaskRunner
{
    public static class RequestManager
    {
        public static WorkerRequest CreateRequest(string task)
        {
            var result = new WorkerRequest() { Command = "RunTask" };

            if (task == "5min" || task == "test")
                result.Args = new[] { task };
            else
                throw new ArgumentException("Allowed values: 5min", "task");

            return result;
        }

        public static WorkerRequest CreateRequest(string task, bool noEmail)
        {
            var result = new WorkerRequest() { Command = "RunTask" };

            if (task == "daily" || task == "monthly")
                result.Args = new[] { task, noEmail ? "true" : "false" };
            else
                throw new ArgumentException("Allowed values: daily, monthly", "task");

            return result;
        }

        public static void SendWorkerRequest(WorkerRequest req)
        {
            var msgq = GetMessageQueue();
            var msg = GetMessage(req);
            msgq.Send(msg);
        }

        public static void SendWorkerRequest(string[] args)
        {
            if (args.Length > 0 && args[0].Length > 0)
            {
                WorkerRequest req = new WorkerRequest()
                {
                    Command = args[0],
                    Args = args.Skip(1).ToArray()
                };

                SendWorkerRequest(req);
            }
            else
            {
                throw new Exception($"At least one argument is required (Command).");
            }
        }

        private static MessageQueue GetMessageQueue()
        {
            var queuePath = @".\private$\osw";

            if (MessageQueue.Exists(queuePath))
                return new MessageQueue(queuePath);
            else
                throw new Exception("Queue not found.");
        }

        private static Message GetMessage(WorkerRequest body)
        {
            var result = new Message(body)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(WorkerRequest) })
            };

            return result;
        }
    }
}
