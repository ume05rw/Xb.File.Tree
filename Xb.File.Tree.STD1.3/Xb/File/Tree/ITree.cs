using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public interface ITree : IDisposable
    {
        /// <summary>
        /// Root node on tree
        /// Treeのルートノード
        /// </summary>
        Xb.File.Tree.INode RootNode { get; }

        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Xb.File.Tree.INode this[string name] { get; }

        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Xb.File.Tree.INode this[int index] { get; }

        /// <summary>
        /// Node-path array of all nodes
        /// ノードパス配列
        /// </summary>
        string[] Paths { get; }

        /// <summary>
        /// Node array of all nodes
        /// ノード配列
        /// </summary>
        Xb.File.Tree.INode[] Nodes { get; }

        /// <summary>
        /// Validate own-node path
        /// 配下ノードのパスか否か
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool Exists(string path);

        /// <summary>
        /// Get matched one Node-object by fullpath
        /// パスが合致したNodeオブジェクトを返す
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        Xb.File.Tree.INode GetNode(string path);

        /// <summary>
        /// Get matched Node-objects by fullpath
        /// パスが合致したNodeオブジェクトの配列を返す
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        Xb.File.Tree.INode[] GetNodes(ICollection<string> paths);

        /// <summary>
        /// Get first-node of matched needle
        /// 渡し値文字列が合致した最初の子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        Xb.File.Tree.INode Find(string needle);

        /// <summary>
        /// Get all-nodes of matched needle
        /// 渡し値文字列が合致した全ての子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        Xb.File.Tree.INode[] FindAll(string needle);

        /// <summary>
        /// Tree-Structure Re-Scan recursive(VERY HEAVY!)
        /// ツリー構造をルートノードから再帰的に取得する
        /// </summary>
        /// <returns></returns>
        Task ScanRecursiveAsync();
    }
}
