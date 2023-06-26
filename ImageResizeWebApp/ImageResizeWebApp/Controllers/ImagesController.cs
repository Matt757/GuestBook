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
            // try
            // {
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
            //     // var condition = TableQuery.GenerateFilterCondition("imageName", QueryComparisons.Equal, imageName);
            //     // var query = new TableQuery<ReviewEntity>().Where(condition);
            //     // TableQuery query = new TableQuery().Where(TableQuery.GenerateFilterCondition("imageName", QueryComparisons.Equal, imageName)).Take(1);
            //     // TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>().Where("imageName eq '" + imageName +  "'");
            //     // var entities = table.ExecuteQuery(new TableQuery<ReviewEntity>()).ToList();
            //     // var review = table.Query<ReviewEntity>(x => x.imageName == imageName);
            //     // var result = table.ExecuteQuery(query);
            //
            //     return new ObjectResult(imageName);
            // }
            // catch (Exception e)
            // {
            //     return StatusCode(500, "An error occurred: " + ex.Message);
            // }
            
            
            // Get Storage Information
            var accountName = "blobstoragegb";
            var accountKey = "RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==";
            
            // Set Auth
            var creds = new StorageCredentials(accountName, accountKey);
            var account = new CloudStorageAccount(creds, useHttps: true);
            
            // Connect to Storage
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference("tablestoragegb");
            
            var review = table.Query<ReviewEntity>(x => x.imageName == imageName);
            
            return new ObjectResult(imageName);
        }
        
        
        [HttpPost("addReview")]
        public async Task<IActionResult> AddReview(ReviewParams reviewParams)
        {
            try
            {
                string userReview = reviewParams.Review;
                string userImageName = reviewParams.ImageName;
                
                // Get Storage Information
                var accountName = "blobstoragegb";
                var accountKey = "RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==";

                // Set Auth
                var creds = new StorageCredentials(accountName, accountKey);
                var account = new CloudStorageAccount(creds, useHttps: true);

                // Connect to Storage
                var client = account.CreateCloudTableClient();
                var table = client.GetTableReference("tablestoragegb");
                
                var obj = new ReviewEntity()
                {
                    PartitionKey = "unic", // Must be unique
                    RowKey = Guid.NewGuid().ToString(), // Must be unique
                    review = userReview,
                    imageName = userImageName
                };
                TableOperation insertOperation = TableOperation.InsertOrMerge(obj);
                
                await table.ExecuteAsync(insertOperation);

                return new ObjectResult(userReview + ", " + userImageName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
    }
}