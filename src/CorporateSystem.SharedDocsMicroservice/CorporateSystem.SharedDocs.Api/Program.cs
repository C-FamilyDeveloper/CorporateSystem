using CorporateSystem.SharedDocs.Api;

public class Program
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