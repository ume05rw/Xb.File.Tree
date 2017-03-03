using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextXb;
using Xb.File.Tree;
using Xb.Net;

namespace TestXb
{
    [TestClass()]
    public class SmbTreeTests : FileBase
    {
        private string _server = "XXXX";
        private string _user = "XXXX";
        private string _password = "XXXX";


        private Xb.Net.SmbTree GetTree(string path)
        {
            return Xb.Net.SmbTree.GetTree(this._server
                , path
                , this._user
                , this._password);
        }

        private async Task<Xb.Net.SmbTree> GetTreeRecursiveAsync(string path)
        {
            return await Xb.Net.SmbTree.GetTreeRecursiveAsync(this._server
                , path
                , this._user
                , this._password);
        }

        [TestMethod()]
        public void NoAuthtest()
        {
            var tree = Xb.Net.SmbTree.GetTree("192.168.254.11"
                , "FreeArea/nonAuthDataTest");
            Assert.AreEqual(5, tree.Nodes.Length);


        }

        [TestMethod()]
        public void Createtest()
        {
            var tree = this.GetTree("Apps/tmp");

            var text = Xb.Type.Json.Stringify(tree.RootNode.GetSerializable(), true);
            this.Out(text);
        }

        [TestMethod()]
        public async Task Create2test()
        {
            var tree = await this.GetTreeRecursiveAsync("Others/OtherBook");

            var text = Xb.Type.Json.Stringify(tree.RootNode.GetSerializable(), true);
            this.Out(text);
        }


