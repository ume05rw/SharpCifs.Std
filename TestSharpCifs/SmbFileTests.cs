using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCifs.Netbios;
using SharpCifs.Smb;
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
        public void ConnectTest()
        {
            var file = new SmbFile(this.GetUriString("FreeArea/SharpCifsTest/test.txt"));
            Assert.IsTrue(file.Exists());
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
                Assert.IsTrue((new string[]{ "taihi.7z", "test.txt", "win10スタートメニュー.txt" }).Contains(name));

                var time = file.LastModified();
                var dateteime = baseDate.AddMilliseconds(time).ToLocalTime();

                this.Out($"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}"); 
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

                this.Out($"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}");
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

                this.Out($"Name: {file.GetName()}, isDir?: {file.IsDirectory()}, Date: {dateteime.ToString("yyyy-MM-dd HH:mm:ss")}");
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
            //ローカルポートと共に、宛先ポートを変更してしまう。
            //SharpCifs.Config.SetProperty("jcifs.netbios.lport", "2137");

            //ローカルポートのみを変更する。ウェルノウンポートは管理者権限が必要なので。
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

            var nname = NbtAddress.GetByName("ume01srv");
            var addrs = nname.GetInetAddress();
            this.Out($"{addrs}");
        }
    }
}
