using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using FluentFTP;

class Program
{
    static void Main()
    {
        logic();
        Console.WriteLine("Press any key to exit.................");
        Console.ReadKey();
    }

    static void logic()
    {
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("Missing configuration file!");
            return;
        }
        else
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            string src = configuration["src"];
            Console.WriteLine(src);
            if (!Directory.Exists(src))
            {
                Console.WriteLine("No such directory: " + src);
                return;
            }
            string zipName = "zip_PC_" + configuration["pcNum"] + ".zip";
            if (File.Exists(zipName))
            {
                File.Delete(zipName);
            }
            ZipFile.CreateFromDirectory(src, zipName);
            string result = Path.GetFullPath(zipName);

            if (
                configuration["ftpUrl"] == null
                && configuration["ftpLogin"] == null
                && configuration["ftpPassword"] == null
            )
            {
                Console.WriteLine("Empty FTP information!");
                return;
            }

            try
            {
                var client = new FtpClient(
                    configuration["ftpUrl"],
                    configuration["ftpLogin"],
                    configuration["ftpPassword"]
                );

                client.Connect();
                var status = client.UploadFile(
                    result,
                    configuration["ftpDir"] + zipName,
                    FtpRemoteExists.Overwrite,
                    true,
                    FtpVerify.Retry
                );

                var msg = status switch
                {
                    FtpStatus.Success => "file successfully uploaded",
                    FtpStatus.Failed => "failed to upload file",
                    _ => "unknown"
                };
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect to FTP: " + configuration["ftpUrl"]);
                Console.WriteLine("Exception " + ex.Message);
            }
            File.Delete(zipName);
        }
    }
}
