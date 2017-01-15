using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCifs.Smb;
using Xb.File.Tree;

namespace Xb.Net
{
    public partial class SmbTree
    {
        public class SmbNode : Xb.File.Tree.NodeBase
        {
            protected SmbFile SmbFile { get; set; }


            /// <summary>
            /// Node indexer
            /// ノード要素インデクサ
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public override Xb.File.Tree.INode this[string name]
            {
                get
                {
                    var fullPath = this.CombinePath(this.FullPath, name);
                    return this.Tree.GetNode(Xb.File.Tree.TreeBase.FormatPath(fullPath));
                }
            }

            /// <summary>
            /// Constructor
            /// コンストラクタ
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="smbFile"></param>
            public SmbNode(Xb.File.Tree.ITree tree
                         , SmbFile smbFile)
            {
                var path = ((SmbTree) tree).GetNodePath(smbFile.GetPath());
                var smbTree = (SmbTree) tree;
                this.SmbFile = smbFile;

                if (tree == null)
                {
                    Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: tree null");
                    throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: tree null");
                }

                //if (string.IsNullOrEmpty(path))
                //{
                //    Xb.Util.Out($"Xb.File.Tree.NodeBase.Constructor: path null");
                //    throw new ArgumentException($"Xb.File.TreeBase.Node.Constructor: path null");
                //}

                this.SetPath(path);
                this.Tree = tree;
                this.ChildPaths = new List<string>();


                if (!this.SmbFile.Exists())
                    Xb.Util.Out($"Xb.Net.SmbTree.SmbNode.Constructor: path not found [{path}]");

                if (this.SmbFile.IsFile())
                {
                    this.Type = NodeType.File;
                    this.Extension = System.IO.Path.GetExtension(path).TrimStart('.');
                    this.UpdateDate = Xb.Date.GetDate(this.SmbFile.LastModified(), true);
                    this.Length = this.SmbFile.Length();
                }
                else if (this.SmbFile.IsDirectory())
                {
                    //ディレクトリのときでも、存在チェックは末尾'/'無しでも可能
                    //ただし子アイテム取得などの操作は末尾'/'が無いとエラーになるため、
                    //ここで補完する。
                    if (!path.EndsWith("/"))
                    {
                        this.SmbFile = new SmbFile($"{smbTree.GetUriString(path)}/");
                        this.SmbFile.Exists();
                    }
                    this.Type = NodeType.Directory;
                    this.Extension = "";
                    this.UpdateDate = Xb.Date.GetDate(this.SmbFile.LastModified(), true);
                    this.Length = 0;
                }
                else
                {
                    Xb.Util.Out("Xb.Net.SmbTree.SmbNode.Constructor: unknown node type");
                    throw new ArgumentException("Xb.Net.SmbTree.SmbNode.Constructor: unknown node type");
                }
            }


            protected virtual void SetPath(string path)
            {
                //SharpCifs上では、区切り文字は常に'/'
                var elems = path.Split('/');

                var idx = -1;
                for (var i = elems.Length - 1; 0 <= i; i--)
                {
                    if (!string.IsNullOrEmpty(elems[i]))
                    {
                        idx = i;
                        break;
                    }
                }

                this.Name = (idx >= 0)
                    ? elems[idx]
                    : "";
                this.ParentPath = (idx > 0)
                    ? string.Join("/", elems.Take(idx))
                    : "";

                this.FullPath = this.CombinePath(this.ParentPath, this.Name);
            }

            protected virtual string CombinePath(params string[] paths)
            {
                if (paths == null || paths.Length <= 0)
                    return "";

                var pathElems = new List<string>();
                foreach (var path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                        pathElems.Add(path);
                }
                return string.Join("/", pathElems);
            }


