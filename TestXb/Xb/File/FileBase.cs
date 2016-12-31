using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestXb;

namespace TextXb
{
    public class FileBase : TestBase
    {
        public DirectoryStructure BuildDirectoryTree(string targetDir = null)
        {
            // baseDir
            //   |
            //   +-dir1
            //   |   |
            //   |   +-subdir1
            //   |   |   |
            //   |   |   +-subFile1.txt
            //   |   |   |
            //   |   |   +-subFile2.txt
            //   |   |
            //   |   +-file2.txt
            //   |
            //   +-dir2
            //   |   |
            //   |   +-マルチバイト∀
            //   |        |
            //   |        +-マルチバイトΠ.txt
            //   |
            //   +-マルチバイトЙ
            //   |
            //   +-dirNoData
            //   |
            //   +-file1.txt
            //   |
            //   +-file3.txt
            //   |
            //   +-マルチバイトΩ.txt
            //
            // ディレクトリ：7
            // ファイル　　：7
            // 計　　　　　：14

            var curDir = targetDir ?? Directory.GetCurrentDirectory();
            
            var baseDir = Path.Combine(curDir, "baseDir");
            var dir1 = Path.Combine(baseDir, "dir1");
            var dir2 = Path.Combine(baseDir, "dir2");
            var dirMb = Path.Combine(baseDir, "マルチバイトЙ");
            var dirNoData = Path.Combine(baseDir, "dirNoData");
            var subdir1 = Path.Combine(dir1, "subdir1");
            var subdirMb = Path.Combine(dir2, "マルチバイト∀");

            Xb.File.Util.Delete(baseDir);

            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);
            Directory.CreateDirectory(dirMb);
            Directory.CreateDirectory(dirNoData);
            Directory.CreateDirectory(subdir1);
            Directory.CreateDirectory(subdirMb);

            var file1 = Path.Combine(baseDir, "file1.txt");
            var file2 = Path.Combine(dir1, "file2.txt");
            var file3 = Path.Combine(baseDir, "file3.txt");
            var fileMb = Path.Combine(baseDir, "マルチバイトΩ.txt");
            var subFile1 = Path.Combine(subdir1, "subFile1.txt");
            var subFile2 = Path.Combine(subdir1, "subFile2.txt");
            var subFileMb = Path.Combine(subdirMb, "マルチバイトΠ.txt");

            (File.Create(file1)).Dispose();
            //(File.Create(file2)).Dispose();
            Xb.File.Util.WriteText(file2, "中身を書き込んであるんだよ");
            (File.Create(file3)).Dispose();
            (File.Create(fileMb)).Dispose();
            (File.Create(subFile1)).Dispose();
            (File.Create(subFile2)).Dispose();
            (File.Create(subFileMb)).Dispose();

            var directories = new List<string>();
            var files = new List<string>();

            directories.Add(baseDir);
            directories.Add(dir1);
            directories.Add(dir2);
            directories.Add(dirMb);
            directories.Add(dirNoData);
            directories.Add(subdir1);
            directories.Add(subdirMb);
            files.Add(file1);
            files.Add(file2);
            files.Add(file3);
            files.Add(fileMb);
            files.Add(subFile1);
            files.Add(subFile2);
            files.Add(subFileMb);

            return new DirectoryStructure(directories.ToArray(), files.ToArray());
        }

        public class DirectoryStructure
        {
            public string[] Directories { get; private set; }
            public string[] Files { get; private set; }

            public Dictionary<string, string> Elements { get; }
            

            public DirectoryStructure(string[] directories, string[] files)
            {
                this.Directories = directories;
                this.Files = files;

                this.Elements = new Dictionary<string, string>();

                foreach (var directory in this.Directories)
                    this.Elements.Add(System.IO.Path.GetFileName(directory), directory);

                foreach (var file in this.Files)
                    this.Elements.Add(System.IO.Path.GetFileName(file), file);

            }
        }
    }
}
