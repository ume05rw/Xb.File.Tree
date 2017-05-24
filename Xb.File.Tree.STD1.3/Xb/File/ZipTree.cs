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
    public partial class ZipTree : Xb.File.Tree.TreeBase
    {
        protected string ZipFileName { get; set; }
        protected Stream Stream { get; set; }
        protected internal ZipArchive Archive { get; set; }
        protected Encoding Encoding { get; set; }
        protected internal char Delimiter { get; set; }

        /// <summary>
        /// Operation Type
        /// 読み取り専用フラグ
        /// </summary>
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        /// <param name="zipFileName"></param>
        protected ZipTree(string zipFileName
                        , bool readOnly = true
                        , Encoding encoding = null)
        {
            this.Encoding = encoding ?? Encoding.UTF8;
            this.ReadOnly = readOnly;

            if (this.ReadOnly
                && !System.IO.File.Exists(zipFileName))
            {
                throw new FileNotFoundException($"Xb.File.ZipTree.Constructor: zip-file not found [{zipFileName}]");
            }

            //not exist zip file, create
            if (!System.IO.File.Exists(zipFileName))
            {
                using (var stream = new FileStream(zipFileName, FileMode.CreateNew, FileAccess.Write))
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        //create only
                    }
                }
            }

            if (this.ReadOnly)
            {
                this.Stream = new FileStream(zipFileName
                    , FileMode.Open
                    , FileAccess.Read);
                this.Archive = new ZipArchive(this.Stream
                    , ZipArchiveMode.Read
                    , false
                    , this.Encoding);
            }
            else
            {
                this.Stream = new FileStream(zipFileName
                    , FileMode.Open
                    , FileAccess.ReadWrite);
                this.Archive = new ZipArchive(this.Stream
                    , ZipArchiveMode.Update
                    , false
                    , this.Encoding);
            }

            this.BuildTree();
        }

        
        /// <summary>
        /// Constructor - for readonly stream
        /// コンストラクタ
        /// </summary>
        /// <param name="readableStream"></param>
        /// <param name="encoding"></param>
        protected ZipTree(Stream readableStream
                        , Encoding encoding = null)
        {
            this.Encoding = encoding ?? Encoding.UTF8;
            this.ReadOnly = true;
            this.Stream = readableStream;

            try
            {
                //引数が少ない方が、平均的に2,3秒早かった。
                if (encoding == null)
                    this.Archive = new ZipArchive(readableStream);
                else
                    this.Archive = new ZipArchive(readableStream
                                                , ZipArchiveMode.Read
                                                , false
                                                , this.Encoding);
            }
            catch (Exception ex)
            {
                //ダメだった場合は一旦全データを吸い出す。
                //一旦byte配列に吸い出すので、しぬほど遅い
                //これでNGなら例外
                try
                {
                    var bytes = Xb.Byte.GetBytes(readableStream);
                    this.Stream = new MemoryStream(bytes);
                    this.Archive = new ZipArchive(readableStream
                                                , ZipArchiveMode.Read
                                                , false
                                                , this.Encoding);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            this.BuildTree();
        }


        /// <summary>
        /// ZipEntryをツリー上に構成する
        /// </summary>
        protected void BuildTree()
        {
            //アーカイブルートは複数存在可能なので、仮想のルートノードを生成しておく。
            this.Init(new Xb.File.ZipTree.ZipNode(this));

            //デリミタ検出: (unix系で、やたら'\'の多い長いファイル名があったらアウト
            var longNameNode = this.Archive.Entries
                                           .OrderByDescending(e => e.FullName.Length)
                                           .FirstOrDefault();
            //遅いのは、archive オブジェクト生成後に初めて Entries にアクセスするため。
            //全件走査の前に見付かったエントリを順次取得するような機能が無い。残念。
            //↑遅いのは、.NetFW上での体験。遅延評価？
            //　Xamarin.iOSでは、ZipArchiveオブジェクト生成時にくっそ時間喰う

            if (longNameNode == null)
            {
                //↓システム標準デリミタだが、ユーザー環境だとWindows上の
                //↓アーカイバアプリ使用者が多そうなので、バックスラッシュにする。
                //this.Delimiter = System.IO.Path.DirectorySeparatorChar;
                this.Delimiter = '\\';
            }
            else
            {
                this.Delimiter = (longNameNode.FullName.Split('/').Length
                                  <= longNameNode.FullName.Split('\\').Length)
                                        ? '\\'
                                        : '/';
            }

            //ツリー構造を作る
            //子要素が存在するディレクトリ単体のエントリは存在しない。
            //よって、パスの先頭からディレクトリノードを順次生成する必要がある。
            //foreach (var entry in this.Archive.Entries)
            var count = this.Archive.Entries.Count;
            for (var i = 0; i < count; i++)
            {
                var entry = this.Archive.Entries[i];
                var fullPath = TreeBase.FormatPath(entry.FullName);

                var path = "";
                foreach (var elem in fullPath.Split(this.Delimiter))
                {
                    path = $"{path}{(path.Length > 0 ? this.Delimiter.ToString() : string.Empty)}{elem}";

                    if (!this.NodeDictionary.ContainsKey(path))
                    {
                        var updateDate = (path == fullPath)
                                            ? entry.LastWriteTime.DateTime
                                            : DateTime.MinValue;

                        var nodeType = (path == fullPath)
                                            ? entry.FullName.EndsWith(this.Delimiter.ToString())
                                                ? NodeBase.NodeType.Directory
                                                : NodeBase.NodeType.File
                                            : NodeBase.NodeType.Directory;

                        var pathEntry = (path == fullPath)
                                            ? entry
                                            : null;

                        var length = (path == fullPath)
                                            ? entry.Length
                                            : 0;

                        //parentPathが空文字であればRootNodeになる
                        //何かディレクトリ名があれば、前段階で既に生成済みのはず。
                        //parentNodeが取得出来ないことは、有り得ない想定。
                        var parentPath = TreeBase.FormatPath(Path.GetDirectoryName(path));
                        ZipTree.ZipNode parentNode;
                        try
                        {
                            parentNode = (ZipTree.ZipNode)this.NodeDictionary[parentPath];
                        }
                        catch (Exception ex)
                        {
                            Xb.Util.Out(ex);
                            throw ex;
                        }

                        var node = new Xb.File.ZipTree.ZipNode(this
                                                             , path
                                                             , updateDate
                                                             , nodeType
                                                             , length
                                                             , pathEntry);
                        parentNode.AddChild(node);
                    }
                }
            }
        }

        
        /// <summary>
        /// Returns ITree-object of ZipArchive with the passing zip-file as the root
        /// zipファイルをルートにした、Treeオブジェクトを返す
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="readOnly"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<Xb.File.ZipTree> GetTreeAsync(string zipFileName
                                                             , bool readOnly = true
                                                             , Encoding encoding = null)
        {
            return await Task.Run(() => 
            {
                return new Xb.File.ZipTree(zipFileName, readOnly, encoding);
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Returns *READONLY* ITree-object of ZipArchive with the passing zip-file as the root
        /// zipファイルをルートにした、読み取り専用Treeオブジェクトを返す
        /// </summary>
        /// <param name="readableStream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<Xb.File.ZipTree> GetTreeAsync(Stream readableStream
                                                             , Encoding encoding = null)
        {
            return await Task.Run(() => 
            {
                return new Xb.File.ZipTree(readableStream, encoding);
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Disposing Stream, ZipArchive
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    base.Dispose(disposing);

                    this.Archive?.Dispose();
                    this.Archive = null;
                    this.Stream?.Dispose();
                    this.Stream = null;
                    this.Encoding = null;
                }
                disposedValue = true;
            }
        }
    }
}
