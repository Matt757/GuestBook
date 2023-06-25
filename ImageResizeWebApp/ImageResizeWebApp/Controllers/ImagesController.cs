using ImageResizeWebApp.Helpers;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Models;
using Azure.Data.Tables;

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
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

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

        [HttpPost("/api/images/review")]
        public async Task<IActionResult> AddReview(string userReview, string userImageName)
        {
            
            // Create a new ReviewEntity instance
            Guid uuid = Guid.NewGuid();
            string partitionKey = uuid.ToString();
            uuid = Guid.NewGuid()
            string rowKey = uuid.ToString();
            var reviewEntity = new ReviewEntity(partitionKey, rowKey)
            {
                review = userReview,
                imageName = userImageName
            };
            response = AzureTables.InsertEntity("blobstoragegb", "DefaultEndpointsProtocol=https;AccountName=blobstoragegb;AccountKey=RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==;EndpointSuffix=core.windows.net", "tablestoragegb", JsonConvert.SerializeObject(reviewEntity));
        }
        //     string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=blobstoragegb;AccountKey=RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==;EndpointSuffix=core.windows.net";
        //     CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        //     CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        //     
        //     string tableName = "tablestoragegb";
        //     CloudTable table = tableClient.GetTableReference(tableName);
        //     await table.CreateIfNotExistsAsync();
        //     
        //     Guid uuid = Guid.NewGuid();
        //     string partitionKey = uuid.ToString();
        //     uuid = Guid.NewGuid()
        //     string rowKey = uuid.ToString();
        //
        //     var entity = new MyEntity(partitionKey, rowKey)
        //     {
        //         review = userReview,
        //         imageName = userImageName
        //     };
        //     // // Create a reference to the table
        //     // CloudTable table = tableClient.GetTableReference(tableName);
        //     //
        //     // // Create a new ReviewEntity instance
        //     // var reviewEntity = new ReviewEntity("PartitionKey", "RowKey")
        //     // {
        //     //     Review = review,
        //     //     ImageName = imageName
        //     // };
        //     //
        //     // // Create an Insert operation
        //     // TableOperation insertOperation = TableOperation.Insert(reviewEntity);
        //     //
        //     // // Execute the operation
        //     // await table.ExecuteAsync(insertOperation);
        //     //
        //     // return Ok("Review added to table storage.");
        // }
    }
}