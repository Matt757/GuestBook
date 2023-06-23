import { BlobServiceClient} from '/@azure/storage-blob';

async function uploadImage() {
    // const { BlobServiceClient } = require('@azure/storage-blob');
    
    const blobStorageConnectionString = "BlobEndpoint=https://storagegbproject.blob.core.windows.net/;QueueEndpoint=https://storagegbproject.queue.core.windows.net/;FileEndpoint=https://storagegbproject.file.core.windows.net/;TableEndpoint=https://storagegbproject.table.core.windows.net/;SharedAccessSignature=sv=2022-11-02&ss=bfqt&srt=c&sp=rwdlacupiytfx&se=2023-06-28T23:13:07Z&st=2023-06-23T15:13:07Z&spr=https,http&sig=F%2BCv0CAytBwqBqrLhTLygHqoO5s6KA6HwGiuq8awPbM%3D";
    const blobStorageContainerName = "fileupload";
    const fileInput = document.getElementById('image');
    const container = new BlobServiceClient(blobStorageConnectionString).getContainerClient(blobStorageContainerName);
    const blob = container.getBlobClient(fileInput);
    const stream = fs.createReadStream(fileInput);

    await blob.uploadStream(stream);

    console.log('Upload completed.');

}