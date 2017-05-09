using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpCifs.Smb;

namespace TestCharpCifsCore
{
    public class Program
    {
        private class AuthInfo
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private static AuthInfo Info = new AuthInfo();
        private static NtlmPasswordAuthentication Auth;

        public static void Main(string[] args)
        {
            if (!Secrets.HasSecrets)
                Console.WriteLine("FUCK OFF!!!!");


            Info.ServerName = Secrets.Get("ServerName");
            Info.ServerIP = Secrets.Get("ServerIp");
            Info.UserName = Secrets.Get("UserName");
            Info.Password = Secrets.Get("Password");

            Auth = new NtlmPasswordAuthentication(null, Info.UserName, Info.Password);

            //**Change local port for NetBios.
            //  In many cases, use of the well-known port is restricted. **
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");


            Program.NameResolutionTest();

            Program.LanScanTest();

            var end = 1;
        }


        private static void NameResolutionTest()
        {
            var namedServer = new SmbFile("smb://ume01srv/apps/", Auth);
            var exists = namedServer.Exists();

            var list = namedServer.ListFiles();
            foreach (var smb in list)
                Out($"{smb.GetName()}");

        }

        private static void LanScanTest()
        {


            var lan = new SmbFile("smb://", "");
            var workgroups = lan.ListFiles();

            foreach (var workgroup in workgroups)
            {
                var wgName = workgroup.GetName();
                Out("");
                Out($"Workgroup: {wgName}");

                try
                {
                    var servers = workgroup.ListFiles();
                    foreach (var server in servers)
                    {
                        var svName = server.GetName();
                        Out($"    Server: {svName}");

                        try
                        {
                            var shares = server.ListFiles();

                            foreach (var share in shares)
                            {
                                var shName = share.GetName();
                                Out($"        Share: {shName}");
                            }
                        }
                        catch (Exception)
                        {
                            Out($"    Server: {svName} - Access Denied");
                        }
                    }
                }
                catch (Exception)
                {
                    Out($"Workgroup: {wgName} - Access Denied");
                }
            }
        }



        private static void Out(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}  ThID: {System.Environment.CurrentManagedThreadId.ToString().PadLeft(4)}]: {message}");
        }
    }
}
