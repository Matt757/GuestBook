using ImageResizeWebApp.Helpers;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;


namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();
                }
                else
                    return BadRequest("Look like the image couldnt upload to the storage");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest(
                        "Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
                return new ObjectResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("review/{imageName}")]
        public async Task<IActionResult> GetReviews(string imageName)
        {
            // Get Storage Information
            var accountName = "blobstoragegb";
            var accountKey = "RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==";
            
            // Set Auth
            var creds = new StorageCredentials(accountName, accountKey);
            var account = new CloudStorageAccount(creds, useHttps: true);
            
            // Connect to Storage
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("tablestoragegb");
            
            // // Define the filter condition based on the column value
            // string filter = $"ColumnName eq '{columnValue}'";
            //
            // // Retrieve entities from the table that match the filter condition
            // AsyncPageable<TableEntity> entities = table.QueryAsync<TableEntity>(filter);
            // //TODO add value to table to check connection to the table
            var obj = new ReviewEntity()
            {
                PartitionKey = Guid.NewGuid().ToString(), // Must be unique
                RowKey = Guid.NewGuid().ToString(), // Must be unique
                Review = "test",
                ImageName = "test"
            };
            
            return new ObjectResult(imageName);
        }
        
        
        // [HttpPost("review")]
        // public async Task<IActionResult> AddReview(ReviewParams reviewParams)
        // {
        //     // string review = reviewParams.Review;
        //     // string imageName = reviewParams.ImageName; 
        //     // // if (fileNames.Count >= 2)
        //     // // {
        //     // //     // Get the second element using ElementAt() method (index 1)
        //     // //     imageName = fileNames.ElementAt(1);
        //     // // }
        //     // // foreach (var formFile in files)
        //     // // {
        //     // //     if (StorageHelper.IsString(formFile))
        //     // //     {
        //     // //         if (formFile.Length > 0)
        //     // //         {
        //     // //             using (Stream stream = formFile.OpenReadStream())
        //     // //             {
        //     // //                 isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
        //     // //             }
        //     // //         }
        //     // //     }
        //     // //     else
        //     // //     {
        //     // //         return new UnsupportedMediaTypeResult();
        //     // //     }
        //     // // }
        //     // var obj = new ReviewEntity()
        //     // {
        //     //     PartitionKey = Guid.NewGuid().ToString(), // Must be unique
        //     //     RowKey = Guid.NewGuid().ToString(), // Must be unique
        //     //     Review = review,
        //     //     ImageName = imageName
        //     // };
        //     //
        //     // // Get Storage Information
        //     // var accountName = "blobstoragegb";
        //     // var accountKey = "RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==";
        //     //
        //     // // Set Auth
        //     // var creds = new StorageCredentials(accountName, accountKey);
        //     // var account = new CloudStorageAccount(creds, useHttps: true);
        //     //
        //     // // Connect to Storage
        //     // var client = account.CreateCloudTableClient();
        //     // var table = client.GetTableReference("tablestoragegb");
        //     //
        //     // var insertOperation = TableOperation.InsertOrMerge(obj);
        //     // table.ExecuteAsync(insertOperation);
        //     //
        //     return new AcceptedResult();
        // }
    }
}