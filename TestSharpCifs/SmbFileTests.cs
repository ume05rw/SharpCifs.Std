using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCifs.Netbios;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;
using TestXb;

namespace TestSharpCifs
{
    [TestClass()]
    public class SmbFileTests : TestBase
    {
        private DateTime EpocDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private string ServerName { get; set; }
        private string UserName { get; set; }
        private string Password { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SmbFileTests()
        {
            if (Secrets.HasSecrets)
            {
                this.ServerName = Secrets.Get("ServerName");
                this.UserName = Secrets.Get("UserName");
                this.Password = Secrets.Get("Password");
            }
        }

        private string GetUriString(string path)
        {
            return $"smb://{this.UserName}:{this.Password}@{this.ServerName}/{path}";
        }



        [TestMethod()]
        public void AuthTest()
        {
            var auth1 = new NtlmPasswordAuthentication($"{this.UserName}:{this.Password}");
            var startTime = default(DateTime);

            var file = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/test.txt"));
            Assert.IsTrue(file.Exists());


            startTime = DateTime.Now;
            for (var i = 0; i < 5; i++)
            {
                var file1 = new SmbFile($"smb://{this.ServerName}/FreeArea/SharpCifsTest/test.txt", auth1);
                Assert.IsTrue(file1.Exists());
            }
            this.Out($"use auth1: {(DateTime.Now - startTime).TotalMilliseconds} msec");


            var auth2 = new NtlmPasswordAuthentication(null, this.UserName, this.Password);
            startTime = DateTime.Now;
            for (var i = 0; i < 5; i++)
            {
                var file1 = new SmbFile($"smb://{this.ServerName}/FreeArea/SharpCifsTest/test.txt", auth2);
                Assert.IsTrue(file1.Exists());
            }
            this.Out($"use auth2: {(DateTime.Now - startTime).TotalMilliseconds} msec");


            startTime = DateTime.Now;
            for (var i = 0; i < 5; i++)
            {
                var file1 =
                    new SmbFile(
                        $"smb://{this.UserName}:{this.Password}@{this.ServerName}/FreeArea/SharpCifsTest/test.txt");
                Assert.IsTrue(file1.Exists());
            }
            this.Out($"use url  : {(DateTime.Now - startTime).TotalMilliseconds} msec");
        }


        [TestMethod()]
        public void ConnectTest()
        {
            //SmbFile(java.lang.String url)
            var dir1 = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/"));
            Assert.IsTrue(dir1.Exists());

            //SmbFile(SmbFile context, java.lang.String name)
            var file1 = new SmbFile(dir1, "test.txt");
            Assert.IsTrue(file1.Exists());


            var file2 = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/not_exists_file.txt"));
            Assert.IsFalse(file2.Exists());

            var dir2 = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/not_exists_dir/"));
            Assert.IsFalse(dir2.Exists());
        }


        /// <summary>
        /// 共有アクセステスト
        /// </summary>
        /// <remarks>
        /// 排他アクセスが出来るかと思ったが、違うらしい。
        /// 本家jcifsも同じ現象が発生していた。
        /// 使い方が分からない。
        /// </remarks>
        [TestMethod()]
        public void ShareAccessFlagTest()
        {
            var sameUrl = this.GetUriString("FreeArea/SharpCifsTest/test.txt");

            //for recover
            var file = new SmbFile(sameUrl, null, SmbFile.FileShareDelete);
            Assert.IsTrue(file.Exists());
            var fileReadStream = file.GetInputStream();
            var bytes = Xb.Byte.GetBytes((Stream) fileReadStream);
            fileReadStream.Dispose();
            this.Out($"base: {Encoding.UTF8.GetString(bytes)}");
            fileReadStream.Close();


            var tmpFile = default(SmbFile);
            var reader = default(InputStream);
            var writer = default(OutputStream);

            //1)ShareAccess = FileNoShare
            var shareAccessWriteFile = new SmbFile(sameUrl, null, SmbFile.FileNoShare);
            Assert.IsTrue(shareAccessWriteFile.Exists());
            reader = file.GetInputStream();

            try
            {
                //multiple read same file
                tmpFile = new SmbFile(sameUrl);
                var tmpReader = tmpFile.GetInputStream();

                Assert.Fail();
            }
            catch (SmbException ex)
            {
                this.Out(ex); // <- OK.
            }
            catch (Exception ex)
            {
                this.Out(ex);
                reader.Dispose();
                Assert.Fail();
            }
            reader.Dispose();


            //2)ShareAccess = FileShareDelete (default)
            var shareAccessDeleteFile = new SmbFile(sameUrl, null, SmbFile.FileShareDelete);
            Assert.IsTrue(shareAccessDeleteFile.Exists());
            reader = file.GetInputStream();

            try
            {
                //multiple read same file
                tmpFile = new SmbFile(sameUrl);
                var tmpReader = tmpFile.GetInputStream();
                tmpReader.Dispose();
            }
            catch (SmbException ex)
            {
                this.Out(ex); // <- WHY HERE?

                reader.Dispose();

                //recover
                writer = file.GetOutputStream();
                writer.Write(bytes);
                writer.Dispose();

                Assert.Fail();
            }
            catch (Exception ex)
            {
                this.Out(ex);
                Assert.Fail();
            }
            reader.Dispose();


            //recover
            writer = file.GetOutputStream();
            writer.Write(bytes);
            writer.Dispose();
        }


        [TestMethod()]
        public void StreamReadTest()
        {
            var file = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/test.txt"));
            Assert.IsTrue(file.Exists());

            var readStream = file.GetInputStream();
            Assert.AreNotEqual(null, readStream);
            var stream = (Stream) readStream;
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.AreEqual(0, stream.Position);

            var text = Encoding.UTF8.GetString(Xb.Byte.GetBytes(stream));
            this.Out(text);

            readStream.Dispose();
        }


        [TestMethod()]
        public void CreateWriteDeleteTest()
        {
            var a = 1;

            var dir = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/"));
            Assert.IsTrue(dir.Exists());

            var file2 = new SmbFile(dir, "newFile.txt");

            Assert.IsFalse(file2.Exists());

            file2.CreateNewFile();

            Assert.IsTrue(file2.Exists());

            var writeStream = file2.GetOutputStream();
            Assert.AreNotEqual(null, writeStream);

            var textBytes = Encoding.UTF8.GetBytes("マルチバイト\r\n\r\n∀\r\n∀");
            writeStream.Write(textBytes);
            writeStream.Dispose();

            var readStream = file2.GetInputStream();
            Assert.AreNotEqual(null, readStream);

            var text = Encoding.UTF8.GetString(Xb.File.Util.GetBytes(readStream));
            this.Out(text);
            Assert.IsTrue(text.IndexOf("バイト") >= 0);
            readStream.Dispose();

            file2.Delete();
            Assert.IsFalse(file2.Exists());
        }


        [TestMethod()]
        public void GetListTest()
        {
            var a = 1;

            var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dir = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/"));
            Assert.IsTrue(dir.Exists());

            var list = dir.ListFiles();
            foreach (var file in list)
            {
                var name = file.GetName();
                Assert.IsTrue(Enumerable.Contains((new string[] {"taihi.7z", "test.txt", "win10スタートメニュー.txt"}), name));

                var time = file.LastModified();
                var dateteime = baseDate.AddMilliseconds(time).ToLocalTime();

                this.Out(
                    $"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }


        [TestMethod()]
        public void ConfigTest1()
        {
            var props = new SharpCifs.Util.Sharpen.Properties();
            props.SetProperty("jcifs.smb.client.username", this.UserName);
            props.SetProperty("jcifs.smb.client.password", this.Password);
            SharpCifs.Config.SetProperties(props);


            var uriString = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/";
            var dir = new SmbFile(uriString);
            Assert.IsTrue(dir.Exists());

            var list = dir.ListFiles();
            foreach (var file in list)
            {
                var name = file.GetName();
                //Assert.IsTrue(name.IndexOf("filename") >= 0);

                var time = file.LastModified();
                var dateteime = EpocDate.AddMilliseconds(time).ToLocalTime();

                this.Out(
                    $"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }


        [TestMethod()]
        public void ConfigTest2()
        {
            SharpCifs.Config.SetProperty("jcifs.smb.client.username", this.UserName);
            SharpCifs.Config.SetProperty("jcifs.smb.client.password", this.Password);

            var uriString = $"smb://{this.ServerName}/Apps/Others/";
            var dir = new SmbFile(uriString);
            Assert.IsTrue(dir.Exists());

            var list = dir.ListFiles();
            foreach (var file in list)
            {
                var name = file.GetName();
                //Assert.IsTrue(name.IndexOf("filename") >= 0);

                var time = file.LastModified();
                var dateteime = EpocDate.AddMilliseconds(time).ToLocalTime();

                this.Out(
                    $"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }


        [TestMethod()]
        public void ZipStreamReadingTest()
        {
            SharpCifs.Config.SetProperty("jcifs.smb.client.username", this.UserName);
            SharpCifs.Config.SetProperty("jcifs.smb.client.password", this.Password);
            var uriString = $"smb://{this.ServerName}/Apps/Others/[BinaryEditor] Stirling.zip";

            var zipFile = new SmbFile(uriString);
            Assert.IsTrue(zipFile.Exists());

            var readStream = zipFile.GetInputStream();


            var xbZip = new Xb.File.Zip(readStream);

            //xbZip.Entries
            foreach (var entry in xbZip.Entries)
            {
                this.Out($"{entry.Name}");
                var bytes = xbZip.GetBytes(entry);
                this.Out($"bytes.Length: {bytes.Length}");
            }
        }


        [TestMethod()]
        public void GetByNameTest()
        {
            //NG: ローカルポートと共に、宛先ポートを変更してしまう。
            //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

            //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

            var srvName1 = Secrets.Get("ServerName");
            var ipAddr = Secrets.Get("ServerName2");
            var nname = NbtAddress.GetByName(srvName1);
            var addrs = nname.GetInetAddress();
            this.Out($"{srvName1} = {addrs}");
            Assert.AreEqual(ipAddr, addrs.ToString());


            
            nname = NbtAddress.GetByName(ipAddr);
            addrs = nname.GetInetAddress();
            this.Out($"{ipAddr} = {nname.GetHostName()}");
            Assert.AreEqual(ipAddr, addrs.ToString());
        }


        [TestMethod()]
        public void GetAllByAddressTest()
        {
            //NG: ローカルポートと共に、宛先ポートを変更してしまう。
            //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

            //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

            var srvName1 = Secrets.Get("ServerName");
            var nnames = NbtAddress.GetAllByAddress(srvName1);

            foreach (var nname in nnames)
            {
                var addrs = nname.GetInetAddress();
                this.Out($"{srvName1} = {addrs}");
            }
        }


        ///// <summary>
        ///// 動くは動くけども、すごい遅い。
        ///// 検出率もいまいち
        ///// </summary>
        //[TestMethod()]
        //public void GetHostsTest()
        //{
        //    //NG: ローカルポートと共に、宛先ポートを変更してしまう。
        //    //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

        //    //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
        //    SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

        //    var nnames = NbtAddress.GetHosts();

        //    foreach (var nname in nnames)
        //    {
        //        var addrs = nname.GetInetAddress();
        //        this.Out($"{nname.GetHostName()} = {addrs}");
        //    }
        //}


        [TestMethod()]
        public void LocalScanTest()
        {
            //ローカルポートと共に、宛先ポートを変更してしまう。
            //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

            //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

            SmbFile[] workgroups;
            try
            {
                var lan = new SmbFile("smb://", "");
                workgroups = lan.ListFiles();
            }
            catch (Exception ex)
            {
                throw;
            }


            foreach (var workgroup in workgroups)
            {
                Console.WriteLine($"Workgroup Name = {workgroup.GetName()}");
                this.Out($"Workgroup Name = {workgroup.GetName()}");

                try
                {
                    var servers = workgroup.ListFiles();
                    foreach (var server in servers)
                    {
                        Console.WriteLine($"{workgroup.GetName()} - Server Name = {server.GetName()}");
                        this.Out($"{workgroup.GetName()} - Server Name = {server.GetName()}");

                        try
                        {
                            var shares = server.ListFiles();

                            foreach (var share in shares)
                            {
                                Console.WriteLine($"{workgroup.GetName()}/{server.GetName()} - Share Name = {share.GetName()}");
                                this.Out($"{workgroup.GetName()}/{server.GetName()} - Share Name = {share.GetName()}");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"{workgroup.GetName()}/{server.GetName()} - Access Denied");
                            this.Out($"{workgroup.GetName()}/{server.GetName()} - Access Denied");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{workgroup.GetName()} - Access Denied");
                    this.Out($"{workgroup.GetName()} - Access Denied");
                }

            }

            Console.WriteLine("Fin");
        }


        [TestMethod()]
        public void GetCanonicalPathTest()
        {
            var stopHere = 1;
            var auth = new NtlmPasswordAuthentication($"{this.UserName}:{this.Password}");
            string url;
            string path;
            SmbFile smb;

            //ファイル名 - 加工なし
            url = $"smb://{this.ServerName}/Apps/Others/[BinaryEditor] Stirling.zip";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual(url, path);

            //フォルダ名 - 加工なし
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual(url, path);

            //不正なフォルダ名だが、加工なし
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual(url, path);

            //相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/../";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/FreeArea/", path);

            //相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/.././";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/FreeArea/", path);

            //相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/../.";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/FreeArea/", path);

            //相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/..";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/FreeArea/", path);

            //相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/../../Apps/./Others/[BinaryEditor] Stirling.zip";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/Apps/Others/[BinaryEditor] Stirling.zip", path);

            //実際に存在するかどうかは検証せず、相対パスを絶対化
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest/../../NOT-EXISTS-SHARE/NOT-EXISTS-FILE.txt";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/NOT-EXISTS-SHARE/NOT-EXISTS-FILE.txt", path);

            //末尾にスペースを入れる
            url = $"smb://{this.ServerName}/FreeArea/SharpCifsTest  ";
            smb = new SmbFile(url, auth);
            path = smb.GetCanonicalPath();
            Assert.AreEqual($"smb://{this.ServerName}/FreeArea/SharpCifsTest", path); //スペースが無くなるはず
        }


        [TestMethod()]
        public void NbtAddressTest()
        {
            //NG: ローカルポートと共に、宛先ポートを変更してしまう。
            //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

            //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");
            try
            {
                var lan = new SmbFile("smb://", "");
                var workgroups = lan.ListFiles();
            }
            catch (Exception ex)
            {
                throw;
            }
            


            //var nbtAddrs = NbtAddress.GetAllByAddress("COCO4");
            //var nbtAddrs = NbtAddress.GetAllByAddress("127.0.0.1");
            var nbtAddrs = NbtAddress.GetAllByAddress("192.168.254.11");

            foreach (var nbtAddr in nbtAddrs)
            {
                this.Out($"{nbtAddr.GetHostName()} - {nbtAddr.GetInetAddress()}");
            }
        }
    }
}
