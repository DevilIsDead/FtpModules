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

            string zipName = "zip_PC_" + configuration["pcNum"] + ".zip";

            if (
                configuration["ftpUrl"] == null
                && configuration["ftpLogin"] == null
                && configuration["ftpPassword"] == null
            )
            {
                Console.WriteLine("Empty FTP information!");
                return;
            }
            else
            {
                try
                {
                    var client = new FtpClient(
                        configuration["ftpUrl"],
                        configuration["ftpLogin"],
                        configuration["ftpPassword"]
                    );

                    client.Connect();
                    var status = client.DownloadFile(
                        configuration["baseDir"] + zipName,
                        configuration["ftpDir"] + zipName,
                        FtpLocalExists.Overwrite,
                        FtpVerify.Retry
                    );

                    var msg = status switch
                    {
                        FtpStatus.Success => "file successfully downloaded",
                        FtpStatus.Failed => "failed to download file",
                        _ => "unknown"
                    };
                    Console.WriteLine(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "Cannot connect to FTP: " + configuration["ftpUrl"]
                    );
                    Console.WriteLine("Exception " + ex.Message);
                }

                if (!File.Exists(configuration["baseDir"] + zipName))
                {
                    Console.WriteLine("Error! File was not downloaded!");
                    return;
                }
                else
                {
                    string dest = "";
                    dest += configuration["baseDir"];
                    ZipFile.ExtractToDirectory(
                        configuration["baseDir"] + zipName,
                        dest,
                        true
                    );
                    File.Delete(configuration["baseDir"] + zipName);
                }
            }
        }
    }
}
