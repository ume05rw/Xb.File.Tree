using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public partial class FileTree : Xb.File.Tree.TreeBase
    {
        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        protected FileTree(string rootPath)
        {
            var rootNode = new Xb.File.FileTree.FileNode(this, rootPath);
            this.Init(rootNode);
        }


        /// <summary>
        /// Returns Tree-object with the passing path as the root
        /// 指定パスをルートにした、Treeオブジェクトを返す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public new static Xb.File.FileTree GetTree(string path)
        {
            var result = new Xb.File.FileTree(path);
            result.RootNode.Scan();
            return result;
        }


        /// <summary>
        /// Returns a Tree object that scans all nodes under the passing path (VERY HEAVY!)
        /// 指定パス配下の全ノードをスキャンしたTreeオブジェクトを返す。重い！
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public new static async Task<Xb.File.FileTree> GetTreeRecursiveAsync(string path)
        {
            Xb.File.FileTree tree = null;

            await Task.Run(() =>
            {
                tree = Xb.File.FileTree.GetTree(path);
            });
            await tree.ScanRecursiveAsync();

            return tree;
        }
    }
}
