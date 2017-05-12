using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpCifs.Smb;
using SharpCifs.Netbios;

namespace Xb.Net
{
    public partial class SmbTree : Xb.File.Tree.TreeBase
    {
        public string Domain { get; protected set; }
        public string UserName { get; protected set; }
        protected string Password { get; set; }
        protected string ServerName { get; set; }
        private int PathStartIndex { get; set; }

        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        protected SmbTree(string serverName
                        , string path
                        , string userName = null
                        , string password = null
                        , string domain = null)
        {
            this.ServerName = serverName;
            this.UserName = userName ?? "";
            this.Password = password ?? "";
            this.Domain = domain ?? "";

            if (string.IsNullOrEmpty(this.ServerName))
                throw new ArgumentNullException(nameof(serverName), "Xb.Net.SmbTree.Constructor: serverName null");

            this.PathStartIndex = this.GetUriString("").Length;

            var newSmbFile = new SmbFile(this.GetUriString(path));
            var rootNode = new Xb.Net.SmbTree.SmbNode(this, newSmbFile);
            this.Init(rootNode);
        }


        /// <summary>
        /// Get uri-string for smb
        /// SMB接続URI文字列を生成する
        /// </summary>
        /// <returns></returns>
        protected internal string GetUriString(string path)
        {
            return SmbTree.GetUriString(this.ServerName
                                      , path
                                      , this.UserName
                                      , this.Password
                                      , this.Domain);
        }


