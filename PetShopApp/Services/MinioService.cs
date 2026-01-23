using Minio;
using Minio.DataModel.Args;

namespace PetShopApp.Services;

public class MinioService
{
    private static MinioService? _instance;
    public static MinioService Instance => _instance ??= new MinioService();

    private IMinioClient _client;
    private const string BucketName = "petshop-images";
    private bool _bucketChecked = false;

    private MinioService()
    {
        // Initialize MinIO Client
        _client = new MinioClient()
            .WithEndpoint("minio.ddedenko.ru")
            .WithCredentials("minioadmin", "DimpYTYT98!")
            .WithSSL(false)
            .Build();
    }

    private async Task EnsureBucketExists()
    {
        if (_bucketChecked) return;

        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(BucketName);
            bool found = await _client.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(BucketName);
                await _client.MakeBucketAsync(mbArgs);
                
                // Public policy
                string policy = $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Effect"":""Allow"",""Principal"":{{""AWS"":[""*""]}},""Action"":[""s3:GetObject""],""Resource"":[""arn:aws:s3:::{BucketName}/*""]}}]}}";
                var spArgs = new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy);
                await _client.SetPolicyAsync(spArgs);
            }
            _bucketChecked = true;
        }
        catch (Exception ex)
        {
            // In a UI app, we might want to throw or log
            System.Diagnostics.Debug.WriteLine("MinIO Init Error: " + ex.Message);
        }
    }

    public async Task<string> UploadFileAsync(string localPath)
    {
        await EnsureBucketExists(); // Ensure bucket exists before upload

        string objectName = Guid.NewGuid().ToString() + Path.GetExtension(localPath);
        
        try
        {
            using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(objectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType("image/jpeg");

                await _client.PutObjectAsync(putObjectArgs);
            }
            return objectName;
        }
        catch
        {
            throw;
        }
    }

    public string GetFileUrl(string objectName)
    {
        return $"http://minio.ddedenko.ru/{BucketName}/{objectName}";
    }
}