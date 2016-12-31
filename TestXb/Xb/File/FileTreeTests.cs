using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xb.File.Tree;

namespace TextXb
{
    [TestClass()]
    public class FileTreeTests : FileBase
    {
        [TestMethod()]
        public void TreeCreateTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //ファイル構造生成
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            Assert.AreNotEqual(null, tree.RootNode);
            Assert.AreEqual(tree.RootNode, tree[baseDir]);
            Assert.AreEqual(8, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains(baseDir));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["baseDir"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["dir1"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["dir2"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["マルチバイトЙ"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["dirNoData"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["file1.txt"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["file3.txt"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["マルチバイトΩ.txt"]));
        }

        [TestMethod()]
        public void NodeScanTest1()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            var dir1 = tree["dir1"];

            Assert.AreEqual(0, dir1.Children.Length);
            Assert.AreEqual(8, tree.Paths.Length);

            dir1.Scan();

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["subdir1"]));
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["file2.txt"]));

            Assert.AreEqual(10, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["subdir1"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["file2.txt"]));
        }

        [TestMethod()]
        public void NodeScanTest2()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            var dir1 = tree["dir1"];
            dir1.Scan();
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, dir1.Type);
            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Nodes.Length);

            //baseDir/dir1フォルダを削除して、dir1ファイルを作る。
            var dir1Path = dir1.FullPath;
            Xb.File.Util.Delete(dir1Path);
            Xb.File.Util.WriteBytes(dir1Path, null);

            //フォルダ削除後、Scanによる同期がまだなので、以前Scanした内容を保持している。
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, dir1.Type);
            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Nodes.Length);

            try
            {
                dir1.Scan();
                Assert.Fail();
            }
            catch (IOException)
            {
                //dir1実体喪失につき再取得
                dir1 = tree["dir1"];
            }
            catch (Exception)
            {
                throw;
            }

            Assert.AreEqual("dir1", dir1.Name);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, dir1.Type);
            Assert.AreEqual(0, dir1.Children.Length);
            Assert.AreEqual("", dir1.Extension);
            Assert.AreEqual(8, tree.Nodes.Length);



            //basDir/file1.txtファイルを削除して、file1.txtディレクトリを作る。
            //その中にフォルダとディレクトリを追加。
            var file1 = tree["file1.txt"];
            file1.Scan();

            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, file1.Type);
            Assert.AreEqual(0, file1.Children.Length);
            Assert.AreEqual(8, tree.Nodes.Length);

            var file1Path = file1.FullPath;
            Xb.File.Util.Delete(file1Path);
            System.IO.Directory.CreateDirectory(file1Path);

            //ファイル削除後、Scanによる同期がまだなので、以前Scanした内容を保持している。
            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, file1.Type);
            Assert.AreEqual(0, file1.Children.Length);
            Assert.AreEqual(8, tree.Nodes.Length);

            try
            {
                file1.Scan();
                Assert.Fail();
            }
            catch (IOException)
            {
                //file1実体喪失につき再取得
                file1 = tree["file1.txt"];
            }
            catch (Exception)
            {
                throw;
            }

            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, file1.Type);
            Assert.AreEqual(0, file1.Children.Length);
            Assert.AreEqual(8, tree.Nodes.Length);

        }


        [TestMethod()]
        public void IndexerTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            //Treeインデクサ経由とNodeインデクサ経由で同一オブジェクトが取れる
            
            Assert.AreEqual(tree.GetNode(structure.Elements["dir1"]), tree.RootNode["dir1"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["dir2"]), tree.RootNode["dir2"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["マルチバイトЙ"]), tree.RootNode["マルチバイトЙ"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["dirNoData"]), tree.RootNode["dirNoData"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["file1.txt"]), tree.RootNode["file1.txt"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["file3.txt"]), tree.RootNode["file3.txt"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["マルチバイトΩ.txt"]), tree.RootNode["マルチバイトΩ.txt"]);

            tree.RootNode["dir1"].Scan();
            Assert.AreEqual(tree.GetNode(structure.Elements["subdir1"]), tree.RootNode["dir1"]["subdir1"]);
            Assert.AreEqual(tree.GetNode(structure.Elements["file2.txt"]), tree.RootNode["dir1"]["file2.txt"]);

        }

        [TestMethod()]
        public void NodeDisposeTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            var dir1 = tree.GetNode(structure.Elements["dir1"]);
            dir1.Scan();

            Assert.AreNotEqual(null, dir1["subdir1"]);
            Assert.AreNotEqual(null, dir1["file2.txt"]);

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Paths.Length);


            //ファイルノードを一つだけ破棄(※削除ではない)
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["file2.txt"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["file2.txt"]));

            dir1["file2.txt"].Dispose();

            Assert.AreEqual(1, dir1.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);

            Assert.IsFalse(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["file2.txt"]));
            Assert.IsFalse(tree.Paths.Contains(structure.Elements["file2.txt"]));


            //子ノードを持つディレクトリノードを一つだけ破棄(※削除ではない)
            Assert.AreEqual(7, tree.RootNode.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["dir1"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["subdir1"]));

            dir1.Dispose();

            Assert.AreEqual(6, tree.RootNode.Children.Length);
            Assert.AreEqual(7, tree.Paths.Length);

            Assert.IsFalse(tree.Paths.Contains(structure.Elements["dir1"]));
            Assert.IsFalse(tree.Paths.Contains(structure.Elements["subdir1"]));
        }

        [TestMethod()]
        public void CreateChildTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);
            var dir1 = tree.GetNode(structure.Elements["dir1"]);
            dir1.Scan();

            //ファイルを一つ生成
            var newFilePath = System.IO.Path.Combine(dir1.FullPath, "file4.txt");
            Assert.IsFalse(tree.Paths.Contains(newFilePath));
            Assert.AreEqual(10, tree.Paths.Length);

            Assert.IsFalse(System.IO.File.Exists(newFilePath));

            dir1.CreateChild("file4.txt", NodeBase.NodeType.File);

            Assert.AreNotEqual(null, dir1["file4.txt"]);
            Assert.AreEqual(newFilePath, dir1["file4.txt"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newFilePath));
            Assert.AreEqual(tree[newFilePath], dir1["file4.txt"]);
            Assert.AreEqual(11, tree.Paths.Length);

            Assert.IsTrue(System.IO.File.Exists(newFilePath));


            var newFileNode = dir1["file4.txt"];
            Assert.AreEqual(tree, newFileNode.Tree);
            Assert.AreEqual(0, newFileNode.Children.Length);
            Assert.AreEqual(newFilePath, newFileNode.FullPath);
            Assert.AreEqual("txt", newFileNode.Extension);
            Assert.AreEqual(false, newFileNode.IsRootNode);
            Assert.AreEqual("file4.txt", newFileNode.Name);
            Assert.AreEqual(dir1, newFileNode.Parent);
            Assert.AreEqual(NodeBase.NodeType.File, newFileNode.Type);


            //フォルダを一つ生成
            var newDirPath = System.IO.Path.Combine(dir1.FullPath, "subdirAdd");
            Assert.IsFalse(tree.Paths.Contains(newDirPath));

            Assert.IsFalse(System.IO.Directory.Exists(newDirPath));

            dir1.CreateChild("subdirAdd", NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, dir1["subdirAdd"]);
            Assert.AreEqual(newDirPath, dir1["subdirAdd"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newDirPath));
            Assert.AreEqual(tree[newDirPath], dir1["subdirAdd"]);
            Assert.AreEqual(12, tree.Paths.Length);

            Assert.IsTrue(System.IO.Directory.Exists(newDirPath));

            var newDirNode = dir1["subdirAdd"];
            Assert.AreEqual(tree, newDirNode.Tree);
            Assert.AreEqual(0, newDirNode.Children.Length);
            Assert.AreEqual(newDirPath, newDirNode.FullPath);
            Assert.AreEqual("", newDirNode.Extension);
            Assert.AreEqual(false, newDirNode.IsRootNode);
            Assert.AreEqual("subdirAdd", newDirNode.Name);
            Assert.AreEqual(dir1, newDirNode.Parent);
            Assert.AreEqual(NodeBase.NodeType.Directory, newDirNode.Type);
        }

        [TestMethod()]
        public void NodeDeleteTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            var dir1 = tree.GetNode(structure.Elements["dir1"]);
            dir1.Scan();

            Assert.AreNotEqual(null, dir1["subdir1"]);
            Assert.AreNotEqual(null, dir1["file2.txt"]);

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Paths.Length);


            //ファイルノードを一つだけ削除
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["file2.txt"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["file2.txt"]));

            var delFilePath = dir1["file2.txt"].FullPath;
            Assert.IsTrue(System.IO.File.Exists(delFilePath));

            dir1["file2.txt"].Delete();

            Assert.AreEqual(1, dir1.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);

            Assert.IsFalse(dir1.Children.Select(n => n.FullPath)
                .Contains(structure.Elements["file2.txt"]));
            Assert.IsFalse(tree.Paths.Contains(structure.Elements["file2.txt"]));

            Assert.IsFalse(System.IO.File.Exists(delFilePath));



            //子ノードを持つディレクトリノードを一つだけ削除
            Assert.AreEqual(7, tree.RootNode.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length); // 9
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["dir1"]));
            Assert.IsTrue(tree.Paths.Contains(structure.Elements["subdir1"]));

            var delDirPath = dir1.FullPath;
            Assert.IsTrue(System.IO.Directory.Exists(delDirPath));

            dir1.Delete();

            Assert.AreEqual(6, tree.RootNode.Children.Length);
            Assert.AreEqual(7, tree.Paths.Length);

            Assert.IsFalse(tree.Paths.Contains(structure.Elements["dir1"]));
            Assert.IsFalse(tree.Paths.Contains(structure.Elements["subdir1"]));

            Assert.IsFalse(System.IO.Directory.Exists(delDirPath));
        }


        [TestMethod()]
        public async Task GetTreeRecursiveAsyncTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);

            this.OutHighlighted(tree.Paths);

            foreach (var path in structure.Directories)
            {
                Assert.IsTrue(tree.Paths.Contains(path));
                var node = tree[path];

                var info = new System.IO.DirectoryInfo(path);
                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual("", node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
                this.OutHighlighted(node.Children.Select(n => n.Name).ToArray());
            }

            foreach (var path in structure.Files)
            {
                Assert.IsTrue(tree.Paths.Contains(path));
                var node = tree[path];

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }
        }


        [TestMethod()]
        public async Task FindTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //get all recursive
            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);

            this.OutHighlighted(tree.Paths);

            var nodes = tree.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            nodes = tree.RootNode.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            var subdir1 = tree.FindAll("subdir1")
                .FirstOrDefault(n => n.Type == Xb.File.Tree.NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, subdir1);
            var path2 = subdir1.FullPath;
            var info2 = new System.IO.DirectoryInfo(path2);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, subdir1.Type);
            Assert.AreEqual(System.IO.Path.GetFileName(path2), subdir1.Name);
            Assert.AreEqual("", subdir1.Extension);
            Assert.AreEqual(info2.LastWriteTime, subdir1.UpdateDate);

            nodes = subdir1.FindAll(".txt");
            Assert.AreEqual(2, nodes.Length);

            var node3 = subdir1.Find("xt");
            Assert.IsTrue((new string[] {"subFile1.txt", "subFile2.txt"}).Contains(node3.Name));

            //get direct-child only
            tree = Xb.File.FileTree.GetTree(baseDir);

            this.OutHighlighted(tree.Paths);

            nodes = tree.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;
                Assert.IsTrue(structure.Files.Contains(path));

                var info = new System.IO.FileInfo(path);
                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);
                Assert.AreEqual(info.LastWriteTime, node.UpdateDate);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }
            Assert.AreEqual(3, nodes.Length);
        }


        [TestMethod()]
        public async Task GetBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);

            var file2 = tree.RootNode["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var slicedBytes = existBytes.Skip(3).Take(5);
            bytes = file2.GetBytes(3, 5);
            Assert.IsTrue(slicedBytes.SequenceEqual(bytes));

            Assert.AreEqual(existBytes.Length, file2.Length);

            Assert.AreEqual(0, tree.RootNode["dir1"].Length);
            Assert.AreEqual(0, tree.RootNode["file1.txt"].Length);

        }

        [TestMethod()]
        public async Task GetReadStreamTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);

            var file2 = tree.RootNode["dir1"]["file2.txt"];

            var stream = file2.GetReadStream();
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = Xb.Byte.GetBytes(stream); //file2.GetBytes();

            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var slicedBytes = existBytes.Skip(3).Take(5);
            bytes = file2.GetBytes(3, 5);
            Assert.IsTrue(slicedBytes.SequenceEqual(bytes));

            stream.Dispose();
        }

        [TestMethod()]
        public void WriteBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = Xb.File.FileTree.GetTree(baseDir);

            //既存ファイルに書き込み
            tree.RootNode["dir1"].Scan();
            var file2 = tree.RootNode["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var newBytes = Encoding.UTF8.GetBytes("書き換え");
            file2.WriteBytes(newBytes);

            //既存より少ないデータで置換されていること
            Assert.IsTrue(newBytes.SequenceEqual(Xb.File.Util.GetBytes(file2.FullPath)));

            newBytes = Encoding.UTF8.GetBytes("新しいファイルだぜ！！！\r\nだぜ！");
            tree.RootNode["dir1"].CreateChild("newFile.txt", NodeBase.NodeType.File);
            var newFile = tree.RootNode["dir1"]["newFile.txt"];
            newFile.WriteBytes(newBytes);

            Assert.IsTrue(newBytes.SequenceEqual(Xb.File.Util.GetBytes(newFile.FullPath)));
        }

        [TestMethod()]
        public async Task TreeGetNodeTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);

            //存在しているものは取得できる
            var subFile1Path = System.IO.Path.Combine(baseDir, "dir1", "subdir1", "subFile1.txt");
            var node = tree.GetNode(subFile1Path);
            Assert.AreNotEqual(null, node);

            //存在しないものは例外
            try
            {
                node = tree.GetNode(System.IO.Path.Combine(baseDir, "not_exists"));
                Assert.Fail();
            }
            catch (Exception)
            {
            }

            //存在しているものは取得できる
            var nodes = tree.GetNodes(new string[]
            {
                baseDir
                , subFile1Path
            });
            Assert.AreEqual(2, nodes.Length);


            //存在しないものは例外
            try
            {
                nodes = tree.GetNodes(new string[]
                {
                    System.IO.Path.Combine(baseDir, "not_exists")
                    , baseDir
                    , subFile1Path
                });
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("not_exists") >= 0);
            }
        }

        [TestMethod()]
        public async Task NodeGetSerializableTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();

            //直下のもの以外は走査しないインスタンス取得
            var tree = await Xb.File.FileTree.GetTreeRecursiveAsync(baseDir);
            var selObj = tree.RootNode.GetSerializable();
            var text = "";

            try
            {
                text = Xb.Type.Json.Stringify(selObj, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            this.Out(text);
        }
    }
}
