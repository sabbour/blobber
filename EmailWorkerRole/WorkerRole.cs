using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace EmailWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public void SendEmail(string blobUrl)
        {
            try
            {
                using (var email = new System.Net.Mail.MailMessage("demo4afrika@outlook.com", "sabbour@outlook.com"))
                {
                    email.Subject = "New file uploaded";
                    email.Body = blobUrl;
                    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.live.com", 25);
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("demo4afrika@outlook.com", "p@ssw0rd.pass");
                    client.Send(email);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("EmailWorkerRole entry point called", "Information");

            // Initialize storage and queue
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnection"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("fileuploadsqueue");
            queue.CreateIfNotExists();

            while (true)
            {
                // Get a message from the queue
               var message = queue.GetMessage();
               if (message != null)
               {
                   SendEmail(message.AsString);
                   queue.DeleteMessage(message);
               }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
