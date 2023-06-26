using Microsoft.WindowsAzure.Storage.Table;

public class ReviewEntity : TableEntity
{
    public string review { get; set; }
    public string imageName { get; set; }
    
    public override string ToString()
    {
        // Customize the string representation as per your requirements
        return $"PartitionKey: {PartitionKey}, RowKey: {RowKey}, review: {review}, imageName: {imageName}";
    }
}