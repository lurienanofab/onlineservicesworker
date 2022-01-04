using LNF;
using LNF.CommonTools;
using LNF.Data;
using LNF.Scheduler;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;

namespace OnlineServicesWorker
{
    public class RoomAccessExpirationCheck
    {
        public int Run()
        {
            var dataFeed = GetDataFeed();
            return EmailExpiringCards(dataFeed.Data.Items(new ExpiringCardConverter()));
        }

        private int EmailExpiringCards(IEnumerable<ExpiringCard> data)
        {
            int count = 0;

            foreach (var item in data)
            {
                if (!item.Email.StartsWith("none"))
                {
                    try
                    {
                        // per Sandrine as of 2017-10-02 [jg]
                        string bodyTemplate = "Dear {1} {2},{0}{0}You are receiving this email because your safety training - and therefore your LNF access - will expire on {3:M/d/yyyy}. In order to ensure continuous lab access, please complete the online training by following the instructions in step 5 (new user training) of the following page:{0}- for internal users: {4}{0}- for external academic users: {5}{0}- for external non-academic users: {6}{0}{0}The system will ask you to login with your LNF Online Services credentials before completing the safety training.{0}{0}If you no longer need access to the LNF, or if you have any question, please let us know.{0}{0}Thank you,{0}LNF Staff";

                        var links = new
                        {
                            internalUsers = "https://LNF.umich.edu/getting-access/internal-users/",
                            externalAcademicUsers = "https://LNF.umich.edu/getting-access/external-academic-users/",
                            nonAcademicUsers = "https://LNF.umich.edu/getting-access/non-academic-users/"
                        };

                        var expireDate = GetMinDateTime(item.CardExpireDate, item.BadgeExpireDate);

                        var addr = new MailAddress(item.Email);
                        string from = "lnf-access@umich.edu";
                        string subj = "LNF Access Card Expiring Soon";
                        string body = string.Format(bodyTemplate,
                            Environment.NewLine,
                            item.FName,
                            item.LName,
                            expireDate,
                            links.internalUsers,
                            links.externalAcademicUsers,
                            links.nonAcademicUsers);

                        string[] cc = GetExpiringCardsEmailRecipients();

                        SendEmail.Send(0, "LNF.Scheduler.Service.Process.CheckClientIssues.EmailExpiringCards", subj, body, from, new[] { item.Email }, cc);

                        count++;
                    }
                    catch
                    {
                        //invalid address so skip it
                    }
                }
            }

            return count;
        }

        public DataFeedResult GetDataFeed()
        {
            var host = GetApiBaseUrl();

            IRestClient client = new RestClient(host);
            IRestRequest req = new RestRequest("data/feed/expiring-cards/json");
            IRestResponse<DataFeedResult> resp = client.Execute<DataFeedResult>(req);

            if (resp.IsSuccessful)
                return resp.Data;
            else
                throw new Exception($"The HTTP request failed. [{(int)resp.StatusCode}] {resp.StatusDescription}{Environment.NewLine}{new string('-', 50)}{Environment.NewLine}{resp.Content}");
        }

        private string[] GetExpiringCardsEmailRecipients()
        {
            var setting = GlobalSettings.Current.GetGlobalSetting("ExpiringCardsReminder_MonthlyEmailRecipient");

            if (string.IsNullOrEmpty(setting))
                return null;
            else
                return setting.Split(',');
        }

        private DateTime GetMinDateTime(DateTime d1, DateTime d2)
        {
            if (d1 < d2)
                return d1;
            else
                return d2;
        }

        private string GetApiBaseUrl()
        {
            var result = ConfigurationManager.AppSettings["ApiBaseUrl"];

            if (string.IsNullOrEmpty(result))
                throw new Exception("Missing required AppSetting: ApiBaseUrl");

            return result;
        }
    }
}