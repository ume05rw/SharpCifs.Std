using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpCifs.Smb;

namespace TestCharpCifsCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //**Change local port for NetBios.
            //  In many cases, use of the well-known port is restricted. **
            SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");
            //SharpCifs.Config.SetProperty("jcifs.smb.client.laddr", "192.168.254.11");

            SmbFile[] workgroups = null;
            try
            {
                var lan = new SmbFile("smb://", "");
                workgroups = lan.ListFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
                throw;
            }

            foreach (var workgroup in workgroups)
            {
                var wgName = workgroup.GetName();
                Console.WriteLine("");
                Console.WriteLine($"Workgroup: {wgName}");

                try
                {
                    var servers = workgroup.ListFiles();
                    foreach (var server in servers)
                    {
                        var svName = server.GetName();
                        Console.WriteLine($"    Server: {svName}");

                        try
                        {
                            var shares = server.ListFiles();

                            foreach (var share in shares)
                            {
                                var shName = share.GetName();
                                Console.WriteLine($"        Share: {shName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"    Server: {svName} - Access Denied");
                            //Console.WriteLine($"{ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Workgroup: {wgName} - Access Denied");
                }
            }

            var a = 1;
        }
    }
}
