using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blobber.Models
{
    public class FileUploadEntity : TableEntity
    {
        public string Description { get; set; }
        public string Url { get; set; }
        public FileUploadEntity(string owner)
        {
            PartitionKey = owner;
            RowKey = Guid.NewGuid().ToString();
        }
        public FileUploadEntity()
        {

        }
    }
}