        protected static string GetUriString(string serverName
                                           , string path
                                           , string userName = null
                                           , string password = null
                                           , string domain = null)
        {
            var prefix = new StringBuilder();
            var existPrefix = false;

            if (!string.IsNullOrEmpty(domain))
            {
                existPrefix = true;
                prefix.Append(domain);
                if (!string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    prefix.Append(";");
            }

            if (!string.IsNullOrEmpty(userName))
            {
                existPrefix = true;
                prefix.Append(userName);
                if (!string.IsNullOrEmpty(password))
                    prefix.Append(":");
            }

            if (!string.IsNullOrEmpty(password))
            {
                existPrefix = true;
                prefix.Append(password);
            }

            if (existPrefix)
                prefix.Append("@");

            var result = $"smb://{(existPrefix ? prefix.ToString() : "")}{serverName}/{path}";
            result = Xb.Net.Http.EncodeUri(result);
            result = result.Replace("#", "%23");
            return result;
        }


        protected internal string GetNodePath(string path)
        {
            return path.Substring(this.PathStartIndex);
        }


        /// <summary>
        /// Get ITree-object of SMB/Cifs-File-System with the passing path as the root
        /// 指定パスをルートにした、Treeオブジェクトを返す
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static Xb.Net.SmbTree GetTree(string serverName
                                           , string path
                                           , string userName = null
                                           , string password = null
                                           , string domain = null)
        {
            var result = new Xb.Net.SmbTree(serverName
                                          , path
                                          , userName
                                          , password
                                          , domain);
            result.RootNode.Scan();
            return result;
        }


        /// <summary>
        /// Get ITree-object of SMB/Cifs-File-System that scans all nodes under the passing path (VERY HEAVY!)
        /// 指定パス配下の全ノードをスキャンしたTreeオブジェクトを返す。重い！
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static async Task<Xb.Net.SmbTree> GetTreeRecursiveAsync(
            string serverName
            , string path
            , string userName = null
            , string password = null
            , string domain = null)
        {
            Xb.Net.SmbTree tree = null;

            await Task.Run(() =>
            {
                tree = Xb.Net.SmbTree.GetTree(serverName
                                            , path
                                            , userName
                                            , password
                                            , domain);
            }).ConfigureAwait(false);

            await tree.ScanRecursiveAsync().ConfigureAwait(false);

            return tree;
        }


        /// <summary>
        /// Validate passing path
        /// 指定パスの存在を検証する。
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="path"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static async Task<bool> Exists(string serverName
                                            , string path
                                            , string userName = null
                                            , string password = null
                                            , string domain = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var uri = SmbTree.GetUriString(serverName
                                                 , path
                                                 , userName
                                                 , password
                                                 , domain);

                    var smbFile = new SmbFile(uri);
                    return smbFile.Exists();
                }
                catch (Exception)
                {
                    return false;
                }
            }).ConfigureAwait(false);
        }



        /// <summary>
        /// Smb Server info class
        /// </summary>
        public class Server
        {
            /// <summary>
            /// HostName
            /// </summary>
            public string Name { get; internal set; } = string.Empty;

            /// <summary>
            /// IpAddress
            /// </summary>
            public IPAddress Address { get; internal set; } = null;

            /// <summary>
            /// Owned Shared Folders
            /// </summary>
            public Share[] Shares { get; internal set; } = new Share[] { };

            internal Server()
            {
            }
        }


        /// <summary>
        /// Smb-shared server, folder info
        /// </summary>
        public class Share
        {
            /// <summary>
            /// Server IPAddress string
            /// </summary>
            public string Server { get; internal set; } = string.Empty;

            /// <summary>
            /// Server HostName
            /// </summary>
            public string HostName { get; internal set; } = string.Empty;

            /// <summary>
            /// Server IPAddress
            /// </summary>
            public IPAddress Address { get; internal set; } = null;

            /// <summary>
            /// Share name
            /// </summary>
            public string Name { get; }

            internal Share(string name)
            {
                this.Name = name;
            }
        }


        



        /// <summary>
        /// Get server, shared-folders on LAN
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public static async Task<Server[]> ScanSharesAsync(string serverName = null)
        {
            return await Task.Run(() => 
            {
                var result = new List<Server>();

                SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");
                if (string.IsNullOrEmpty(serverName))
                {
                    var lan = new SmbFile("smb://", "");

                    foreach (var workgroup in lan.ListFiles())
                    {
                        try
                        {
                            foreach (var server in workgroup.ListFiles())
                            {
                                var hostName = server.GetName().TrimEnd('/').ToUpper();
                                var item = new Server()
                                {
                                    Name = hostName,
                                    Address = null
                                };

                                try
                                {
                                    var naddr = NbtAddress.GetByName(server.GetName().TrimEnd('/'));
                                    item.Name = naddr.GetHostName();
                                    item.Address = naddr.GetInetAddress();
                                }
                                catch (Exception)
                                {
                                }
                                result.Add(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            //workgroup access denied.
                        }
                    }
                }
                else
                {
                    try
                    {
                        var item = new Server()
                        {
                            Name = serverName.ToUpper(),
                            Address = null
                        };

                        try
                        {
                            var naddr = NbtAddress.GetByName(item.Name);
                            item.Name = naddr.GetHostName();
                            item.Address = naddr.GetInetAddress();
                        }
                        catch (Exception)
                        {
                        }
                        
                        result.Add(item);
                    }
                    catch (Exception)
                    {
                        //not found specified server
                    }
                }

                //if the server cannot be found, returns a null array.
                if (result.Count <= 0)
                    return result.ToArray();

                foreach (var server in result)
                {
                    try
                    {
                        var target = server.Address?.ToString() ?? server.Name;
                        var smbServer = new SmbFile($"smb://{target}/");
                        server.Shares = smbServer.ListFiles()
                                                 .Select(s => s.GetName().TrimEnd('/'))
                                                 .Where(n => n.IndexOf('$') < 0)
                                                 .Select(n => new Share(n)
                                                    {
                                                       Server = server.Name,
                                                       HostName = server.Name,
                                                       Address = server.Address
                                                    })
                                                 .ToArray();
                    }
                    catch (Exception)
                    {
                        //server access denied.
                    }
                }

                return result.ToArray();
            });
        }


        /// <summary>
        /// Get shared-folder names on server (deprecated: duplicate implementation)
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        public static async Task<Share[]> GetSharesAsync(string serverAddress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "8137");

                    var shareNamess = (new SmbFile($"smb://{serverAddress}"))
                                            .ListFiles()
                                            .Select(node => node.GetName())
                                            .Select(name => name.EndsWith("/")
                                                                ? name.Substring(0, name.Length - 1)
                                                                : name)
                                            .Where(name => name != "IPC$")
                                            .ToArray();

                    var result = new List<Share>();

                    foreach (var shareName in shareNamess)
                    {
                        result.Add(new Share(shareName)
                        {
                            Server = serverAddress,
                            Address = null,
                            HostName = null
                        });
                    }

                    return result.ToArray();
                }
                catch (Exception)
                {
                    return new Share[] { };
                }
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Get server, shared-folders on LAN (deprecated: inefficient operation.)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Please switch to ScanSharesAsync.
        /// </remarks>
        public static async Task<Share[]> GetSharesAsync()
        {
            var servers = await Xb.Net.SmbTree.GetServersAsync().ConfigureAwait(false);

            return await Task.Run(() =>
            {
                var result = new List<Share>();
                foreach (var server in servers)
                {
                    var smb = new SmbFile($"smb://{server}");

                    if (smb.Exists())
                    {
                        try
                        {
                            var shareNamess = smb.ListFiles()
                                            .Select(node => node.GetName())
                                            .Select(name => name.EndsWith("/") 
                                                                ? name.Substring(0, name.Length - 1) 
                                                                : name)
                                            .Where(name => name != "IPC$")
                                            .ToArray();

                            foreach (var shareName in shareNamess)
                            {
                                var share = new Share(shareName);
                                share.Server = server;
                                share.Address = IPAddress.Parse(server);
                                result.Add(share);
                            }

                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                return result.ToArray();
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Get SMB-servers on LAN (deprecated: inefficient operation.)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Please switch to ScanServerAsync.
        /// </remarks>
        public static async Task<string[]> GetServersAsync()
        {
            return await ServerScanner.GetServersAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get SMB-servers on passing LAN (deprecated: high-load operation.)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Please switch to ScanServerAsync.
        /// </remarks>
        public static async Task<string[]> GetServersAsync(IPAddress address)
        {
            return await ServerScanner.GetServersAsync(address).ConfigureAwait(false);
        }

        /// <summary>
        /// Socket Based Smb-Server Scanner Class (deprecated: high-load operation.)
        /// </summary>
        /// <remarks>
        /// Please switch to ScanServerAsync.
        /// </remarks>
        public class ServerScanner
        {
            public static async Task<string[]> GetServersAsync()
            {
                var instance = new ServerScanner();
                return await instance.Exec().ConfigureAwait(false);
            }

            public static async Task<string[]> GetServersAsync(IPAddress address)
            {
                var instance = new ServerScanner(address);
                return await instance.Exec().ConfigureAwait(false);
            }

            public static int ScanTimeoutMilliSecond { get; set; } = 10000;
            public static int ConnectTimeoutMilliSecound { get; set; } = 3000;
            public static int MaxSocketCount { get; set; } = 100;

            private IPAddress[] _addresses = null;


            private ServerScanner()
            {
            }

            private ServerScanner(IPAddress address)
            {
                if (address == null
                    || address.AddressFamily != AddressFamily.InterNetwork)
                {
                    throw new ArgumentOutOfRangeException("Xb.Net.SmbTree.ServerScanner: not target address");
                }
                this._addresses = new IPAddress[] { address }; 
            }

            private async Task<string[]> Exec()
            {
                return await Task.Run(() =>
                {
                    if (this._addresses == null)
                    {
                        this._addresses = Task.Run(() => Dns.GetHostAddressesAsync(Dns.GetHostName()))
                                              .ConfigureAwait(false)
                                              .GetAwaiter()
                                              .GetResult()
                                              .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                                              .ToArray();
                    }
                    
                    if (this._addresses.Length <= 0)
                        return new string[] { };
                    
                    var detectors = new List<Detector>();

                    foreach (var address in this._addresses)
                    {
                        var addrBytes = address.GetAddressBytes();

                        //自身のv4アドレスは取得可能だが、マスクが取得できないため
                        //24bitマスクとしてスキャンする。
                        for (var i = 1; i < 255; i++)
                        {
                            addrBytes[3] = (byte)i;
                            var target = new IPAddress(addrBytes);

                            while (detectors.Count(d => !d.Completed) >= MaxSocketCount)
                                Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();

                            detectors.Add(new Detector(target));
                        }
                    }

                    var limitTime = DateTime.Now.AddSeconds(ScanTimeoutMilliSecond);
                    while (detectors.Count(d => d.Completed) < (this._addresses.Length * 253))
                    {
                        if (limitTime < DateTime.Now)
                            break;

                        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    var existAddress = detectors.Where(d => d.Exists)
                                                .Select(d => d.IpAddress)
                                                .ToList();

                    foreach (var detector in detectors)
                        detector.Dispose();

                    var result = new List<string>();
                    foreach (var addr in existAddress)
                    {
                        var server = string.Join(".", addr.GetAddressBytes().Select(b => ((int)b).ToString()));
                        var smb = new SmbFile($"smb://{server}");
                        if (smb.Exists())
                            result.Add(server);
                    }

                    return result.ToArray();
                }).ConfigureAwait(false);
            }
            

            private class Detector : IDisposable
            {
                private Socket _socket;
                public IPAddress IpAddress { get; private set; }
                private DateTime _startTime;

                public bool Exists { get; private set; } = false;
                public bool Completed { get; private set; } = false;

                public Detector(IPAddress address)
                {
                    //Xb.Util.Out($"Detector.Constructor");
                    this.IpAddress = address;
                    this._socket = new Socket(address.AddressFamily
                                            , SocketType.Stream
                                            , ProtocolType.Tcp);

                    var ev = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = new System.Net.IPEndPoint(this.IpAddress, 445)
                    };
                    ev.Completed += (sender, e) =>
                    {
                        this.Completed = true;

                        //Xb.Util.Out($"Detector - Completed: Error = {e.SocketError}");
                        if (e.SocketError == SocketError.Success)
                        {
                            this.Exists = true;
                            var span = (DateTime.Now - this._startTime);
                            Xb.Util.Out($"Detector - Found: {this.IpAddress.ToString()} / {span.TotalSeconds.ToString("f3")} sec");
                        }

                        this._socket?.Dispose();
                        this._socket = null;
                    };

                    this._startTime = DateTime.Now;
                    //Xb.Util.Out($"Detector - Start: {this.IpAddress.ToString()}");
                    this._socket.ConnectAsync(ev);

                    Task.Delay(ConnectTimeoutMilliSecound).ContinueWith(t =>
                    {
                        //Xb.Util.Out($"Detector - Timeout");
                        this.Completed = true;

                        this._socket?.Dispose();
                        this._socket = null;
                    }).ConfigureAwait(false);
                }

                public void Dispose()
                {
                    this._socket?.Dispose();
                    this._socket = null;

                    this.IpAddress = null;
                }
            }
        }
    }
}
