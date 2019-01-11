using LNF.Models.Service;
using System;
using System.Messaging;

namespace OnlineServicesExecute
{
    public static class MessageManager
    {
        public static void SendWorkerRequest(WorkerRequest req)
        {
            var msgq = GetMessageQueue();
            var msg = GetMessage(req);
            msgq.Send(msg);
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
