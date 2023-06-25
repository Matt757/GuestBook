using Azure.Data.Table;

public class ReviewEntity : TableEntity
{
    public ReviewEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public ReviewEntity() { } // Required for deserialization

    public string Review { get; set; }
    public string ImageName { get; set; }
}