using Minio;
using Minio.DataModel.Args;
using Microsoft.Win32;
using System.IO;

namespace PetShopApp.Services;

public class MinioService
{
    private static MinioService? _instance;
    public static MinioService Instance => _instance ??= new MinioService();

    private IMinioClient? _client;
    private const string BucketName = "petshop-app-images";
    private bool _bucketChecked = false;

    private MinioService()
    {
        try
        {
            // Initialize MinIO Client
            _client = new MinioClient()
                .WithEndpoint("45.66.228.138:9000")
                .WithCredentials("minioadmin", "DimpYTYT98!")
                .WithSSL(false)
                .Build();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MinIO Client Initialization Error: {ex.Message}");
            // Depending on the app's needs, you might want to rethrow or set a flag
            // that MinIO is not operational. For now, just logging.
            _client = null; // Mark client as unusable
        }
    }

    private async Task EnsureBucketExists()
    {
        if (_client == null)
        {
            System.Diagnostics.Debug.WriteLine("MinIO: Client not initialized, cannot ensure bucket exists.");
            return;
        }
        if (_bucketChecked) return;

            var beArgs = new BucketExistsArgs().WithBucket(BucketName);
            bool found = await _client.BucketExistsAsync(beArgs);
            if (!found)
            {
                System.Diagnostics.Debug.WriteLine($"MinIO: Bucket '{BucketName}' not found. Creating...");
                var mbArgs = new MakeBucketArgs().WithBucket(BucketName);
                await _client.MakeBucketAsync(mbArgs);
                
                // Public policy
                string policy = $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Effect"":""Allow"",""Principal"":{{""AWS"":[""*""]}},""Action"":[""s3:GetObject""],""Resource"":[""arn:aws:s3:::{BucketName}/*""]}}]}}";
                var spArgs = new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy);
                await _client.SetPolicyAsync(spArgs);
                System.Diagnostics.Debug.WriteLine($"MinIO: Bucket '{BucketName}' created and policy set.");
            } else {
                System.Diagnostics.Debug.WriteLine($"MinIO: Bucket '{BucketName}' already exists.");
            }
            _bucketChecked = true;
    }

    public async Task<string> UploadFileAsync(string localPath)
    {
        if (_client == null)
        {
            System.Diagnostics.Debug.WriteLine("MinIO: Client not initialized, cannot upload file.");
            return string.Empty; // Or throw an exception
        }
        await EnsureBucketExists(); // User created bucket manually

        string objectName = Guid.NewGuid().ToString() + Path.GetExtension(localPath);
        
        // Let exception propagate to UI to see the error
        using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(GetContentType(localPath));

            await _client.PutObjectAsync(putObjectArgs);
        }
        return objectName;
    }

    public string GetFileUrl(string objectName)
    {
        return $"http://45.66.228.138:9000/{BucketName}/{objectName}";
    }

    private string GetContentType(string filePath)
    {
        string contentType = "application/octet-stream"; // Default
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext);

        if (regKey != null && regKey.GetValue("Content Type") != null)
        {
            contentType = regKey.GetValue("Content Type").ToString();
        }
        return contentType;
    }
}