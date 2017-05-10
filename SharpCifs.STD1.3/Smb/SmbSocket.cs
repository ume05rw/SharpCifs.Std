using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using SharpCifs.Util.Sharpen;
using System.Net;

namespace SharpCifs.Smb
{
    public class SmbSocket : SocketEx
    {
        private static AddressFamily TargetAddressFamily = AddressFamily.InterNetwork;
        private static SocketType TargetSocketType = SocketType.Stream;
        private static ProtocolType TargetProtocolType = ProtocolType.Tcp;


        internal static SmbSocket GetSmbSocket(IPAddress ipAddress, int localPort)
        {
            lock (typeof(SmbSocket))
            {
                SmbSocket socket = null;
                lock (SmbConstants.Sockets)
                {
                    socket = SmbConstants.Sockets
                                         .FirstOrDefault(s => ipAddress.Equals(s.GetLocalInetAddress())
                                                              && s.GetLocalPort() == localPort);

                    if (socket == null
                        || socket.IsDisposed)
                    {
                        if (SmbConstants.Sockets.Contains(socket))
                            SmbConstants.Sockets.Remove(socket);

                        socket = new SmbSocket();
                        socket.Bind(new IPEndPoint(ipAddress, localPort));
                        SmbConstants.Sockets.Add(socket);
                    }
                }
                return socket;
            }
        }


        public bool IsDisposed => this._isDisposed;
        private bool _isDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public SmbSocket() 
            : base(TargetAddressFamily, TargetSocketType, TargetProtocolType)
        {
        }

        
        protected override void Dispose(bool disposing)
        {
            this._isDisposed = true;
            base.Dispose(disposing);
        }
    }
}
