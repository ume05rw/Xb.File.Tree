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
        /// Get Tree-object with the passing path as the root
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
        /// Get Tree object that scans all nodes under the passing path (VERY HEAVY!)
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
            });
            await tree.ScanRecursiveAsync();

            return tree;
        }


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
            });
        }



        /// <summary>
        /// Get shared-folder names on server
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        public static async Task<Share[]> GetSharesAsync(string serverAddress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var shares = (new SmbFile($"smb://{serverAddress}"))
                                        .ListFiles()
                                        .Select(node => node.GetName())
                                        .Select(name => name.EndsWith("/") 
                                                            ? name.Substring(0, name.Length - 1) 
                                                            : name)
                                        .Where(name => name != "IPC$")
                                        .ToArray();
                    var result = new List<Share>();
                    foreach (var share in shares)
                        result.Add(new Share(serverAddress, share));

                    return result.ToArray();
                }
                catch (Exception)
                {
                    return new Share[] {};
                }
            });
        }


        public class Share
        {
            /// <summary>
            /// Server address
            /// </summary>
            public string Server { get; }

            /// <summary>
            /// Share name
            /// </summary>
            public string Name { get; }

            public Share(string server, string name)
            {
                this.Server = server;
                this.Name = name;
            }
        }

        /// <summary>
        /// Get server & shared-folders on lan
        /// </summary>
        /// <returns></returns>
        public static async Task<Share[]> GetSharesAsync()
        {
            var servers = await Xb.Net.SmbTree.GetServersAsync();

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
                            var shares = smb.ListFiles()
                                            .Select(node => node.GetName())
                                            .Select(name => name.EndsWith("/") 
                                                                ? name.Substring(0, name.Length - 1) 
                                                                : name)
                                            .Where(name => name != "IPC$")
                                            .ToArray();
                            foreach (var share in shares)
                                result.Add(new Share(server, share));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                return result.ToArray();
            });
        }


        /// <summary>
        /// Get smb-servers on lan
        /// </summary>
        /// <returns></returns>
        public static async Task<string[]> GetServersAsync()
        {
            return await ServerScanner.GetServersAsync();
        }

        /// <summary>
        /// Get smb-servers on lan
        /// </summary>
        /// <returns></returns>
        public static async Task<string[]> GetServersAsync(IPAddress address)
        {
            return await ServerScanner.GetServersAsync(address);
        }

        public class ServerScanner
        {
            public static async Task<string[]> GetServersAsync()
            {
                var instance = new ServerScanner();
                return await instance.Exec();
            }

            public static async Task<string[]> GetServersAsync(IPAddress address)
            {
                var instance = new ServerScanner(address);
                return await instance.Exec();
            }

            public static int ParallelSocketCount { get; set; } = 20;
            public static int NoResponseTimeoutMilliSecond { get; set; } = 200;
            public static int StoreResponseTimeoutSecond { get; set; } = 40;
            public static int WaitParOneScanMilliSecond { get; set; } = 100;

            private List<IPAddress> _tryAddress;
            private List<IPAddress> _existAddress;
            private IPAddress[] _addresses = null;
            private Socket[] _sockets;


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
                        this._addresses = Task.Run(() => System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()))
                                              .GetAwaiter()
                                              .GetResult()
                                              .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                                              .ToArray();
                    }
                    
                    if (this._addresses.Length <= 0)
                        return new string[] { };
                    
                    this._tryAddress = new List<IPAddress>();
                    this._existAddress = new List<IPAddress>();
                    this._sockets = new Socket[ParallelSocketCount];

                    foreach (var address in this._addresses)
                    {
                        var addrBytes = address.GetAddressBytes();

                        //自身のv4アドレスは取得可能だが、マスクが取得できないため
                        //24bitマスクとしてスキャンする。
                        for (var i = 1; i < 255; i++)
                        {
                            addrBytes[3] = (byte)i;

                            //445ポートへの接続試行を実行する。
                            //呼び出し先メソッド内の処理が全て別スレッドで動作するため、
                            //スキャンが平行する。
                            //配列をまるごと渡すと参照渡しになってしまうため、分割してbyte型を渡す。
                            this.TryConnect(addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3]);

                            //大量にスレッドを生成すると、イベントの割り込みが遅くなるらしい。
                            Thread.Sleep(WaitParOneScanMilliSecond);
                        }
                    }

                    var limitTime = DateTime.Now.AddSeconds(StoreResponseTimeoutSecond);
                    while (this._tryAddress.Count < (this._addresses.Length * 253))
                    {
                        //Xb.Util.Out($"tryAddress.Count: {this._tryAddress.Count}");
                        //Xb.Util.Out($"existAddrs.Count: {this._existAddrs.Count}");

                        if (limitTime < DateTime.Now)
                            break;

                        Thread.Sleep(50);
                    }


                    var result = new List<string>();
                    foreach (var addr in this._existAddress)
                    {
                        var server = string.Join(".", addr.GetAddressBytes().Select(b => ((int)b).ToString()));
                        var smb = new SmbFile($"smb://{server}");
                        if (smb.Exists())
                            result.Add(server);
                    }

                    return result.ToArray();
                });
            }


            //値渡しのため、バイト配列を分割して受け取る。
            private void TryConnect(byte byte1, byte byte2, byte byte3, byte byte4)
            {
                Task.Run(() =>
                {
                    var index = (int)byte4;
                    var socketIndex = index % this._sockets.Length;
                    var addrBytes = new byte[] { byte1, byte2, byte3, byte4 };

                    DateTime startTime = DateTime.MinValue;

                    //別スレッドでソケット使用中のとき、時間を置いてリトライ
                    if (this._sockets[socketIndex] != null)
                    {
                        //Xb.Util.Out($"TryConnect-Retry: {(int)byte1}.{(int)byte2}.{(int)byte3}.{(int)byte4} / socketIndex = {socketIndex}");
                        Task.Run(() =>
                        {
                            Thread.Sleep(NoResponseTimeoutMilliSecond + 50);
                            //this.TryConnect(byte1, byte2, byte3, byte4);
                        })
                        .ContinueWith(t =>
                        {
                            this.TryConnect(byte1, byte2, byte3, byte4);
                        });
                        return;
                    }

                    this._sockets[socketIndex] = new Socket((new IPAddress(addrBytes)).AddressFamily
                                                          , SocketType.Stream
                                                          , ProtocolType.Tcp);

                    //Xb.Util.Out($"TryConnect-Exec: {(int)byte1}.{(int)byte2}.{(int)byte3}.{(int)byte4} / socketIndex = {socketIndex}");

                    var ipAddress = new IPAddress(addrBytes);
                    this._tryAddress.Add(ipAddress);

                    var connected = false;

                    var ev = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = new System.Net.IPEndPoint(ipAddress, 445)
                    };
                    ev.Completed += (sender, e) =>
                    {
                        connected = true;
                        if (e.SocketError == SocketError.Success)
                        {
                            Xb.Util.Out($"TryConnect-Found: {(int)byte1}.{(int)byte2}.{(int)byte3}.{(int)byte4} / socketIndex = {socketIndex} / {startTime.ToString("HH:mm:ss.fff")}");
                            this._existAddress.Add(ipAddress);
                        }
                    };

                    startTime = DateTime.Now;
                    this._sockets[socketIndex].ConnectAsync(ev);

                    Task.Run(() =>
                    {
                        //NoResponseTimeoutMilliSec(ミリ秒)以内に応答がないとき、存在しないアドレスとする。
                        var limitTime = DateTime.Now.AddMilliseconds(NoResponseTimeoutMilliSecond);
                        while (!connected)
                        {
                            if (limitTime < DateTime.Now)
                                break;

                            Thread.Sleep(NoResponseTimeoutMilliSecond / 10);
                        }

                        this._sockets[socketIndex].Dispose();
                        this._sockets[socketIndex] = null;
                        //if (!connected) Xb.Util.Out($"TryConnect-Failed: {(int)byte1}.{(int)byte2}.{(int)byte3}.{(int)byte4} / socketIndex = {socketIndex} / {startTime.ToString("HH:mm:ss.fff")}");
                        //Xb.Util.Out($"TryConnect-End: {(int)byte1}.{(int)byte2}.{(int)byte3}.{(int)byte4} / socketIndex = {socketIndex} / {(connected ? "Success" : "Fail") }");
                    });
                });
            }
        }
    }
}
