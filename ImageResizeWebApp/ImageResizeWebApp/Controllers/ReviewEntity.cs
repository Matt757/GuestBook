using Microsoft.WindowsAzure.Storage.Table;

public class ReviewEntity : TableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string review { get; set; }
    public string imageName { get; set; }
}