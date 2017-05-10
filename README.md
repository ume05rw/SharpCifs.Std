SharpCifs.Std
====

Xamarin & .NET Core Ready, SMB/CIFS(Windows shared folder) Access Library.    
This is a port of [SharpCifs](https://github.com/zinkpad/SharpCifs) to .NET Standard.
  
Project Site:  
[http://sharpcifsstd.dobes.jp/](http://sharpcifsstd.dobes.jp/)

Xamarin/.NET Core対応のSMB/CIFS(Windows共有)アクセスライブラリです。  
[SharpCifs](https://github.com/zinkpad/SharpCifs)を .NET Standard に移植したものです。  

## Description
You can access the Windows shared folder, NAS by Xamarin, .NET Core.(= without mpr.dll, Netapi32.dll)  
It's a rework of [SharpCifs](https://github.com/zinkpad/SharpCifs), and The origin is [JCIFS](https://jcifs.samba.org/).  
  
Windowsの共有フォルダやNASへ、Xamarin/.NET Coreアプリからアクセス出来ます。  
[JCIFS](https://jcifs.samba.org/)のWindows Phone 8.1移植版だった[SharpCifs](https://github.com/zinkpad/SharpCifs)を、.NET Standardで動作するように修正しました。   

Supports .NET Standard 1.3 (= Xamarin.Android/iOS1.0, .NET Core1.0, .NET Framework 4.6)

## Requirement
System.Console (>= 4.3.0)  
System.Net.NameResolution (>= 4.3.0)  
System.Net.NetworkInformation (>= 4.3.0)  
System.Net.Sockets (>= 4.3.0)  
System.Security.Cryptography.Algorithms (>= 4.3.0)  
System.Security.Cryptography.Primitives (>= 4.3.0)  
System.Threading.Tasks (>=4.3.0)  
~System.Threading.Thread (>= 4.3.0)~ <-removed  

## Usage  

1) [Add NuGet Package](https://www.nuget.org/packages/SharpCifs.Std/) to your project, or download this and add ref [SharpCifs.STD1.3/SharpCifs.STD1.3.csproj](https://github.com/ume05rw/SharpCifs.Std/blob/master/SharpCifs.STD1.3/SharpCifs.STD1.3.csproj)   
2) setting, ussage are same as JCIFS.  
* for installation details, Go to the [Project Site - Installation.](http://sharpcifsstd.dobes.jp/#installation) *
    
　  
1) プロジェクトに[NuGetパッケージ](https://www.nuget.org/packages/SharpCifs.Std/)を追加するか、   
　もしくはこのソースをダウンロードの上 [SharpCifs.STD1.3/SharpCifs.STD1.3.csproj](https://github.com/ume05rw/SharpCifs.Std/blob/master/SharpCifs.STD1.3/SharpCifs.STD1.3.csproj) をプロジェクト参照してください。  
2) 設定や使い方は、JCIFSに準じます。  
* インストール手順詳細は、[プロジェクトページ - Installation](http://sharpcifsstd.dobes.jp/#installation) を参照ください。 *

    
　  
### Get items in shared folder: ###

    using System;
    using SharpCifs.Smb;
    
    
    //Get SmbFile-Object of a folder.
    var folder = new SmbFile("smb://UserName:Password@ServerIP/ShareName/FolderName/");

    //UnixTime
    var epocDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //List items
    foreach (SmbFile item in folder.ListFiles())
    {
        var lastModDate = epocDate.AddMilliseconds(item.LastModified())
                                  .ToLocalTime();

        var name = item.GetName();
        var type = item.IsDirectory() ? "dir" : "file";
        var date = lastModDate.ToString("yyyy-MM-dd HH:mm:ss");
        var msg = $"{name} ({type}) - LastMod: {date}";
        Console.WriteLine(msg);
    }
    
 
　  
### Read a File: ###

    using System;
    using System.IO;
    using System.Text;
    using SharpCifs.Smb;
    
    
    //Get target's SmbFile.
    var file = new SmbFile("smb://UserName:Password@ServerIP/ShareName/Folder/FileName.txt");

    //Get readable stream.
    var readStream = file.GetInputStream();

    //Create reading buffer.
    var buffer = new byte[1024*8];
    var memStream = new MemoryStream();

    //Get bytes.
    int size;
    while ((size = readStream.Read(buffer, 0, buffer.Length)) > 0)
        memStream.Write(buffer, 0, size);
    
    //Dispose readable stream.
    readStream.Dispose();
    
    Console.WriteLine(Encoding.UTF8.GetString(memStream.ToArray()));

 
　  
### Create a new File: ###

    using System.Text;
    using SharpCifs.Smb;


    //Get the SmbFile specifying the file name to be created.
    var file = new SmbFile("smb://UserName:Password@ServerIP/ShareName/Folder/NewFileName.txt");

    //Create file.
    file.CreateNewFile();

    //Get writable stream.
    var writeStream = file.GetOutputStream();

    //Write bytes.
    writeStream.Write(Encoding.UTF8.GetBytes("Hello!"));

    //Dispose writable stream.
    writeStream.Dispose();

 
　  
### Scan Servers & Shares on LAN: ###

    using System;
    using SharpCifs.Smb;


    //When using the host name when connecting,
    //When using the host name when connecting,
    //Change default local port(137) to a value larger than 1024.
    //In many cases, use of the well-known port is restricted.
    //
    // ** If possible, using IP addresses instead of host names 
    // ** to get better performance.
    //
    SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");

    //Get local workgroups.
    var lan = new SmbFile("smb://", "");
    var workgroups = lan.ListFiles();

    foreach (var workgroup in workgroups)
    {
        Console.WriteLine($"Workgroup Name = {workgroup.GetName()}");

        try
        {
            //Get servers in workgroup.
            var servers = workgroup.ListFiles();
            foreach (var server in servers)
            {
                Console.WriteLine($"{workgroup.GetName()} - Server Name = {server.GetName()}");

                try
                {
                    //Get shared folders in server.
                    var shares = server.ListFiles();

                    foreach (var share in shares)
                    {
                        Console.WriteLine($"{workgroup.GetName()}{server.GetName()} - Share Name = {share.GetName()}");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"{workgroup.GetName()}{server.GetName()} - Access Denied");
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"{workgroup.GetName()} - Access Denied");
        }
    }


## Licence
[LGPL v2.1 Licence](https://github.com/ume05rw/SharpCifs.Std/blob/master/LICENSE)

## Showcase
[Nasphotos (Xamarin.iOS implements - App Store link)](https://itunes.apple.com/us/app/nasphotos-the-simplest-photoframe/id1225087488?l=ja&ls=1&mt=8)

## Author
[Do-Be's](http://dobes.jp)


## Links
Project Site:  
[http://sharpcifsstd.dobes.jp/](http://sharpcifsstd.dobes.jp/)
  
    
GitHub - zinkpad/SharpCifs: SharpCifs is a port of JCIFS to C#  
[https://github.com/zinkpad/SharpCifs](https://github.com/zinkpad/SharpCifs)  
  

JCIFS - The Java CIFS Client Library  
[https://jcifs.samba.org/](https://jcifs.samba.org/)  