        [TestMethod()]
        public void TreeCreateTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //ファイル構造生成
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            Assert.AreNotEqual(null, tree.RootNode);
            Assert.AreEqual(tree.RootNode, tree.GetNode("Private/Tmp/baseDir"));
            Assert.AreEqual(8, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir2"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/マルチバイトЙ"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dirNoData"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/file1.txt"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/file3.txt"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/マルチバイトΩ.txt"));
        }

        [TestMethod()]
        public void NodeScanTest1()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            var dir1 = tree["dir1"];

            Assert.AreEqual(0, dir1.Children.Length);
            Assert.AreEqual(8, tree.Paths.Length);

            dir1.Scan();

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.IsTrue(dir1.Children.Select(n => n.Name).Contains("subdir1"));
            Assert.IsTrue(dir1.Children.Select(n => n.Name).Contains("file2.txt"));

            Assert.AreEqual(10, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/subdir1"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/file2.txt"));
        }

        [TestMethod()]
        public void NodeScanTest2()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            var dir1 = tree["dir1"];
            dir1.Scan();
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, dir1.Type);
            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Nodes.Length);

            //baseDir/dir1フォルダを削除して、dir1ファイルを作る。
            var dir1Path = @"R:\Tmp\baseDir\dir1";
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
            catch (IOException ex)
            {
                //dir1実体喪失につき再取得
                dir1 = tree["dir1"];
            }
            catch (Exception ex)
            {
                this.Out(ex.Message);
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

            var file1Path = @"R:\Tmp\baseDir\file1.txt";
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
            catch (IOException ex)
            {
                //file1実体喪失につき再取得
                file1 = tree["file1.txt"];
            }
            catch (Exception ex)
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
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            //Treeインデクサ経由とNodeインデクサ経由で同一オブジェクトが取れる
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir"), tree.RootNode);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/dir1"), tree["dir1"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/dir2"), tree["dir2"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/マルチバイトЙ"), tree["マルチバイトЙ"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/dirNoData"), tree["dirNoData"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/file1.txt"), tree["file1.txt"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/file3.txt"), tree["file3.txt"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/マルチバイトΩ.txt"), tree["マルチバイトΩ.txt"]);

            tree.RootNode["dir1"].Scan();
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/dir1/subdir1"), tree["dir1"]["subdir1"]);
            Assert.AreEqual(tree.GetNode("Private/Tmp/baseDir/dir1/file2.txt"), tree["dir1"]["file2.txt"]);

        }

        [TestMethod()]
        public void NodeDisposeTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            var dir1 = tree["dir1"];
            dir1.Scan();

            Assert.AreNotEqual(null, dir1["subdir1"]);
            Assert.AreNotEqual(null, dir1["file2.txt"]);

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Paths.Length);


            //ファイルノードを一つだけ破棄(※削除ではない)
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath).Contains("Private/Tmp/baseDir/dir1/file2.txt"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/file2.txt"));

            dir1["file2.txt"].Dispose();

            Assert.AreEqual(1, dir1.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);

            Assert.IsFalse(dir1.Children.Select(n => n.FullPath).Contains("Private/Tmp/baseDir/dir1/file2.txt"));
            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1/file2.txt"));


            //子ノードを持つディレクトリノードを一つだけ破棄(※削除ではない)
            Assert.AreEqual(7, tree.RootNode.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/subdir1"));

            dir1.Dispose();

            Assert.AreEqual(6, tree.RootNode.Children.Length);
            Assert.AreEqual(7, tree.Paths.Length);

            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1"));
            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1/subdir1"));
        }

        [TestMethod()]
        public void CreateChildTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            var dir1 = tree["dir1"];
            dir1.Scan();

            //ファイルを一つ生成
            var newFilePath = "R:\\Tmp\\baseDir\\dir1\\file4.txt";
            var newNodePath = "Private/Tmp/baseDir/dir1/file4.txt";
            Assert.IsFalse(tree.Paths.Contains(newNodePath));
            Assert.AreEqual(10, tree.Paths.Length);

            Assert.IsFalse(System.IO.File.Exists(newFilePath));

            dir1.CreateChild("file4.txt", NodeBase.NodeType.File);

            Assert.AreNotEqual(null, dir1["file4.txt"]);
            Assert.AreEqual(newNodePath, dir1["file4.txt"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newNodePath));
            Assert.AreEqual(tree.GetNode(newNodePath), dir1["file4.txt"]);
            Assert.AreEqual(11, tree.Paths.Length);




            var newFileNode = dir1["file4.txt"];
            Assert.AreEqual(tree, newFileNode.Tree);
            Assert.AreEqual(0, newFileNode.Children.Length);
            Assert.AreEqual(newNodePath, newFileNode.FullPath);
            Assert.AreEqual("txt", newFileNode.Extension);
            Assert.AreEqual(false, newFileNode.IsRootNode);
            Assert.AreEqual("file4.txt", newFileNode.Name);
            Assert.AreEqual(dir1, newFileNode.Parent);
            Assert.AreEqual(NodeBase.NodeType.File, newFileNode.Type);


            //フォルダを一つ生成
            var newDirPath = "R:\\Tmp\\baseDir\\dir1\\subdirAdd"; //System.IO.Path.Combine(dir1.FullPath, "subdirAdd");
            newNodePath = "Private/Tmp/baseDir/dir1/subdirAdd";
            Assert.IsFalse(tree.Paths.Contains(newNodePath));

            Assert.IsFalse(System.IO.Directory.Exists(newDirPath));

            dir1.CreateChild("subdirAdd", NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, dir1["subdirAdd"]);
            Assert.AreEqual(newNodePath, dir1["subdirAdd"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newNodePath));
            Assert.AreEqual(tree.GetNode(newNodePath), dir1["subdirAdd"]);
            Assert.AreEqual(12, tree.Paths.Length);



            var newDirNode = dir1["subdirAdd"];
            Assert.AreEqual(tree, newDirNode.Tree);
            Assert.AreEqual(0, newDirNode.Children.Length);
            Assert.AreEqual(newNodePath, newDirNode.FullPath);
            Assert.AreEqual("", newDirNode.Extension);
            Assert.AreEqual(false, newDirNode.IsRootNode);
            Assert.AreEqual("subdirAdd", newDirNode.Name);
            Assert.AreEqual(dir1, newDirNode.Parent);
            Assert.AreEqual(NodeBase.NodeType.Directory, newDirNode.Type);

            Task.Run(() =>
            {
                //リモート共有をマウントしていると、反映が遅い？
                System.Threading.Thread.Sleep(5000);
                Assert.IsTrue(System.IO.File.Exists(newFilePath));
                Assert.IsTrue(System.IO.Directory.Exists(newDirPath));
            }).Wait();
        }

        [TestMethod()]
        public void NodeDeleteTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = this.GetTree("Private/Tmp/baseDir");

            var dir1 = tree["dir1"];
            dir1.Scan();

            Assert.AreNotEqual(null, dir1["subdir1"]);
            Assert.AreNotEqual(null, dir1["file2.txt"]);

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(10, tree.Paths.Length);


            //ファイルノードを一つだけ削除
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                .Contains("Private/Tmp/baseDir/dir1/file2.txt"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/file2.txt"));

            var delFilePath = dir1["file2.txt"].FullPath;
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(baseDir, "dir1\\file2.txt")));

            dir1["file2.txt"].Delete();

            Assert.AreEqual(1, dir1.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length);

            Assert.IsFalse(dir1.Children.Select(n => n.FullPath)
                .Contains("Private/Tmp/baseDir/dir1/file2.txt"));
            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1/file2.txt"));


            Task.Run(() =>
            {
                //リモート共有をマウントしていると、反映が遅い？
                System.Threading.Thread.Sleep(10000);
                Assert.IsFalse(System.IO.File.Exists(Path.Combine(baseDir, "dir1\\file2.txt")));
            }).Wait();


            //子ノードを持つディレクトリノードを一つだけ削除
            Assert.AreEqual(7, tree.RootNode.Children.Length);
            Assert.AreEqual(9, tree.Paths.Length); // 9
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1"));
            Assert.IsTrue(tree.Paths.Contains("Private/Tmp/baseDir/dir1/subdir1"));

            var delDirPath = dir1.FullPath;
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(baseDir, "dir1")));

            dir1.Delete();

            Assert.AreEqual(6, tree.RootNode.Children.Length);
            Assert.AreEqual(7, tree.Paths.Length);

            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1"));
            Assert.IsFalse(tree.Paths.Contains("Private/Tmp/baseDir/dir1/subdir1"));


            Task.Run(() =>
            {
                //リモート共有をマウントしていると、反映が遅い？
                System.Threading.Thread.Sleep(10000);
                Assert.IsFalse(System.IO.Directory.Exists(Path.Combine(baseDir, "dir1")));
            }).Wait();
        }


        [TestMethod()]
        public async Task GetTreeRecursiveAsyncTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            var structure = this.BuildDirectoryTree("R:\\Tmp");

            var tree = await this.GetTreeRecursiveAsync("Private/Tmp/baseDir");

            var node = tree.RootNode;
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("baseDir", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(7, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir", node.FullPath);
            Assert.IsTrue(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir1"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("dir1", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(2, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir1", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir1"]["subdir1"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("subdir1", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(2, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir1/subdir1", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir1"]["subdir1"]["subFile1.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("subFile1.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir1/subdir1/subFile1.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir1"]["subdir1"]["subFile2.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("subFile2.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir1/subdir1/subFile2.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir1"]["file2.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("file2.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir1/file2.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(39, node.Length);

            node = tree["dir2"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("dir2", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(1, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir2", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir2"]["マルチバイト∀"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("マルチバイト∀", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(1, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir2/マルチバイト∀", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dir2"]["マルチバイト∀"]["マルチバイトΠ.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("マルチバイトΠ.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dir2/マルチバイト∀/マルチバイトΠ.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["マルチバイトЙ"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("マルチバイトЙ", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/マルチバイトЙ", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["dirNoData"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual("dirNoData", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/dirNoData", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["file1.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("file1.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/file1.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["file3.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("file3.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/file3.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);

            node = tree["マルチバイトΩ.txt"];
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
            Assert.AreEqual("マルチバイトΩ.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual("Private/Tmp/baseDir/マルチバイトΩ.txt", node.FullPath);
            Assert.IsFalse(node.IsRootNode);
            Assert.AreEqual(0, node.Length);
        }


        [TestMethod()]
        public async Task FindTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");

            Xb.File.Util.Delete(baseDir);
            Assert.IsFalse(Directory.Exists(baseDir));

            //get all recursive
            var structure = this.BuildDirectoryTree("R:\\Tmp");
            var tree = await this.GetTreeRecursiveAsync("Private/Tmp/baseDir");

            this.OutHighlighted(tree.Paths);


            var nodes = tree.FindAll(".txt");
            var results = new INode[]
            {
                tree["dir1"]["subdir1"]["subFile1.txt"],
                tree["dir1"]["subdir1"]["subFile2.txt"],
                tree["dir1"]["file2.txt"],
                tree["dir2"]["マルチバイト∀"]["マルチバイトΠ.txt"],
                tree["file1.txt"],
                tree["file3.txt"],
                tree["マルチバイトΩ.txt"]
            };
            foreach (var node in nodes)
            {
                Assert.IsTrue(results.Contains(node));
            }
            Assert.AreEqual(7, nodes.Length);



            nodes = tree.RootNode.FindAll(".txt");
            results = new INode[]
            {
                tree["dir1"]["subdir1"]["subFile1.txt"],
                tree["dir1"]["subdir1"]["subFile2.txt"],
                tree["dir1"]["file2.txt"],
                tree["dir2"]["マルチバイト∀"]["マルチバイトΠ.txt"],
                tree["file1.txt"],
                tree["file3.txt"],
                tree["マルチバイトΩ.txt"]
            };
            foreach (var node in nodes)
            {
                Assert.IsTrue(results.Contains(node));
            }
            Assert.AreEqual(7, nodes.Length);


            var subdir1 = tree.FindAll("subdir1")
                .FirstOrDefault(n => n.Type == Xb.File.Tree.NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, subdir1);
            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, subdir1.Type);
            Assert.AreEqual("subdir1", subdir1.Name);
            Assert.AreEqual("", subdir1.Extension);


            nodes = subdir1.FindAll(".txt");
            Assert.AreEqual(2, nodes.Length);

            var node3 = subdir1.Find("xt");
            Assert.IsTrue((new string[] {"subFile1.txt", "subFile2.txt"}).Contains(node3.Name));

            //get direct-child only
            tree = this.GetTree("Private/Tmp/baseDir");



            nodes = tree.FindAll(".txt");
            results = new INode[]
            {
                tree["file1.txt"],
                tree["file3.txt"],
                tree["マルチバイトΩ.txt"]
            };
            foreach (var node in nodes)
            {
                Assert.IsTrue(results.Contains(node));
            }
            Assert.AreEqual(3, nodes.Length);
        }


        [TestMethod()]
        public async Task GetBytesTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");
            var tree = await this.GetTreeRecursiveAsync("Private/Tmp/baseDir");


            var file2 = tree["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var slicedBytes = existBytes.Skip(3).Take(5);
            bytes = file2.GetBytes(3, 5);
            Assert.IsTrue(slicedBytes.SequenceEqual(bytes));
        }


        [TestMethod()]
        public void WriteBytesTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");
            var tree = this.GetTree("Private/Tmp/baseDir");

            //既存ファイルに書き込み
            tree.RootNode["dir1"].Scan();
            var file2 = tree.RootNode["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var newBytes = Encoding.UTF8.GetBytes("書き換え");
            file2.WriteBytes(newBytes);

            //既存より少ないデータで置換されていること
            Assert.IsTrue(newBytes.SequenceEqual(file2.GetBytes()));

            newBytes = Encoding.UTF8.GetBytes("新しいファイルだぜ！！！\r\nだぜ！");
            var newNode = tree["dir1"].CreateChild("newFile.txt", NodeBase.NodeType.File);
            var newFile = tree.RootNode["dir1"]["newFile.txt"];
            Assert.AreEqual(newNode, newFile);
            newFile.WriteBytes(newBytes);

            Assert.IsTrue(newBytes.SequenceEqual(newFile.GetBytes()));
        }

        [TestMethod()]
        public async Task TreeGetNodeTest()
        {
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = await this.GetTreeRecursiveAsync("Private/Tmp/baseDir");

            //存在しているものは取得できる
            var subFile1Path = "Private/Tmp/baseDir/dir1/subdir1/subFile1.txt";
            var node = tree.GetNode(subFile1Path);
            Assert.AreNotEqual(null, node);

            //存在しないものは例外
            try
            {
                node = tree.GetNode("Private/Tmp/baseDir/dir1/not_exists");
                Assert.Fail();
            }
            catch (Exception)
            {
            }

            //存在しているものは取得できる
            var nodes = tree.GetNodes(new string[]
            {
                "Private/Tmp/baseDir"
                , "Private/Tmp/baseDir/dir2/マルチバイト∀/マルチバイトΠ.txt"
            });
            Assert.AreEqual(2, nodes.Length);


            //存在しないものは例外
            try
            {
                nodes = tree.GetNodes(new string[]
                {

                    "Private/Tmp/baseDir"
                    , "Private/Tmp/baseDir/not_exists"
                    , "Private/Tmp/baseDir/dir2/マルチバイト∀/マルチバイトΠ.txt"
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
            var curDir = "R:\\Tmp";
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree("R:\\Tmp");

            //直下のもの以外は走査しないインスタンス取得
            var tree = await this.GetTreeRecursiveAsync("Private/Tmp/baseDir");

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

        [TestMethod()]
        public async Task Streamtest()
        {
            var tree = this.GetTree("Apps/Others/[BinaryEditor] Stirling.zip");
            var stream = tree.RootNode.GetReadStream();

            var zip = await Xb.File.ZipTree.GetTreeAsync(stream);
            foreach (var node in zip.Nodes)
            {
                if (node.Type == NodeBase.NodeType.File)
                {
                    var resultBytes = node.GetBytes();
                }

                this.Out(node.FullPath);
            }

            stream.Dispose();
            zip.Dispose();

        }

        [TestMethod()]
        public async Task TreeGetSharesAsyncTest()
        {
            var shares = await Xb.Net.SmbTree.GetSharesAsync(this._server);
            this.Out(Xb.Type.Json.Stringify(shares, true));
        }

        [TestMethod()]
        public async Task TreeGetSharesAsyncTest2()
        {
            var shares = await Xb.Net.SmbTree.GetSharesAsync();
            this.Out(Xb.Type.Json.Stringify(shares, true));
        }

        [TestMethod()]
        public async Task TreeGetServersAsyncTest()
        {
            var shares = await Xb.Net.SmbTree.GetServersAsync();
            this.Out(Xb.Type.Json.Stringify(shares, true));
        }

        [TestMethod()]
        public async Task FileNameTest()
        {
            try
            {
                var tree1 = Xb.Net.SmbTree.GetTree(
                    "192.168.254.11"
                    , "Others/Photo/ビビアン・スー - Angel.zip"
                    , "XXX"
                    , "XXX");

                var tree = Xb.Net.SmbTree.GetTree(
                    "192.168.254.11"
                    , "Others/Photo/風景#壁紙"
                    , "XXX"
                    , "XXX");
            }
            catch (Exception ex)
            {
                this.Out(ex.Message);
                throw;
            }

            var a = 1;

            //this.Out(Xb.Type.Json.Stringify(shares, true));
        }
    }
}
