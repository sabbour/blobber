using Blobber.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blobber.Controllers
{
    public class HomeController : Controller
    {
        CloudStorageAccount storageAccount;

        public HomeController()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnection"));                
        }

        public ActionResult Upload(HttpPostedFileBase uploadedfile, string owner, string description)
        {
            if (uploadedfile != null && uploadedfile.ContentLength > 0)
            {
                #region Blob Storage code
                // Create Blob client
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Get reference to the container and create it if it doesn't exist
                CloudBlobContainer uploadContainer = blobClient.GetContainerReference("uploadcontainer");
                uploadContainer.CreateIfNotExists();

                // Set permissions
                uploadContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });

                // Get a reference to the blob
                CloudBlockBlob blob = uploadContainer.GetBlockBlobReference(uploadedfile.FileName);

                // Upload the file to the blob
                blob.UploadFromStream(uploadedfile.InputStream); 
                #endregion
                
                #region Table code
                // Create Table client
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("fileuploads");
                table.CreateIfNotExists();

                // Initialize the FileUploadEntity "row"
                var fileUploadEntity = new FileUploadEntity(owner);
                fileUploadEntity.Description = description;
                fileUploadEntity.Url = blob.Uri.ToString();
                
                // Save the row
                TableOperation insertOperation = TableOperation.Insert(fileUploadEntity);
                table.Execute(insertOperation);
                #endregion

                #region Queue code
                // Create Queue cient
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference("fileuploadsqueue");
                queue.CreateIfNotExists();

                // Create a message with the blob URL as content and add it to the queue
                CloudQueueMessage message = new CloudQueueMessage(blob.Uri.ToString());
                queue.AddMessage(message);
                #endregion
            }

            return RedirectToAction("Index");
        }

        public ActionResult Index(string owner)
        {
            // Create Table client
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("fileuploads");
            table.CreateIfNotExists();

            // Get list of rows in the table filtered by the partition key
            var query = new TableQuery<FileUploadEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, owner));
            var results = table.ExecuteQuery(query);
            ViewBag.Uploads = results.ToList();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
