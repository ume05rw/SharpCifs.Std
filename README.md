SharpCifs.Std
====

Xamarin & .NET Core Ready, SMB/CIFS(Windows shared folder) Access Library.    
This is a port of [SharpCifs](https://github.com/zinkpad/SharpCifs) to .NET Standard.
  
Xamarin/.NET Core対応のSMB/CIFS(Windows共有)アクセスライブラリです。  
[SharpCifs](https://github.com/zinkpad/SharpCifs)を .NET Standard に移植したものです。  

## Description
**It's still a pre-release version!**  
You can access the Windows shared folder, NAS by Xamarin, .NET Core.(= without mpr.dll, Netapi32.dll)  
It's a little rework of [SharpCifs](https://github.com/zinkpad/SharpCifs), and The origin is [JCIFS](https://jcifs.samba.org/).  
  
**ざっくり動作確認しただけのプレリリース版です。** 不具合のお知らせは再現手順を添えて頂けるとありがたいです。  
Windowsの共有フォルダやNASへ、Xamarin/.NET Coreアプリからアクセス出来ます。  
[JCIFS](https://jcifs.samba.org/)のWindows Phone 8.1移植版だった[SharpCifs](https://github.com/zinkpad/SharpCifs)を、.NET Standardで動作するように少しだけ書き換えています。  

Supports .NET Standard 1.3 (= Xamarin.Android/iOS1.0, .NET Core1.0, .NET Framework 4.6)

## Requirement
System.Console (>= 4.3.0)  
System.Net.NameResolution (>= 4.3.0)  
System.Net.Sockets (>= 4.3.0)  
System.Security.Cryptography.Algorithms (>= 4.3.0)  
System.Security.Cryptography.Primitives (>= 4.3.0)  
System.Threading.Thread (>= 4.3.0)  

## Usage  
　  
1. [Add NuGet Package](https://www.nuget.org/packages/SharpCifs.Std/) to your project, or download this and add ref [SharpCifs.STD1.3/SharpCifs.STD1.3.csproj](https://github.com/ume05rw/SharpCifs.Std/blob/master/SharpCifs.STD1.3/SharpCifs.STD1.3.csproj)   
2. setting, ussage are same as JCIFS.  
    
　  
1. プロジェクトに[NuGetパッケージ](https://www.nuget.org/packages/SharpCifs.Std/)を追加するか、 もしくはこのソースをダウンロードの上 [SharpCifs.STD1.3/SharpCifs.STD1.3.csproj](https://github.com/ume05rw/SharpCifs.Std/blob/master/SharpCifs.STD1.3/SharpCifs.STD1.3.csproj) をプロジェクト参照してください。  
2. 設定や使い方は、JCIFSに準じます。  


Get Item in Folder:

    var folder = new SmbFile("smb://UserName:Password@ServerName/ShareName/Folder/")); //<-need last'/' in directory
    var epocDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    foreach (var item in folder.ListFiles())
    {
        var lastModDate = baseDate.AddMilliseconds(item.LastModified()).ToLocalTime();
        Console.WriteLine($"Name: {item.GetName()}, isDir?: {item.IsDirectory()}, Date: {lastModDate.ToString("yyyy-MM-dd HH:mm:ss")}"); 
    }
  
File Reading:  

    var file = new SmbFile("smb://UserName:Password@ServerName/ShareName/Folder/FileName.txt"));
    var readStream = file.GetInputStream();
    var buffer = new byte[1024*8];
    var memStream = new MemoryStream();
    int size;
    while ((size = readStream.Read(buffer, 0, buffer.Length)) > 0)
        memStream.Write(buffer, 0, size);
        
    Console.WriteLine(Encoding.UTF8.GetString(memStream.ToArray()));

Create New file and Writing:  

    var file = new SmbFile("smb://UserName:Password@ServerName/ShareName/Folder/NewFileName.txt"));
    file.CreateNewFile();
    var writeStream = file2.GetOutputStream();
    writeStream.Write(Encoding.UTF8.GetBytes("Hello!"));

## Licence
[LGPL v2.1 Licence](https://github.com/ume05rw/Xb.Core/blob/master/LICENSE)

## Author
[Do-Be's](http://dobes.jp)


## Links  
GitHub - zinkpad/SharpCifs: SharpCifs is a port of JCIFS to C#  
[https://github.com/zinkpad/SharpCifs](https://github.com/zinkpad/SharpCifs)  
  

JCIFS - The Java CIFS Client Library  
[https://jcifs.samba.org/](https://jcifs.samba.org/)  
