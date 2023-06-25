using Microsoft.WindowsAzure.Storage.Table;

public class ReviewEntity : TableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string Review { get; set; }
    public string ImageName { get; set; }
}