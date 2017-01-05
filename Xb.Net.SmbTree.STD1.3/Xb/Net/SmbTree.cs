using System;
using System.Collections.Generic;
using System.Linq;
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
            var prefix = new StringBuilder();
            var existPrefix = false;

            if (!string.IsNullOrEmpty(this.Domain))
            {
                existPrefix = true;
                prefix.Append(this.Domain);
                if (!string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
                    prefix.Append(";");
            }

            if (!string.IsNullOrEmpty(this.UserName))
            {
                existPrefix = true;
                prefix.Append(this.UserName);
                if (!string.IsNullOrEmpty(this.Password))
                    prefix.Append(":");
            }

            if (!string.IsNullOrEmpty(this.Password))
            {
                existPrefix = true;
                prefix.Append(this.Password);
            }

            if (existPrefix)
                prefix.Append("@");

            return $"smb://{(existPrefix ? prefix.ToString() : "")}{this.ServerName}/{path}";
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
                    var shares = (new SmbFile($"smb://{serverAddress}")).ListFiles()
                        .Select(node => node.GetName())
                        .Select(name => name.EndsWith("/") ? name.Substring(0, name.Length - 1) : name)
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
                                .Select(name => name.EndsWith("/") ? name.Substring(0, name.Length - 1) : name)
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
            return await Task.Run(() =>
            {
                //自身のIPv4アドレスを取得
                var addresses = Task.Run(() => System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()))
                                    .GetAwaiter()
                                    .GetResult()
                                    .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
                                    .ToArray();

                if (addresses.Length <= 0)
                    return new string[] {};

                var tryAddress = new Dictionary<IPEndPoint, IPAddress>();
                var resultCount = 0;
                var existAddrs = new List<IPAddress>();

                foreach (var address in addresses)
                {
                    var addrBytes = address.GetAddressBytes();

                    //自身のv4アドレスは取得可能だが、マスクが取得できないため
                    //24bitマスクとしてスキャンする。
                    for (var i = 1; i < 255; i++)
                    {
                        addrBytes[3] = (byte) i;

                        var addr = new IPAddress(addrBytes);
                        var endPoint = new System.Net.IPEndPoint(addr, 445);
                        tryAddress.Add(endPoint, addr);

                        var connected = false;

                        var ev = new SocketAsyncEventArgs
                        {
                            RemoteEndPoint = new System.Net.IPEndPoint(addr, 445)
                        };
                        ev.Completed += (sender, e) =>
                        {
                            resultCount++;

                            connected = true;

                            if (e.SocketError == SocketError.Success)
                                existAddrs.Add(tryAddress[(IPEndPoint)e.RemoteEndPoint]);
                        };

                        var soc = new Socket(addr.AddressFamily
                                           , SocketType.Stream
                                           , ProtocolType.Tcp);
                        soc.ConnectAsync(ev);

                        //非同期多重実行したかったが、iOSで`TooManyOpenFiles`エラー発生につき
                        //一つ一つアドレスを検証することにする。
                        //50ミリ秒以内に応答がないとき、次へ。
                        var limitTime = DateTime.Now.AddMilliseconds(50);
                        while (!connected)
                        {
                            if (limitTime < DateTime.Now)
                                break;

                            Thread.Sleep(50);
                        }

                        soc.Dispose();
                    }
                }

                var result = new List<string>();
                foreach (var addr in existAddrs)
                {
                    var server = string.Join(".", addr.GetAddressBytes().Select(b => ((int)b).ToString()));
                    var smb = new SmbFile($"smb://{server}");
                    if (smb.Exists())
                        result.Add(server);
                }

                return result.ToArray();
            });
        }
    }
}
