using Minio;
using Minio.DataModel.Args;

namespace PetShopApp.Services;

public class MinioService
{
    private static MinioService? _instance;
    public static MinioService Instance => _instance ??= new MinioService();

    private IMinioClient _client;
    private const string BucketName = "petshop-images";

    private MinioService()
    {
        // Initialize MinIO Client
        _client = new MinioClient()
            .WithEndpoint("minio.ddedenko.ru")
            .WithCredentials("minioadmin", "DimpYTYT98!")
            .WithSSL(false) // Try false first if no valid cert, or true if configured
            .Build();
            
        InitializeBucket();
    }

    private async void InitializeBucket()
    {
        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(BucketName);
            bool found = await _client.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(BucketName);
                await _client.MakeBucketAsync(mbArgs);
                
                // Set policy to public read (simplified for demo)
                // In real prod, use Presigned URLs for everything or specific policy
                string policy = $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Effect"":""Allow"",""Principal"":{{""AWS"":[""*""]}},""Action"":[""s3:GetObject""],""Resource"":[""arn:aws:s3:::{BucketName}/*""]}}]}}";
                var spArgs = new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy);
                await _client.SetPolicyAsync(spArgs);
            }
        }
        catch (Exception ex)
        {
            // Log error or ignore in demo
            System.Diagnostics.Debug.WriteLine("MinIO Init Error: " + ex.Message);
        }
    }

    public async Task<string> UploadFileAsync(string localPath)
    {
        string objectName = Guid.NewGuid().ToString() + Path.GetExtension(localPath);
        
        try
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithFileName(localPath)
                .WithContentType("image/jpeg"); // Simplified

            await _client.PutObjectAsync(putObjectArgs);
            return objectName;
        }
        catch
        {
            throw;
        }
    }

    public string GetFileUrl(string objectName)
    {
        // Return direct URL assuming public bucket or generate presigned
        // Since we are setting public policy, direct URL is faster for UI
        return $"http://minio.ddedenko.ru/{BucketName}/{objectName}";
    }
}
