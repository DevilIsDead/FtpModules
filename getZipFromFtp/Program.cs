using System;
using System.IO;
using System.Configuration;
using System.IO.Compression;
using FluentFTP;

class Program
{
    static void Main()
    {
        ConfigurationManager.RefreshSection("src");
        ConfigurationManager.RefreshSection("pcNum");
        ConfigurationManager.RefreshSection("ftpUrl");
        ConfigurationManager.RefreshSection("ftpPort");
        ConfigurationManager.RefreshSection("ftpLogin");
        ConfigurationManager.RefreshSection("ftpPassword");
        ConfigurationManager.RefreshSection("ftpDir");

        string zipName = "zip_PC_" + ConfigurationManager.AppSettings["pcNum"] + ".zip";

        if (
            ConfigurationManager.AppSettings["ftpUrl"] == null
            && ConfigurationManager.AppSettings["ftpLogin"] == null
            && ConfigurationManager.AppSettings["ftpPassword"] == null
        )
        {
            Console.WriteLine("Empty FTP information!");
        }
        else
        {
            try
            {
                var client = new FtpClient(
                    ConfigurationManager.AppSettings["ftpUrl"],
                    ConfigurationManager.AppSettings["ftpLogin"],
                    ConfigurationManager.AppSettings["ftpPassword"]
                );

                client.Connect();
                var status = client.DownloadFile(
                    ConfigurationManager.AppSettings["baseDir"] + zipName,
                    ConfigurationManager.AppSettings["ftpDir"] + zipName,
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
                    "Cannot connect to FTP: " + ConfigurationManager.AppSettings["ftpUrl"]
                );
                Console.WriteLine("Exception " + ex.Message);
            }

            if (!File.Exists(ConfigurationManager.AppSettings["baseDir"] + zipName)) {
                Console.WriteLine("Error! File was not downloaded!");
            } else { 
                string dest = "";
                dest += ConfigurationManager.AppSettings["baseDir"];
                ZipFile.ExtractToDirectory(ConfigurationManager.AppSettings["baseDir"] + zipName, dest, true);
                File.Delete(ConfigurationManager.AppSettings["baseDir"] + zipName);
            }
        }

        Console.WriteLine("Press any key to exit.................");
        Console.ReadKey();
    }
}
