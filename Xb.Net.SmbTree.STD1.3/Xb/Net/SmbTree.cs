using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if(string.IsNullOrEmpty(this.ServerName))
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
        /// Returns Tree-object with the passing path as the root
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
        /// Returns a Tree object that scans all nodes under the passing path (VERY HEAVY!)
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
    }
}