            /// <summary>
            /// Scan & refresh nodes
            /// 子ノードを走査する
            /// </summary>
            public override void Scan()
            {
                this.ValidateMyself();

                //2.If node.Type is `File`, exit
                //  自分自身のNodetypeが`File`のとき、終了。ファイルは子が居ないので。
                if (this.Type == NodeType.File)
                    return;
                
                //3.Get all child-elements path of real systems
                //  直下のファイル／ディレクトリのパス文字列配列を取得する。
                var tree = (SmbTree)this.Tree;
                Dictionary<string, SmbFile> childrenDic;

                try
                {
                    childrenDic = this.SmbFile
                                      .ListFiles()
                                      .ToDictionary(n => tree.GetNodePath(n.GetPath())
                                                  , n => n);
                }
                catch (IOException)
                {
                    Xb.Util.Out($"Xb.Net.SmbTree.FileNode.Scan: Scan failure, may be not permitted [{this.FullPath}]");
                    return;
                }

                //4.If not exists child-node on real systems, dispose child-node 
                //  配下ノードとして存在するものの実システム上には無いものを、破棄する。
                var removeTargets = this.ChildPaths.Where(path => !childrenDic.Keys.Contains(path))
                                                   .ToArray();
                foreach (var removeTarget in removeTargets)
                    this.Tree.GetNode(removeTarget).Dispose();

                //5.Create new-child node, and passing this.`AddChild` method
                //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
                //path => !this.ChildPaths.Contains(path)))
                foreach (var smbfile in childrenDic.Where(pair => !this.ChildPaths.Contains(pair.Key))
                                                   .Select(pair => pair.Value))
                {
                    var newNode = new Xb.Net.SmbTree.SmbNode(this.Tree, smbfile);
                    this.AddChild(newNode, TreeBase.FormatPath(this.CombinePath(this.FullPath, newNode.Name)));
                }
            }


            private void ValidateMyself()
            {
                var tree = (SmbTree)this.Tree;
                
                var newSmbFile = new SmbFile(tree.GetUriString(this.FullPath));

                var exists = newSmbFile.Exists();
                var fileExists = (exists && newSmbFile.IsFile());
                var dirExists = (exists && newSmbFile.IsDirectory());
                var nodeExists = (this.Type == NodeType.File)
                                    ? fileExists
                                    : dirExists;

                if ((fileExists || dirExists) != nodeExists)
                {
                    //ルートノードのファイル/ディレクトリが差し替えされたとき
                    if(this.IsRootNode)
                        throw new FileNotFoundException("Xb.Net.SmbTree.FileNode.Scan: Lost root node");


                    //1)ファイルノードのはずが同名のディレクトリを検知した、
                    //2)もしくは、ディレクトリノードのはずが同名のディレクトリを検知した
                    //のとき、Treeオブジェクトでない何かから操作を受けた。
                    //一旦自身を破棄して、再生成する。

                    var fullPath = $"{this.FullPath}{(dirExists && !this.FullPath.EndsWith("/") ? "/" : "")}";
                    var parent = this.Parent;

                    this.Dispose();

                    
                    newSmbFile = new SmbFile(tree.GetUriString(fullPath));
                    var newThis = new Xb.Net.SmbTree.SmbNode(parent.Tree, newSmbFile);
                    var parentNode = (Xb.Net.SmbTree.SmbNode) parent;
                    parentNode.AddChild(newThis, 
                                        TreeBase.FormatPath(this.CombinePath(parentNode.FullPath, newThis.Name)));
                    newThis.Scan();

                    throw new IOException("Xb.Net.SmbTree.FileNode.Scan: Node entity type changed");
                }

                else if (!nodeExists)
                {
                    //自身を示すパスが実システム上に存在しない
                    //自身を破棄して終了
                    this.Dispose();
                    throw new IOException("Xb.Net.SmbTree.FileNode.Scan: Node entity deleted");
                }
            }


