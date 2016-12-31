using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xb.File
{
    public partial class FileTree
    {
        public class FileNode : Xb.File.Tree.NodeBase
        {
            /// <summary>
            /// Constructor
            /// コンストラクタ
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="path"></param>
            public FileNode(Xb.File.Tree.ITree tree
                          , string path)
                : base(tree, path)
            {
                if (System.IO.File.Exists(path))
                {
                    this.Type = NodeType.File;
                    this.Extension = System.IO.Path.GetExtension(path).TrimStart('.');
                    var info = new System.IO.FileInfo(path);
                    this.UpdateDate = info.LastWriteTime;
                    this.Length = info.Length;
                }
                else if (System.IO.Directory.Exists(path))
                {
                    this.Type = NodeType.Directory;
                    this.Extension = "";
                    this.UpdateDate = (new System.IO.DirectoryInfo(path)).LastWriteTime;
                    this.Length = 0;
                }
                else
                {
                    Xb.Util.Out($"Xb.File.FileTree.FileNode.Constructor: path not found [{path}]");
                    throw new ArgumentException($"Xb.File.FileTree.FileNode.Constructor: path not found [{path}]");
                }
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
                var children = new List<string>();
                try
                {
                    children.AddRange(System.IO.Directory.GetFiles(this.FullPath)
                                                  　     .Select(Xb.File.Tree.TreeBase.FormatPath)
                                                         .ToArray());
                    children.AddRange(System.IO.Directory.GetDirectories(this.FullPath)
                                                         .Select(Xb.File.Tree.TreeBase.FormatPath)
                                                         .ToArray());
                }
                catch (IOException)
                {
                    Xb.Util.Out($"Xb.File.FileTree.FileNode.Scan: Scan failure, may be not permitted [{this.FullPath}]");
                    return;
                }

                //4.If not exists child-node on real systems, dispose child-node 
                //  配下ノードとして存在するものの実システム上には無いものを、破棄する。
                var removeTargets = this.ChildPaths.Where(path => !children.Contains(path))
                                                   .ToArray();
                foreach (var removeTarget in removeTargets)
                    this.Tree[removeTarget]?.Dispose();

                //5.Create new-child node, and passing this.`AddChild` method
                //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
                foreach (var path in children.Where(path => !this.ChildPaths.Contains(path)))
                    this.AddChild(new Xb.File.FileTree.FileNode(this.Tree, path));
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
                    throw new InvalidOperationException("Xb.File.FileTree.FileNode.CreateChild: not directory");

                var childFullPath = System.IO.Path.Combine(this.FullPath, name);
                if (System.IO.File.Exists(childFullPath)
                    || System.IO.Directory.Exists(childFullPath))
                {
                    throw new ArgumentException($"Xb.File.FileTree.FileNode.CreateChild: Exists child [{name}]");
                }

                //2.Generate elements corresponding to nodes for real systems
                //  file on file-system, node on zip-archive, or so
                //  実システム上に、渡し値に該当する要素を生成する。
                //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど
                try
                {
                    switch (type)
                    {
                        case NodeType.File:
                            System.IO.File.WriteAllBytes(childFullPath, new byte[] { });
                            break;
                        case NodeType.Directory:
                            System.IO.Directory.CreateDirectory(childFullPath);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), $"Xb.File.FileTree.FileNode.CreateChild: Unknown type [{type}]");
                    }
                }
                catch (Exception ex)
                {
                    Xb.Util.Out(ex);
                    throw ex;
                }

                //3.Create new-child node, and passing this.`AddChild` method
                //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
                var result = new Xb.File.FileTree.FileNode(this.Tree, childFullPath);
                this.AddChild(result);
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
                var exists = (System.IO.File.Exists(this.FullPath)
                              || System.IO.Directory.Exists(this.FullPath));

                if (exists)
                    FileNode.Delete(this.FullPath);

                exists = (System.IO.File.Exists(this.FullPath)
                          || System.IO.Directory.Exists(this.FullPath));
                if(exists)
                    throw new IOException($"Xb.File.FileTree.FileNode.Delete: Delete failure");

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
                    throw new InvalidOperationException("Xb.File.FileTree.FileNode.GetBytes: Not file");

                return System.IO.File.ReadAllBytes(this.FullPath);
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
                    throw new InvalidOperationException("Xb.File.FileTree.FileNode.GetBytes: Not file");

                var memStream = new MemoryStream();
                using (var stream = System.IO.File.OpenRead(this.FullPath))
                {
                    stream.Seek(offset, SeekOrigin.Begin);

                    var buffer = new byte[length];
                    var size = stream.Read(buffer, 0, buffer.Length);
                    if(size > 0)
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
                    throw new InvalidOperationException("Xb.File.FileTree.FileNode.GetReadStream: Not file");

                return System.IO.File.OpenRead(this.FullPath);
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
                    throw new InvalidOperationException("Xb.File.FileTree.FileNode.GetBytes: Not file");

                bytes = bytes ?? new byte[] { };
                System.IO.File.WriteAllBytes(this.FullPath, bytes);
            }


            /// <summary>
            /// 実システムと自身の保持データに矛盾が無いかどうか検証する。
            /// </summary>
            private void ValidateMyself()
            {
                var fileExists = System.IO.File.Exists(this.FullPath);
                var dirExists = System.IO.Directory.Exists(this.FullPath);
                var nodeExists = (this.Type == NodeType.File)
                                    ? fileExists
                                    : dirExists;

                if ((fileExists || dirExists) != nodeExists)
                {
                    //1)ファイルノードのはずが同名のディレクトリを検知した、
                    //2)もしくは、ディレクトリノードのはずが同名のディレクトリを検知した
                    //のとき、Treeオブジェクトでない何かから操作を受けた。
                    //一旦自身を破棄して、再生成する。
                    var parent = this.Parent;
                    var fullPath = this.FullPath;
                    this.Dispose();
                    var newThis = new Xb.File.FileTree.FileNode(parent.Tree, fullPath);
                    ((Xb.File.FileTree.FileNode)parent).AddChild(newThis);
                    newThis.Scan();

                    throw new IOException("Xb.File.FileTree.FileNode.Scan: Node entity deleted");
                }

                else if (!nodeExists)
                {
                    //自身を示すパスが実システム上に存在しない
                    //自身を破棄して終了
                    this.Dispose();
                    throw new IOException("Xb.File.FileTree.FileNode.Scan: Node entity deleted");
                }
            }

            /// <summary>
            /// Remove file, directory recursive
            /// ファイル／ディレクトリを再帰的に削除する
            /// </summary>
            /// <param name="path"></param>
            /// <param name="force"></param>
            /// <remarks>C# recursive option bug, FUCK!</remarks>
            protected static void Delete(string path, bool force = false)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    return;
                }

                if (Directory.Exists(path))
                {
                    //remove files
                    var filePaths = Directory.GetFiles(path);
                    foreach (var filePath in filePaths)
                    {
                        if (force)
                            System.IO.File.SetAttributes(filePath, FileAttributes.Normal);

                        System.IO.File.Delete(filePath);
                    }

                    //remove directory 
                    var directoryPaths = Directory.GetDirectories(path);
                    foreach (var directoryPath in directoryPaths)
                    {
                        FileNode.Delete(directoryPath);
                    }

                    Directory.Delete(path);
                }
            }
        }
    }
}
