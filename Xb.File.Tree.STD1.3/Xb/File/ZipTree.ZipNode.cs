using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xb.File.Tree;

namespace Xb.File
{
    public partial class ZipTree
    {
        public class ZipNode : Xb.File.Tree.NodeBase
        {
            protected ZipArchiveEntry Entry { get; set; }


            /// <summary>
            /// Constructor for root-node only
            /// コンストラクタ
            /// </summary>
            /// <param name="tree"></param>
            /// <remarks>
            /// Zipアーカイブ用Tree.RootNodeには、該当するZipArchiveEntryオブジェクトが存在しない。
            /// </remarks>
            internal ZipNode(Xb.File.Tree.ITree tree)
                : base(tree
                     , ""
                     , DateTime.MinValue
                     , NodeType.Directory
                     , 0
                     , true)
            {
                if(tree.RootNode != null)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.Constructor: exist root node");

                this.Entry = null;
            }


            /// <summary>
            /// Constructor
            /// コンストラクタ
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="path"></param>
            /// <param name="updateDate"></param>
            /// <param name="type"></param>
            /// <param name="entry"></param>
            internal ZipNode(Xb.File.Tree.ITree tree
                           , string path
                           , DateTime updateDate
                           , NodeType type
                           , long length
                           , ZipArchiveEntry entry)
                : base(tree
                     , path
                     , updateDate
                     , type
                     , length)
            {
                this.Entry = entry;
            }


            protected new void Scan()
            {
                //Zipアーカイブを複数が同時に操作することは無いこととする。
                //ノードのCreate, Deleteは常にXb.File.ZipTree経由で操作すると想定、
                //Zipアーカーブ構造をScanする必要は無い。
            }

            protected new async Task ScanRecursiveAsync()
            {
                //Zipアーカイブを複数が同時に操作することは無いこととする。
                //ノードのCreate, Deleteは常にXb.File.ZipTree経由で操作すると想定、
                //Zipアーカーブ構造をScanする必要は無い。
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

                if (((ZipTree)this.Tree).ReadOnly)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.CreateChild: Archive read only");

                if (this.Type != NodeType.Directory)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.CreateChild: not directory");

                var path = Xb.File.Tree.TreeBase.FormatPath(Path.Combine(this.FullPath, name));

                if (this.ChildPaths.Contains(path))
                    throw new ArgumentException($"Xb.File.ZipTree.ZipNode.CreateChild: Exists child [{name}]");


                //2.Generate elements corresponding to nodes for real systems
                //  file on file-system, node on zip-archive, or so
                //  実システム上に、渡し値に該当する要素を生成する。
                //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど

                var entryName = $"{path}{(type == NodeType.Directory ? ((ZipTree)this.Tree).Delimiter.ToString() : "")}";
                var entry = ((ZipTree)this.Tree).Archive.CreateEntry(entryName);


                //3.Create new-child node, and passing this.`AddChild` method
                //  INodeインスタンスを生成し、`AddChild`メソッドに渡す。
                
                var result = new Xb.File.ZipTree.ZipNode(this.Tree
                                                       , path
                                                       , entry.LastWriteTime.DateTime
                                                       , type
                                                       , entry.Length
                                                       , entry);
                this.AddChild(result);
                return result;
            }


            /// <summary>
            /// Add child node
            /// 子ノードを追加する。
            /// </summary>
            /// <param name="node"></param>
            internal void AddChild(INode node)
            {
                base.AddChild(node);
            }


            /// <summary>
            /// Delete real system elements and myself-node from tree
            /// 実システムから自身に該当する要素を削除し、自分自身を破棄する。
            /// </summary>
            public override void Delete()
            {
                //implement flow
                //1.Validate you need
                //  バリデーション(必要ならば)

                if (((ZipTree)this.Tree).ReadOnly)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.Delete: Archive read only");

                //2.If exists child-node, execute child-node.`Delete` method.
                //  子ノードが存在するとき、子ノードの`Delete`メソッドを実行する。
                foreach (var child in this.Children)
                    child.Delete();

                //3.Remove elements corresponding to nodes for real system
                //  file on file-system, node on zip-archive, or so
                //  実システム上に自分自身に該当する要素が存在するとき、削除する。
                //  ファイルシステム上のファイル、Zipアーカイブ上のノードなど
                this.Entry?.Delete();

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
                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetBytes: Not file");

                if (this.Entry == null)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetBytes: Entry not found");

                var memStream = new MemoryStream();
                using (var stream = this.Entry.Open())
                {
                    var buffer = new byte[Xb.Byte.BufferSize];
                    int size;
                    while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
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
                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetReadStream: Not file");

                return this.Entry.Open();
            }


            /// <summary>
            /// Get byte-array of node
            /// ノードのデータをバイト配列で取得する
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            /// <remarks>file size max 2GB</remarks>
            protected new byte[] GetBytes(long offset, int length)
            {
                throw new NotSupportedException();
                //stream.Seek(offset, SeekOrigin.Begin); で例外。「サポート外の操作」と。

                //if (this.Type != NodeType.File)
                //    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetBytes: Not file");

                //if (this.Entry == null)
                //    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.GetBytes: Entry not found");

                //var memStream = new MemoryStream();
                //using (var stream = this.Entry.Open())
                //{
                //    stream.Seek(offset, SeekOrigin.Begin);

                //    var buffer = new byte[length];
                //    var size = stream.Read(buffer, 0, buffer.Length);
                //    if (size > 0)
                //        memStream.Write(buffer, 0, size);
                //}

                //return memStream.ToArray();
            }


            /// <summary>
            /// Overwrite data of node
            /// バイト配列データをノードに上書きする。
            /// </summary>
            /// <param name="bytes"></param>
            public override void WriteBytes(byte[] bytes)
            {
                if (((ZipTree)this.Tree).ReadOnly)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.WriteBytes: Archive read only");

                if (this.Type != NodeType.File)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.WriteBytes: Not file");

                if (this.Entry == null)
                    throw new InvalidOperationException("Xb.File.ZipTree.ZipNode.WriteBytes: Entry not found");

                bytes = bytes ?? new byte[] { };
                var fullName = this.Entry.FullName;
                var archive = ((ZipTree)this.Tree).Archive;
                if (archive.Entries.Contains(this.Entry))
                    this.Entry.Delete();

                this.Entry = archive.CreateEntry(fullName);

                using (var stream = this.Entry.Open())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            /// <summary>
            /// Disposing Entry
            /// </summary>
            /// <param name="disposing"></param>
            protected override void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        base.Dispose(disposing);
                        this.Entry = null;
                    }
                    disposedValue = true;
                }
            }
        }
    }
}
