Xb.File.Tree / Xb.Net.SmbTree
====

Ready to Xamarin & .NET Core, File-System Library.

## Description
It's File-System Libraries, Get a File-System as a Tree-Structure Objects, and Operate it.

Xb.File.Tree: for Local File-System, and Zip-Archive Files.(includes Tree-Structure Defenitions.)  
Xb.Net.SmbTree: for files on SMB/Cifs Networks.

Supports .NET4.5.1, .NET Standard1.3

## Requirement
Xb.File.Tree:  
[System.IO.FileSystem](https://www.nuget.org/packages/System.IO.FileSystem/)  
[System.IO.Compression](https://www.nuget.org/packages/System.IO.Compression/)  
[System.IO.Compression.ZipFile](https://www.nuget.org/packages/System.IO.Compression.ZipFile/)  
[Xb.Core](https://www.nuget.org/packages/Xb.Core/)  

Xb.Net.SmbTree:  
[SharpCifs.Std](https://www.nuget.org/packages/SharpCifs.Std/)  
[Xb.File.Tree](https://www.nuget.org/packages/Xb.File.Tree/)  
[Xb.Core](https://www.nuget.org/packages/Xb.Core/)  

## Usage
1. Add NuGet Package [Xb.File.Tree](https://www.nuget.org/packages/Xb.File.Tree/), [Xb.Net.SmbTree](https://www.nuget.org/packages/Xb.Net.SmbTree/) to your project.
2. Exec Tree's Static Method to Get ITree-object, and do any()

Namespace and Methods are...


    ・Xb.File
          |
          +- .ITree(Instance)
          |    |
          |    +- .Exists(string path)
          |    |   Validate own-INode path
          |    |
          |    +- .GetNode(string path)
          |    |   Get matched one INode-object by fullpath
          |    |
          |    +- .GetNodes(ICollection<string> paths)
          |    |   Get matched INode-objects by fullpath
          |    |
          |    +- .Find(string needle)
          |    |   Get first-INode of matched needle
          |    |
          |    +- .FindAll(string needle)
          |    |   Get all-INodes of matched needle
          |    |
          |    +- .ScanRecursiveAsync()
          |        Tree-Structure Re-Scan recursive(VERY HEAVY!)
          |
          +- .INode(Instance)
          |    |
          |    +- .Scan()
          |    |   Scan child-INodes
          |    |
          |    +- .ScanRecursiveAsync()
          |    |   Scan refresh nodes recursive on async
          |    |
          |    +- .GetSerializable()
          |    |   Get serializable-object of tree structure(ex: JSON-Converting)
          |    |
          |    +- .GetAllChildrenRecursive()
          |    |   Get INode-Array of all-children, exec recursive
          |    |
          |    +- .Find(string needle)
          |    |   Get first-INode of matched needle
          |    |
          |    +- .FindAll(string needle)
          |    |   Get all-INodes Array of matched needle
          |    |
          |    +- .GetBytes()
          |    |   Get byte-array of this INode
          |    |
          |    +- .GetBytes(long offset, int length)
          |    |   Get byte-array of this INode, sliced by passing values
          |    |
          |    +- .GetBytesAsync()
          |    |   Get byte-array of INode on async
          |    |
          |    +- .GetBytesAsync(long offset, int length)
          |    |   Get byte-array of node on async, sliced by passing values
          |    |
          |    +- .GetReadStream()
          |    |   Get stream for read-only
          |    |
          |    +- .WriteBytes(byte[] bytes)
          |    |   Overwrite data of INode
          |    |
          |    +- .WriteBytesAsync(byte[] bytes)
          |    |   Overwrite data of INode on async
          |    |
          |    +- .CreateChild(string name, Xb.File.Tree.NodeBase.NodeType type)
          |    |   Create child-INode
          |    |
          |    +- .Delete()
          |        Delete myself from ITree/INode's children
          |
          |
          |
          +- .FileTree(Static)
          |    |
          |    +- .GetTree(string path)
          |    |   Returns ITree-object of Local File-System with the passing path as the root
          |    |
          |    +- .GetTreeRecursiveAsync(string path)
          |        Returns a Tree object of Local File-System that scans all nodes under the passing path(VERY HEAVY!)
          |
          |
          +- .ZipTree(Static)
               |
               +- .GetTreeAsync(string zipFileName,
               |                bool readOnly = true,
               |                Encoding encoding = null)
               |   Returns ITree-object of ZipArchive with the passing zip-file as the root
               |
               +- .GetTreeAsync(Stream readableStream,
                                Encoding encoding = null)
                   Returns *READONLY* ITree-object of ZipArchive with the passing zip-file as the root

    ・Xb.Net
          |
          +- .SmbTree(Static)
               |
               +- .GetTree(string serverName,
               |           string path,
               |           string userName = null,
               |           string password = null,
               |           string domain = null)
               |   Get ITree-object of SMB/Cifs-File-System with the passing path as the root
               |
               +- .GetTreeRecursiveAsync(string serverName,
               |                         string path,
               |                         string userName = null
               |                         string password = null
               |                         string domain = null)
               |   Get ITree-object of SMB/Cifs-File-System that scans all nodes under the passing path (VERY HEAVY!)               |
               |
               +- .Exists(string serverName,
               |          string path,
               |          string userName = null,
               |          string password = null,
               |          string domain = null)
               |   Validate passing path
               |
               +- .GetServersAsync()
               |   Get SMB-servers on LAN
               |
               +- .GetServersAsync(IPAddress address)
               |   Get SMB-servers on passing LAN
               |
               +- .GetSharesAsync()
               |   Get server, shared-folder's names on LAN
               |
               +- .GetSharesAsync(string serverAddress)
                   Get shared-folder names on server


## Licence
Xb.File.Tree:  
[MIT Licence](https://github.com/ume05rw/Xb.File.Tree/blob/master/Xb.File.Tree.STD1.3/LICENSE)

Xb.Net.SmbTree:  
[LGPL2.1](https://github.com/ume05rw/Xb.File.Tree/blob/master/Xb.Net.SmbTree.STD1.3/LICENSE)
## Author

[Do-Be's](http://dobes.jp)
