using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public abstract partial class TreeBase : Xb.File.Tree.ITree
    {
        /// <summary>
        /// Nodes array
        /// 配下のノード配列
        /// </summary>
        protected Dictionary<string, Xb.File.Tree.INode> NodeDictionary;


        /// <summary>
        /// Root node on tree
        /// Treeのルートノード
        /// </summary>
        public Xb.File.Tree.INode RootNode { get; protected set; }


        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Xb.File.Tree.INode this[string name] => this.RootNode[name];


        /// <summary>
        /// Node-path array of all nodes
        /// ノードパス配列
        /// </summary>
        public string[] Paths =>  this.NodeDictionary.Select(pair => pair.Key).ToArray(); 


        /// <summary>
        /// Node array of all nodes
        /// ノード配列
        /// </summary>
        public Xb.File.Tree.INode[] Nodes => this.NodeDictionary.Select(pair => pair.Value).ToArray();


        /// <summary>
        /// Initialize
        /// 初期化
        /// </summary>
        /// <param name="rootNode"></param>
        protected void Init(Xb.File.Tree.INode rootNode)
        {
            this.RootNode = rootNode;

            this.NodeDictionary = new Dictionary<string, Xb.File.Tree.INode>
            {
                {this.RootNode.FullPath, this.RootNode}
            };

            this.RootNode.ChildAdded += this.OnNodeChildAdded;
            this.RootNode.Deleted += this.OnNodeDeleted;
        }


        /// <summary>
        /// Event when child-node is added to node-tree
        /// 配下のノードに子ノードが追加されたときのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNodeChildAdded(object sender, NodeEventArgs e)
        {
            this.NodeDictionary.Add(e.Node.FullPath, e.Node);

            e.Node.ChildAdded += this.OnNodeChildAdded;
            e.Node.Deleted += this.OnNodeDeleted;
        }


        /// <summary>
        /// Event when node is deleted on node-tree
        /// 配下のノードが削除されたときのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            e.Node.ChildAdded -= this.OnNodeChildAdded;
            e.Node.Deleted -= this.OnNodeDeleted;

            this.NodeDictionary[e.Node.FullPath] = null;
            this.NodeDictionary.Remove(e.Node.FullPath);
        }


        /// <summary>
        /// Validate own-node path
        /// 配下ノードのパスか否か
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Exists(string path)
        {
            return this.NodeDictionary.ContainsKey(path);
        }

        /// <summary>
        /// Get matched one Node-object by fullpath
        /// パスが合致したNodeオブジェクトを返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Xb.File.Tree.INode GetNode(string path)
        {
            return this.NodeDictionary[path];
        }

        /// <summary>
        /// Get matched Node-objects by fullpath
        /// パスが合致したNodeオブジェクトの配列を返す
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public Xb.File.Tree.INode[] GetNodes(ICollection<string> paths)
        {
            var result = this.NodeDictionary.Where(pair => paths.Contains(pair.Key))
                                            .Select(pair => pair.Value)
                                            .ToArray();

            //渡し値パス配列の中に、存在しないパスがあるとき、それを指摘して例外
            if (result.Length != paths.Count)
            {
                var exists = result.Select(n => n.FullPath);
                var errorPaths = new List<string>();
                foreach (var path in paths)
                    if (!exists.Contains(path))
                        errorPaths.Add(path);

                throw new InvalidOperationException($"Xb.File.Tree.TreeBase.GetNodes: Not found node [{ string.Join(", ", errorPaths)}]");
            }

            return result;
        }


        /// <summary>
        /// Get first-node of matched needle
        /// 渡し値文字列が合致した最初の子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public Xb.File.Tree.INode Find(string needle)
        {
            return this.NodeDictionary
                       .Where(pair => pair.Key.IndexOf(needle
                                                     , StringComparison.Ordinal) >= 0)
                       .Select(pair => pair.Value)
                       .FirstOrDefault();
        }


        /// <summary>
        /// Get all-nodes of matched needle
        /// 渡し値文字列が合致した全ての子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public Xb.File.Tree.INode[] FindAll(string needle)
        {
            return this.NodeDictionary
                       .Where(pair => pair.Key.IndexOf(needle
                                                     , StringComparison.Ordinal) >= 0)
                       .Select(pair => pair.Value)
                       .ToArray();
        }



        /// <summary>
        /// Tree-Structure Re-Scan recursive(VERY HEAVY!)
        /// ツリー構造をルートノードから再帰的に取得する
        /// </summary>
        /// <returns></returns>
        public async Task ScanRecursiveAsync()
        {
            await this.RootNode.ScanRecursiveAsync();
        }


        /// <summary>
        /// Returns Tree-object with the passing path as the root
        /// 指定パスをルートにした、Treeオブジェクトを返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static Xb.File.Tree.ITree GetTree(string path)
        {
            throw new NotImplementedException("Xb.File.Tree.TreeBase.GetTree: Execute only subclass");
        }


        /// <summary>
        /// Returns a Tree object that scans all nodes under the passing path (VERY HEAVY!)
        /// 指定パス配下の全ノードをスキャンしたTreeオブジェクトを返す。重い！
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static async Task<Xb.File.Tree.ITree> GetTreeRecursiveAsync(string path)
        {
            throw new NotImplementedException("Xb.File.Tree.TreeBase.GetTreeRecursiveAsync: Execute only subclass");
        }



        /// <summary>
        /// Delimiter char
        /// パスの区切り文字
        /// </summary>
        protected static readonly char[] DelimiterChars = new char[] { '\\', '/' };

        /// <summary>
        /// Format path-string
        /// パス文字列を整形する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FormatPath(string path)
        {
            return (path == null)
                        ? ""
                        : path.TrimEnd(Xb.File.Tree.TreeBase.DelimiterChars);
        }


        #region IDisposable Support
        protected bool disposedValue = false; // 重複する呼び出しを検出するには


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //dispose all node recursive
                    this.RootNode.Dispose();

                    if(this.NodeDictionary.Count > 0)
                        throw new Exception("想定外やで！！！！！！！！！！！！！");

                    this.RootNode = null;
                    this.NodeDictionary = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
