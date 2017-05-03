using System;

using UIKit;

using Xb.App;
using TestSharpCifs.iOS.Helpers;
using SharpCifs.Netbios;

namespace TestSharpCifs.iOS.ViewControllers
{
    public partial class ViewController : UIViewController
    {
        public ViewController() : base("ViewController", null)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Job.Init();

            Job.Run(() =>
            {
                try
                {
                    var hosts = NbtAddress.GetHosts();

                    foreach (var nbtAddress in hosts)
                    {
                        Xb.Util.Out(nbtAddress.GetInetAddress().ToString());
                    }
                }
                catch (Exception ex)
                {
                    Xb.Util.Out(ex);
                    throw;
                }

            });

        }
    }
}