            /// <summary>
            /// Create real system element and child tree-node
            /// 実システムに指定要素を追加し、自身に子ノードを追加する
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            public override Xb.File.Tree.INode CreateChild(string name
                                                         , NodeType type = NodeType.File)
            {
                //1.Validate you need
                //  バリデーション(必要ならば)
                if (this.Type != NodeType.Directory)
                    throw new InvalidOperationException("Xb.Net.ZipTree.ZipNode.CreateChild: not directory");

                //2.Generate elements corresponding to nodes for real systems
                //  file on file-system, node on zip-archive, or so
                //  実システム上に、渡し値に該当する要素を生成する。
                //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど

                var childFullPath = this.CombinePath(this.FullPath, name);

                if (type == NodeType.Directory && !childFullPath.EndsWith("/"))
                    childFullPath = $"{childFullPath}/";

                var tree = (SmbTree)this.Tree;
                var newSmbFile = new SmbFile(tree.GetUriString(childFullPath));

                if(newSmbFile.Exists())
                    throw new ArgumentException($"Xb.Net.ZipTree.ZipNode.CreateChild: Exists child [{name}]");

                try
                {
                    switch (type)
                    {
                        case NodeType.File:
                            newSmbFile.CreateNewFile();

                            break;
                        case NodeType.Directory:
                            newSmbFile.Mkdir();

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), $"Xb.Net.ZipTree.ZipNode.CreateChild: Unknown type [{type}]");
                    }
                }
                catch (Exception ex)
                {
                    Xb.Util.Out(ex);
                    throw ex;
                }

                //3.Create new-child node, and passing this.`AddChild` method
                //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
                var result = new Xb.Net.SmbTree.SmbNode(this.Tree, newSmbFile);
                this.AddChild(result, TreeBase.FormatPath(childFullPath));
                return result;
            }


            /// <summary>
            /// Delete real system elements and myself-node from tree
            /// 実システムから自身に該当する要素を削除し、自分自身を破棄する。
            /// </summary>
            public override void Delete()
            {
                //1.Validate you need
                //  バリデーション(必要ならば)

                //2.If exists child-node, execute child-node.`Delete` method.
                //  子ノードが存在するとき、子ノードの`Delete`メソッドを実行する。
                foreach (var child in this.Children)
                    child.Delete();

                //3.Remove elements corresponding to nodes for real system
                //  file on file-system, node on zip-archive, or so
                //  実システム上に自分自身に該当する要素が存在するとき、削除する。
                //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど
                if (this.SmbFile.Exists())
                    this.SmbFile.Delete();

                if (this.SmbFile.Exists())
                    throw new IOException($"Xb.Net.ZipTree.ZipNode.Delete: Delete failure");

                //3.Call this.`Dispose` method
                //  自身の`Dispose`メソッドを実行する。
                this.Dispose();
            }


            /// <summary>
            /// Get byte-array of node
            /// ノードのデータをバイト配列で取得する
            /// </summary>
            /// <returns></returns>
            public override byte[] GetBytes()
            {
                this.ValidateMyself();

                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.Net.ZipTree.ZipNodeGetBytes: Not file");

                var stream = this.SmbFile.GetInputStream();
                var result = Xb.Byte.GetBytes(stream);
                stream.Dispose();
                return result;
            }


            /// <summary>
            /// Get byte-array of node
            /// ノードのデータをバイト配列で取得する
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            /// <remarks>file size max 2GB</remarks>
            public override byte[] GetBytes(long offset, int length)
            {
                this.ValidateMyself();

                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.Net.ZipTree.ZipNodeGetBytes: Not file");

                var memStream = new MemoryStream();
                using (var stream = this.SmbFile.GetInputStream())
                {
                    stream.Skip(offset);

                    var buffer = new byte[length];
                    var size = stream.Read(buffer, 0, buffer.Length);
                    if (size > 0)
                        memStream.Write(buffer, 0, size);
                }

                return memStream.ToArray();
            }


            /// <summary>
            /// Get stream for read-only
            /// 読込専用Streamを取得する
            /// </summary>
            /// <returns></returns>
            public override Stream GetReadStream()
            {
                this.ValidateMyself();

                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetReadStream: Not file");

                return this.SmbFile.GetInputStream();
            }


            /// <summary>
            /// Overwrite data of node
            /// バイト配列データをノードに上書きする。
            /// </summary>
            /// <param name="bytes"></param>
            public override void WriteBytes(byte[] bytes)
            {
                this.ValidateMyself();

                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.Net.ZipTree.ZipNodeGetBytes: Not file");

                using (var stream = this.SmbFile.GetOutputStream())
                {
                    stream.Write(bytes);
                }
            }

            /// <summary>
            /// Dispoing SmbFile
            /// </summary>
            /// <param name="disposing"></param>
            protected override void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        base.Dispose(disposing);
                        this.SmbFile = null;
                    }
                    disposedValue = true;
                }
            }
        }
    }
}
