using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File.Tree
{
    public abstract class NodeBase : Xb.File.Tree.INode
    {
        /// <summary>
        /// Node type(file or directory)
        /// </summary>
        public enum NodeType
        {
            File,
            Directory
        }

        /// <summary>
        /// Child add event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ChildAddEventHandler(object sender, NodeEventArgs e);

        /// <summary>
        /// Child added event
        /// </summary>
        public event ChildAddEventHandler ChildAdded;

        /// <summary>
        /// Delete-myself event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DeleteEventHandler(object sender, NodeEventArgs e);

        /// <summary>
        /// Deleteed-myself event
        /// </summary>
        public event DeleteEventHandler Deleted;

        /// <summary>
        /// Tree(= node manager) object
        /// ノード管理オブジェクト
        /// </summary>
        public virtual Xb.File.Tree.ITree Tree { get; protected set; }

        /// <summary>
        /// Parent-node
        /// 親ノード
        /// </summary>
        public Xb.File.Tree.INode Parent => this.Tree.Exists(this.ParentPath) 
                                                ? this.Tree.GetNode(this.ParentPath)
                                                : null;

        /// <summary>
        /// Child-Nodes array
        /// 子ノード配列
        /// </summary>
        public Xb.File.Tree.INode[] Children => this.Tree.GetNodes(this.ChildPaths);

        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode this[string name]
        {
            get
            {
                var fullPath = System.IO.Path.Combine(this.FullPath, name);
                return this.Tree.GetNode(Xb.File.Tree.TreeBase.FormatPath(fullPath));
            }
        }

        /// <summary>
        /// Node indexer
        /// ノード要素インデクサ
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode this[int index] => this.Tree.GetNode(this.ChildPaths[index]);

        /// <summary>
        /// Parent-Node full-path
        /// 親ノードのフルパス
        /// </summary>
        protected string ParentPath { get; set; }

        /// <summary>
        /// Child-Node array of full-path(key)
        /// 子ノードのフルパス配列
        /// </summary>
        protected List<string> ChildPaths { get; set; }

        /// <summary>
        /// Node-name (not full-path)
        /// ノード名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Extension
        /// 拡張子
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        /// Node type(file or directory)
        /// </summary>
        public NodeType Type { get; protected set; }

        /// <summary>
        /// Full-Path
        /// </summary>
        public string FullPath { get; protected set; }

        /// <summary>
        /// Last update-date
        /// 最終更新日時
        /// </summary>
        public DateTime UpdateDate { get; protected set; }

        /// <summary>
        /// File Size
        /// ファイルサイズ
        /// </summary>
        public long Length { get; protected set; }

        /// <summary>
        /// ルートノードか否か
        /// is root node?
        /// </summary>
        public bool IsRootNode => (this.Tree.RootNode == this);



        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        protected NodeBase()
        {
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="path"></param>
        protected NodeBase(Xb.File.Tree.ITree tree
                         , string path)
        {
            if (tree == null)
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: tree null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: tree null");
            }

            if (string.IsNullOrEmpty(path))
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: path null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: path null");
            }

            this.Name = System.IO.Path.GetFileName(path);
            this.ParentPath = TreeBase.FormatPath(System.IO.Path.GetDirectoryName(path));
            this.FullPath = TreeBase.FormatPath(System.IO.Path.Combine(this.ParentPath, this.Name));
            this.Tree = tree;
            this.ChildPaths = new List<string>();
        }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="path"></param>
        /// <param name="updateDate"></param>
        /// <param name="type"></param>
        protected NodeBase(Xb.File.Tree.ITree tree
                         , string path
                         , DateTime updateDate
                         , NodeType type = NodeType.File
                         , long length = 0
                         , bool allowEmptyPath = false)
        {
            if (tree == null)
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: tree null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: tree null");
            }

            if (!allowEmptyPath && string.IsNullOrEmpty(path))
            {
                Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: path null");
                throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: path null");
            }
            path = path ?? "";

            switch (type)
            {
                case NodeType.File:

                    this.Type = NodeType.File;
                    this.Extension = System.IO.Path.GetExtension(path).TrimStart('.');
                    this.Length = length;

                    break;

                case NodeType.Directory:

                    this.Type = NodeType.Directory;
                    this.Extension = "";
                    this.Length = 0;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type),
                        $"Xb.File.Tree.NodeBase.Constructor: undefined type[{type}]");
            }

            if (string.IsNullOrEmpty(path))
            {
                this.Name = "";
                this.ParentPath = "";
            }
            else
            {
                this.Name = System.IO.Path.GetFileName(path);
                this.ParentPath = TreeBase.FormatPath(System.IO.Path.GetDirectoryName(path));
            }
            
            this.UpdateDate = updateDate;
            this.FullPath = TreeBase.FormatPath(System.IO.Path.Combine(this.ParentPath, this.Name));
            this.Tree = tree;
            this.ChildPaths = new List<string>();
        }


        /// <summary>
        /// Scan, refresh nodes
        /// 子ノードを走査する
        /// </summary>
        public virtual void Scan()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.Scan: Execute only subclass");

            //implement flow
            //1.If not exists this-node element of real systems, dispose myself
            //  自分自身を示すパスが実システム上に存在しなくなったら、破棄する。
            //2.If node.Type is `File`, exit
            //  自分自身のNodetypeが`File`のとき、終了。ファイルは子が居ないので。
            //3.Get all child-elements path of real systems
            //  直下のファイル／ディレクトリのパス文字列配列を取得する。
            //4.If not exists child-node on real systems, dispose child-node 
            //  配下ノードとして存在するものの実システム上には無いものを、破棄する。
            //5.Create new-child node, and passing this.`AddChild` method
            //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
        }


        /// <summary>
        /// Scan, refresh nodes recursive on async
        /// 子ノードを再帰的に走査する
        /// </summary>
        /// <returns></returns>
        public virtual async Task ScanRecursiveAsync()
        {
            var me = (Xb.File.Tree.INode)this;
            var tree = this.Tree;
            var fullPath = this.FullPath;
            await Task.Run(() =>
            {
                try
                {
                    me.Scan();
                }
                catch (IOException)
                {
                    //自分自身の実体要素が消えてしまった場合、何もしない。
                    if (!tree.Paths.Contains(fullPath))
                        return;

                    //他、実体要素が置換された場合、また子要素走査に失敗した場合は
                    //このまま処理を続ける。
                    me = tree[fullPath];
                }
                catch (Exception)
                {
                    throw;
                }
            });

            //配下のディレクトリをループ
            foreach (var node in me.Children.Where(node => node.Type == NodeType.Directory))
            {
                await node.ScanRecursiveAsync();
            }
        }


        /// <summary>
        /// Get serializable-object of tree structure
        /// 配下のツリー構造をシリアライズ可能なオブジェクトとして取得する
        /// </summary>
        /// <returns></returns>
        public virtual SerializableNode GetSerializable()
        {
            var result = new SerializableNode(this);

            var children = new List<SerializableNode>();

            foreach (var child in this.Children)
                children.Add(child.GetSerializable());

            result.Children = children.ToArray();

            return result;
        }


        /// <summary>
        /// Get all-children recursive
        /// 配下の全ノードを再帰的に取得する
        /// </summary>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode[] GetAllChildrenRecursive()
        {
            var result = new List<Xb.File.Tree.INode>();

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                result.Add(child);

                if (child.Type == NodeType.Directory)
                    result.AddRange(child.GetAllChildrenRecursive());
            }

            return result.ToArray();
        }


        /// <summary>
        /// Get first-node of matched needle
        /// 渡し値文字列が合致した最初の子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode Find(string needle)
        {
            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                    return child;
            }

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                var childResult = child.Find(needle);
                if (childResult != null)
                    return childResult;
            }

            return null;
        }


        /// <summary>
        /// Get all-nodes of matched needle
        /// 渡し値文字列が合致した全ての子ノードを返す
        /// </summary>
        /// <param name="needle"></param>
        /// <returns></returns>
        public virtual Xb.File.Tree.INode[] FindAll(string needle)
        {
            var result = new List<Xb.File.Tree.INode>();

            foreach (var path in this.ChildPaths)
            {
                var child = this.Tree.GetNode(path);
                if (child == null)
                    continue;

                if (child.FullPath.IndexOf(needle, StringComparison.Ordinal) >= 0)
                    result.Add(child);

                result.AddRange(child.FindAll(needle));
            }

            return result.ToArray();
        }


        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.GetBytes: Execute only subclass");
        }


        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <returns></returns>
        public virtual async Task<byte[]> GetBytesAsync()
        {
            return await Task.Run(() => this.GetBytes());
        }


        /// <summary>
        /// Get byte-array of node
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <remarks>file size max 2GB</remarks>
        public virtual byte[] GetBytes(long offset, int length)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.GetBytes: Execute only subclass");
        }


        /// <summary>
        /// Get byte-array of node on async
        /// ノードのデータをバイト配列で取得する
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <remarks>file size max 2GB</remarks>
        public virtual async Task<byte[]> GetBytesAsync(long offset, int length)
        {
            return await Task.Run(() => this.GetBytes(offset, length));
        }


        /// <summary>
        /// Get stream for read-only
        /// 読込専用Streamを取得する
        /// </summary>
        /// <returns></returns>
        public virtual Stream GetReadStream()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.GetReadStream: Execute only subclass");
        }


        /// <summary>
        /// Overwrite data of node
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        public virtual void WriteBytes(byte[] bytes)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.WriteBytes: Execute only subclass");
        }


        /// <summary>
        /// Overwrite data of node on async
        /// バイト配列データをノードに上書きする。
        /// </summary>
        /// <param name="bytes"></param>
        public virtual async Task WriteBytesAsync(byte[] bytes)
        {
            await Task.Run(() => this.WriteBytes(bytes));
        }



        /// <summary>
        /// Create real system element and child tree-node
        /// 実システムに指定要素を追加し、自身に子ノードを追加する
        /// </summary>
        /// <param name="name"></param>
        public virtual INode CreateChild(string name
                                       , NodeType type = NodeType.File)
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.CreateChild: Execute only subclass");

            //implement flow
            //1.Validate you need
            //  バリデーション(必要ならば)
            //2.Generate elements corresponding to nodes for real systems
            //  file on file-system, node on zip-archive, or so
            //  実システム上に、渡し値に該当する要素を生成する。
            //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど
            //3.Create new-child node, and passing this.`AddChild` method
            //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
        }


        /// <summary>
        /// Append new-node to child-list, tree
        /// 新規ノードを、子リストとTreeインスタンスに追加する
        /// </summary>
        /// <param name="node"></param>
        /// <param name="childPath"></param>
        /// <remarks>
        /// ここでは実システムに対する操作は行わない。
        /// あくまでXb.File.Tree構造に対する操作のみを対象とする。
        /// </remarks>
        protected virtual void AddChild(INode node
                                      , string childPath = null)
        {
            if (this.Type == NodeType.File)
                throw new InvalidOperationException("Xb.File.Tree.NodeBase.AddChild: Not directory");

            childPath = childPath 
                            ?? TreeBase.FormatPath(System.IO.Path.Combine(this.FullPath, node.Name));

            if (this.ChildPaths.Contains(childPath))
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.AddChild: Exist node [{childPath}]");

            if (childPath != node.FullPath)
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.AddChild: Invalid relationship");

            node.Deleted += this.OnChildRemoved;
            this.ChildPaths.Add(childPath);
            this.ChildAdded?.Invoke(this, new NodeEventArgs(node));
        }


        /// <summary>
        /// Delete real system elements and myself-node from tree
        /// 実システムから自身に該当する要素を削除し、自分自身を破棄する。
        /// </summary>
        public virtual void Delete()
        {
            throw new NotImplementedException("Xb.File.Tree.NodeBase.Delete: Execute only subclass");

            //implement flow
            //1.Validate you need
            //  バリデーション(必要ならば)
            //2.If exists child-node, execute child-node.`Delete` method.
            //  子ノードが存在するとき、子ノードの`Delete`メソッドを実行する。
            //3.Remove elements corresponding to nodes for real system
            //  file on file-system, node on zip-archive, or so
            //  実システム上に自分自身に該当する要素が存在するとき、削除する。
            //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど
            //3.Call this.`Dispose` method
            //  自身の`Dispose`メソッドを実行する。
        }


        /// <summary>
        /// child-node removed event
        /// 子ノードの削除イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnChildRemoved(object sender, NodeEventArgs e)
        {
            if (e.Node.Parent != this)
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.RemoveChild: Invalid relationship");

            if (!this.ChildPaths.Contains(e.Node.FullPath))
                throw new InvalidOperationException($"Xb.File.Tree.NodeBase.RemoveChild: Child-list broken");

            e.Node.Deleted -= this.OnChildRemoved;
            this.ChildPaths.Remove(e.Node.FullPath);
        }


        #region IDisposable Support
        protected bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //直下の子ノードを破棄
                    foreach (var node in this.Children)
                        node?.Dispose();

                    this.Deleted?.Invoke(this, new NodeEventArgs(this));

                    this.ParentPath = null;

                    for (var i = 0; i < this.ChildPaths.Count; i++)
                        this.ChildPaths[i] = null;

                    this.Tree = null;

                    this.Name = null;
                    this.FullPath = null;
                    this.Extension = null;
                }
                disposedValue = true;
            }
        }
        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion   
        
    }
}
