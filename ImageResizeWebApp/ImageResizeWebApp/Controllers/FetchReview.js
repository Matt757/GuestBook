const { TableClient, AzureNamedKeyCredential } = require("@azure/data-tables");

async function fetchRowByColumn(tableName, columnName, columnValue) {
    const accountName = "blobstoragegb";
    const accountKey = "RK9FSZy7Z1oyKtIbSy8qOilQXW22FwcofWwdp1DoMjchWZDm8R0FVd7BZfx2+xVGsan4/GADAMi6+AStoRfMoQ==";
    const credential = new AzureNamedKeyCredential(accountName, accountKey);

    const tableClient = new TableClient(
        `https://${accountName}.table.core.windows.net`,
        tableName,
        credential,
        null
    );

    const queryOptions = {
        filter: `${columnName} eq '${columnValue}'`
    };

    const result = await tableClient.listEntities(queryOptions).byPage().next();
    const entities = result.value;

    // Process the fetched entities
    entities.forEach(entity => {
        console.log(entity);
    });
}

// Usage
const tableName = "tablestoragegb";
const columnName = "imageName";
const columnValue = "Apetri Stefan-Cosmin IMG_1080.jpg";

fetchRowByColumn(tableName, columnName, columnValue)
    .then(() => {
        console.log("Fetch completed.");
    })
    .catch(error => {
        console.error("Error fetching row:", error);
    });