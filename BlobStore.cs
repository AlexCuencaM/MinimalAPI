using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace Store;
public class BlobStore{
    public BlobServiceClient MyServiceClient {get; set;}
    public UploadFile UploadFileHelper {get; set; }
    public BlobContainerClient? Container {get; set; }
    public string ContainerName {get; set;}
    public BlobStore(string stringConnection, UploadFile manager){
        MyServiceClient = new(stringConnection);
        UploadFileHelper = manager;
        ContainerName = $"videocontainer-{DateTime.Today.ToString("yyyy-MM-dd")}";
    }
    public async Task CreateContainerClient () {
        Container = MyServiceClient.GetBlobContainerClient(ContainerName);
        if(Container is null){
            Container = await MyServiceClient.CreateBlobContainerAsync(ContainerName);
        }
    } 

    public async Task<Azure.Response<BlobContentInfo>> UploadBlob(IFormFile file){
        UploadFileHelper.CreateLocalFilePath();
        await UploadFileHelper.WriteToNewFile(file);
        BlobClient? blobClient = Container?.GetBlobClient(file.FileName);
        if(blobClient is null) throw new Exception("Unexpected error at getting an blobClient");
        var response = await blobClient.UploadAsync(UploadFileHelper.FileLocalPath, true);
        return response;
    }
    
}

public class UploadFile{
    public string FileName {get; set;} = String.Empty;
    public string FileLocalPath {get; set;} = String.Empty;
    public UploadFile(string filename){
        FileName = filename;
    }
    public void CreateLocalFilePath (){
        string localPath = "data";
        Directory.CreateDirectory(localPath);
        string fileName = Guid.NewGuid().ToString() + FileName;
        FileLocalPath = Path.Combine(localPath, fileName);
    }
    public async Task WriteToNewFile(IFormFile file){
        if(string.IsNullOrEmpty(FileLocalPath)) throw new InvalidDataException("FileName must be created");
        using var stream = File.OpenWrite(FileLocalPath);
        await file.CopyToAsync(stream);
        // await File.WriteAllBytesAsync(FileLocalPath, fileAsBytes);
    }
}

public static class Extensions
    {
        public async static Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            byte[] buffer = new byte[16 << 10];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }