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
        var src = ConfigurationManager.AppSettings["src"];
        if (!Directory.Exists(src))
        {
            Console.WriteLine("No such directory!");
        }
        else
        {
            string zipName = "zip_PC_" + ConfigurationManager.AppSettings["pcNum"] + ".zip";
            if (File.Exists(zipName))
            {
                File.Delete(zipName);
            }
            ZipFile.CreateFromDirectory(src, zipName);
            string result = Path.GetFullPath(zipName);

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
                    var status = client.UploadFile(
                        result,
                        ConfigurationManager.AppSettings["ftpDir"] + zipName,
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
                    Console.WriteLine(
                        "Cannot connect to FTP: " + ConfigurationManager.AppSettings["ftpUrl"]
                    );
                    Console.WriteLine("Exception " + ex.Message);
                }
            }
        }
        Console.WriteLine("Press any key to exit.................");
        Console.ReadKey();
    }
}