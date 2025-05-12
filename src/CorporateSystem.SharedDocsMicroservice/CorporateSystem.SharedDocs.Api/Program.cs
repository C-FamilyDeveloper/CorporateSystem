using CorporateSystem.SharedDocs.Api;

public partial class Program
{
    private static void Main(string[] args) 
    {
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(8093);
                });
            })
            .Build()
            .Run();  
    }
}