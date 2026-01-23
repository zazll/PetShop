namespace PetShopApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // TEMPORARY: Ensure the main application bucket exists and has policy set
        Services.MinioService.Instance.EnsureBucketExists().Wait();

        Application.Run(new Forms.LoginForm());
    }
}
