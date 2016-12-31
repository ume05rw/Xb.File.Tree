using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xb.File.Tree;

namespace TextXb
{
    [TestClass()]
    public class ZipTreeTests : FileBase
    {
        [TestMethod()]
        public async Task TreeCreateTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);

            Assert.AreNotEqual(null, tree.RootNode);
            Assert.AreEqual(15, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains("")); //rootNode
            Assert.IsTrue(tree.Paths.Contains("baseDir"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile1.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile2.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\file2.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀\\マルチバイトΠ.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトЙ"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dirNoData"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file1.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file3.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトΩ.txt"));

            var node = tree.RootNode["baseDir"];
            Assert.AreEqual("baseDir", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(7, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir1"];
            Assert.AreEqual("dir1", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(2, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir1"]["subdir1"];
            Assert.AreEqual("subdir1", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(2, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir1"]["subdir1"]["subFile1.txt"];
            Assert.AreEqual("subFile1.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir1"]["subdir1"]["subFile2.txt"];
            Assert.AreEqual("subFile2.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir1"]["file2.txt"];
            Assert.AreEqual("file2.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(39, node.Length);

            node = tree.RootNode["baseDir"]["dir2"];
            Assert.AreEqual("dir2", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(1, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir2"]["マルチバイト∀"];
            Assert.AreEqual("マルチバイト∀", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(1, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dir2"]["マルチバイト∀"]["マルチバイトΠ.txt"];
            Assert.AreEqual("マルチバイトΠ.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["マルチバイトЙ"];
            Assert.AreEqual("マルチバイトЙ", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["dirNoData"];
            Assert.AreEqual("dirNoData", node.Name);
            Assert.AreEqual("", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.Directory, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["file1.txt"];
            Assert.AreEqual("file1.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["file3.txt"];
            Assert.AreEqual("file3.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            node = tree.RootNode["baseDir"]["マルチバイトΩ.txt"];
            Assert.AreEqual("マルチバイトΩ.txt", node.Name);
            Assert.AreEqual("txt", node.Extension);
            Assert.AreEqual(NodeBase.NodeType.File, node.Type);
            Assert.AreEqual(0, node.Children.Length);
            Assert.AreEqual(0, node.Length);

            tree.Dispose();
        }


        [TestMethod()]
        public async Task CreateChildTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName, false);

            var dir1 = tree.RootNode["baseDir"]["dir1"];

            //ファイルを一つ生成
            var newFilePath = System.IO.Path.Combine(dir1.FullPath, "file4.txt");
            Assert.IsFalse(tree.Paths.Contains(newFilePath));
            Assert.AreEqual(15, tree.Paths.Length);

            dir1.CreateChild("file4.txt", NodeBase.NodeType.File);

            Assert.AreNotEqual(null, dir1["file4.txt"]);
            Assert.AreEqual(newFilePath, dir1["file4.txt"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newFilePath));
            Assert.AreEqual(tree[newFilePath], dir1["file4.txt"]);
            Assert.AreEqual(16, tree.Paths.Length);


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

            dir1.CreateChild("subdirAdd", NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, dir1["subdirAdd"]);
            Assert.AreEqual(newDirPath, dir1["subdirAdd"].FullPath);
            Assert.IsTrue(tree.Paths.Contains(newDirPath));
            Assert.AreEqual(tree[newDirPath], dir1["subdirAdd"]);
            Assert.AreEqual(17, tree.Paths.Length);

            var newDirNode = dir1["subdirAdd"];
            Assert.AreEqual(tree, newDirNode.Tree);
            Assert.AreEqual(0, newDirNode.Children.Length);
            Assert.AreEqual(newDirPath, newDirNode.FullPath);
            Assert.AreEqual("", newDirNode.Extension);
            Assert.AreEqual(false, newDirNode.IsRootNode);
            Assert.AreEqual("subdirAdd", newDirNode.Name);
            Assert.AreEqual(dir1, newDirNode.Parent);
            Assert.AreEqual(NodeBase.NodeType.Directory, newDirNode.Type);

            tree.Dispose();
            Xb.File.Util.Unzip(zipName);
            Assert.IsTrue(File.Exists(Path.Combine(baseDir, "dir1", "file4.txt")));
            Assert.IsTrue(Directory.Exists(Path.Combine(baseDir, "dir1", "subdirAdd")));
        }

        [TestMethod()]
        public async Task NodeDeleteTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName, false);

            var dir1 = tree.RootNode["baseDir"]["dir1"];

            Assert.AreNotEqual(null, dir1["subdir1"]);
            Assert.AreNotEqual(null, dir1["file2.txt"]);

            Assert.AreEqual(2, dir1.Children.Length);
            Assert.AreEqual(15, tree.Paths.Length);


            //ファイルノードを一つだけ削除
            Assert.IsTrue(dir1.Children.Select(n => n.FullPath)
                              .Contains("baseDir\\dir1\\file2.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\file2.txt"));

            var delFilePath = dir1["file2.txt"].FullPath;

            dir1["file2.txt"].Delete();

            Assert.AreEqual(1, dir1.Children.Length);
            Assert.AreEqual(14, tree.Paths.Length);

            Assert.IsFalse(dir1.Children.Select(n => n.FullPath)
                .Contains("baseDir\\dir1\\file2.txt"));
            Assert.IsFalse(tree.Paths.Contains("baseDir\\dir1\\file2.txt"));
            

            //子ノードを持つディレクトリノードを一つだけ削除
            Assert.AreEqual(7, tree.RootNode["baseDir"].Children.Length);
            Assert.AreEqual(14, tree.Paths.Length);
            Assert.IsTrue(tree.Paths.Contains("baseDir"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile1.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile2.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀\\マルチバイトΠ.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトЙ"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dirNoData"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file1.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file3.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトΩ.txt"));

            var delDirPath = dir1.FullPath;
            dir1.Delete();

            Assert.AreEqual(6, tree.RootNode["baseDir"].Children.Length);
            Assert.AreEqual(10, tree.Paths.Length);

            Assert.IsTrue(tree.Paths.Contains("baseDir"));
            Assert.IsFalse(tree.Paths.Contains("baseDir\\dir1"));
            Assert.IsFalse(tree.Paths.Contains("baseDir\\dir1\\subdir1"));
            Assert.IsFalse(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile1.txt"));
            Assert.IsFalse(tree.Paths.Contains("baseDir\\dir1\\subdir1\\subFile2.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dir2\\マルチバイト∀\\マルチバイトΠ.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトЙ"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\dirNoData"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file1.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\file3.txt"));
            Assert.IsTrue(tree.Paths.Contains("baseDir\\マルチバイトΩ.txt"));

            tree.Dispose();
            Xb.File.Util.Unzip(zipName);

            Assert.IsTrue(Directory.Exists(Path.Combine(curDir, "baseDir")));
            Assert.IsFalse(Directory.Exists(Path.Combine(curDir, "baseDir\\dir1")));
            Assert.IsFalse(Directory.Exists(Path.Combine(curDir, "baseDir\\dir1\\subdir1")));
            Assert.IsFalse(File.Exists(Path.Combine(curDir, "baseDir\\dir1\\subdir1\\subFile1.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(curDir, "baseDir\\dir1\\subdir1\\subFile2.txt")));
            Assert.IsTrue(Directory.Exists(Path.Combine(curDir, "baseDir\\dir2")));
            Assert.IsTrue(Directory.Exists(Path.Combine(curDir, "baseDir\\dir2\\マルチバイト∀")));
            Assert.IsTrue(File.Exists(Path.Combine(curDir, "baseDir\\dir2\\マルチバイト∀\\マルチバイトΠ.txt")));
            Assert.IsTrue(Directory.Exists(Path.Combine(curDir, "baseDir\\マルチバイトЙ")));
            Assert.IsTrue(Directory.Exists(Path.Combine(curDir, "baseDir\\dirNoData")));
            Assert.IsTrue(File.Exists(Path.Combine(curDir, "baseDir\\file1.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(curDir, "baseDir\\file3.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(curDir, "baseDir\\マルチバイトΩ.txt")));
        }


        [TestMethod()]
        public async Task FindTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);

            this.OutHighlighted(tree.Paths);

            var nodes = tree.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;

                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);

                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            nodes = tree.RootNode.FindAll(".txt");
            foreach (var node in nodes)
            {
                var path = node.FullPath;

                Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.File, node.Type);
                Assert.AreEqual(System.IO.Path.GetFileName(path), node.Name);
                Assert.AreEqual(System.IO.Path.GetDirectoryName(path), node.Parent.FullPath);
                Assert.AreEqual(System.IO.Path.GetExtension(path).TrimStart('.'), node.Extension);


                this.Out(path);
                this.Out(
                    $"Name: {node.Name}, Ext: {node.Extension}, Date: {node.UpdateDate}, ChildCount: {node.Children.Length}");
            }

            Assert.AreEqual(7, nodes.Length);


            var subdir1 = tree.FindAll("subdir1")
                .FirstOrDefault(n => n.Type == Xb.File.Tree.NodeBase.NodeType.Directory);

            Assert.AreNotEqual(null, subdir1);
            var path2 = subdir1.FullPath;

            Assert.AreEqual(Xb.File.Tree.NodeBase.NodeType.Directory, subdir1.Type);
            Assert.AreEqual(System.IO.Path.GetFileName(path2), subdir1.Name);
            Assert.AreEqual("", subdir1.Extension);

            nodes = subdir1.FindAll(".txt");
            Assert.AreEqual(2, nodes.Length);

            var node3 = subdir1.Find("xt");
            Assert.IsTrue((new string[] { "subFile1.txt", "subFile2.txt" }).Contains(node3.Name));

            tree.Dispose();
        }


        [TestMethod()]
        public async Task GetBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);


            var file2 = tree.RootNode["baseDir"]["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            tree.Dispose();
        }

        [TestMethod()]
        public async Task GetReadStreamTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);


            var file2 = tree.RootNode["baseDir"]["dir1"]["file2.txt"];
            var stream = file2.GetReadStream();

            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = Xb.Byte.GetBytes(stream);
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            tree.Dispose();
            stream.Dispose();
        }

        [TestMethod()]
        public async Task WriteBytesTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName, false);

            //既存ファイルに書き込み
            var file2 = tree.RootNode["baseDir"]["dir1"]["file2.txt"];
            var existBytes = Encoding.UTF8.GetBytes("中身を書き込んであるんだよ");
            var bytes = file2.GetBytes();
            Assert.IsTrue(existBytes.SequenceEqual(bytes));

            var newBytes = Encoding.UTF8.GetBytes("書き換え");
            file2.WriteBytes(newBytes);

            //既存より少ないデータで置換されていること
            Assert.IsTrue(newBytes.SequenceEqual(file2.GetBytes()));

            newBytes = Encoding.UTF8.GetBytes("新しいファイルだぜ！！！\r\nだぜ！");
            tree.RootNode["baseDir"]["dir1"].CreateChild("newFile.txt", NodeBase.NodeType.File);
            var newFile = tree.RootNode["baseDir"]["dir1"]["newFile.txt"];
            newFile.WriteBytes(newBytes);

            Assert.IsTrue(newBytes.SequenceEqual(newFile.GetBytes()));

            tree.Dispose();
        }

        [TestMethod()]
        public async Task TreeGetNodeTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);

            //存在しているものは取得できる
            var subFile1Path = System.IO.Path.Combine("baseDir", "dir1", "subdir1", "subFile1.txt");
            var node = tree.GetNode(subFile1Path);
            Assert.AreNotEqual(null, node);

            //存在しないものは例外
            try
            {
                node = tree.GetNode(System.IO.Path.Combine("baseDir", "not_exists"));
                Assert.Fail();
            }
            catch (Exception)
            {
            }

            //存在しているものは取得できる
            var nodes = tree.GetNodes(new string[]
            {
                "baseDir"
                , subFile1Path
            });
            Assert.AreEqual(2, nodes.Length);


            //存在しないものは例外
            try
            {
                nodes = tree.GetNodes(new string[]
                {
                    System.IO.Path.Combine("baseDir", "not_exists")
                    , "baseDir"
                    , subFile1Path
                });
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("not_exists") >= 0);
            }

            tree.Dispose();
        }

        [TestMethod()]
        public async Task NodeGetSerializableTest()
        {
            var curDir = Directory.GetCurrentDirectory();
            var baseDir = Path.Combine(curDir, "baseDir");
            Xb.File.Util.Delete(baseDir);
            var structure = this.BuildDirectoryTree();
            Xb.File.Util.ToZip(baseDir);
            Xb.File.Util.Delete(baseDir);
            var zipName = $"{baseDir}.zip";
            var tree = await Xb.File.ZipTree.GetTreeAsync(zipName);

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

            tree.Dispose();
        }
    }
}
