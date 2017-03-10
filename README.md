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
2. Create Instance of Subclass, and do any()

Namespace and Methods are...


    ・Xb.File.Tree
         |
         +- .DbBase(Instance)
         |    |
         |    +- .Quote(string text,
         |    |         LikeMarkPosition likeMarkPos = LikeMarkPosition.None)
         |    |   Get Quoted-String for SQL value
         |    |
         |    +- .Execute(string sql,
         |    |           DbParameter[] parameters = null)
         |    |   Execute Non-Select query, Get effected row count
         |    |
         |    +- .ExecuteAsync(string sql,
         |    |                DbParameter[] parameters = null)
         |    |   Execute Non-Select query on async, Get effected row count
         |    |
         |    +- .GetReader(string sql, 
         |    |             DbParameter[] parameters = null)
         |    |   Execute Select query, Get DbDataReader object.
         |    |
         |    +- .GetReaderAsync(string sql, 
         |    |                  DbParameter[] parameters = null)
         |    |   Execute Select query on async, Get DbDataReader object.
         |    |
         |    +- .Query(string sql, 
         |    |         DbParameter[] parameters = null)
         |    |   Execute Select query, Get Xb.Db.ResultTable
         |    |
         |    +- .QueryAsync(string sql, 
         |    |              DbParameter[] parameters = null)
         |    |   Execute Select query on async, Get Xb.Db.ResultTable(like DataTable)
         |    |
         |    +- .Query<T>(string sql,
         |    |            DbParameter[] parameters = null)
         |    |   Execute Select query, Get array of Generic-Type object.
         |    |
         |    +- .QueryAsync<T>(string sql,
         |    |                 DbParameter[] parameters = null)
         |    |   Execute Select query on async, Get Generic-Type object.
         |    |
         |    +- .Find(string tableName,
         |    |        string whereString)
         |    |   Get first matched Xb.Db.ResultRow(like DataRow)
         |    |
         |    +- .FindAsync(string tableName,
         |    |             string whereString)
         |    |   Get first matched Xb.Db.ResultRow(like DataRow) on async
         |    |
         |    +- .FindAll(string tableName,
         |    |           string whereString = null,
         |    |           string orderString = null)
         |    |   Get matched all rows(Xb.DbResultTable, like DataTable)
         |    |
         |    +- .FindAllAsync(string tableName,
         |    |                string whereString = null,
         |    |                string orderString = null)
         |    |   Get matched all rows(Xb.DbResultTable, like DataTable) on async
         |    |
         |    +- .BeginTransaction()
         |    |   Start transaction
         |    |
         |    +- .BeginTransactionAsync()
         |    |   Start transaction on async
         |    |
         |    +- .CommitTransaction()
         |    |   Commit transaction
         |    |
         |    +- .CommitTransactionAsync()
         |    |   Commit transaction on async
         |    |
         |    +- .RollbackTransaction()
         |    |   Rollback transaction
         |    |
         |    +- .RollbackTransactionAsync()
         |    |   Rollback transaction on async
         |    |
         |    +- .BackupDbAsync(string fileName)
         |        Get Database backup file on async
         |
         |
         +- .Model(Instance)
              |
              +- .Constructor(Xb.Db.DbBase db,
              |               Xb.Db.DbBase.Structure[] infoRows)
              |   Create instance of Table-Model
              |
              +- .GetColumn(string columnName)
              |   Get Xb.Db.Model.Column-object by name
              |
              +- .GetColumn(int index)
              |   Get Xb.Db.Column object by index
              |
              +- .Find(object primaryKeyValue)
              |   Get first matched Xb.Db.ResultRow(like DataRow)
              |
              +- .Find(params object[] primaryKeyValues)
              |   Get first matched Xb.Db.ResultRow(like DataRow)
              |
              +- .FindAsync(object primaryKeyValue)
              |   Get first matched Xb.Db.ResultRow(like DataRow) on async
              |
              +- .FindAsync(params object[] primaryKeyValues)
              |   Get first matched Xb.Db.ResultRow(like DataRow) on async
              |
              +- .FindAll(string whereString = null,
              |           string orderString = null)
              |   Get matched Xb.Db.ResultTable(like DataTable)
              |
              +- .FindAllAsync(string whereString = null,
              |                string orderString = null)
              |   Get matched Xb.Db.ResultTable(like DataTable) on async
              |
              +- .NewRow()
              |   Get new Xb.Db.ResultRow for CRUD
              |
              +- .Validate(Xb.Db.ResultRow row)
              |   Validate values of Xb.Db.ResultRow
              |
              +- .Write(Xb.Db.ResultRow row,
              |         params string[] excludeColumnsOnUpdate)
              |   Write value of Xb.Db.ResultRow to Database
              |
              +- .WriteAsync(Xb.Db.ResultRow row,
              |              params string[] excludeColumnsOnUpdate)
              |   Write value of Xb.Db.ResultRow to Database on async
              |
              +- .Insert(Xb.Db.ResultRow row)
              |   Execute [INSERT] SQL-Command
              |
              +- .InsertAsync(Xb.Db.ResultRow row)
              |   Execute [INSERT] SQL-Command on async
              |
              +- .Update(Xb.Db.ResultRow row,
              |          string[] keyColumns = null,
              |          string[] excludeColumns = null)
              |   Execute [UPDATE] SQL-Command
              |
              +- .UpdateAsync(Xb.Db.ResultRow row,
              |               string[] keyColumns = null,
              |               string[] excludeColumns = null)
              |   Execute [UPDATE] SQL-Command on async
              |
              +- .Delete(Xb.Db.ResultRow row,
              |          params string[] keyColumns)
              |   Execute [DELETE] SQL-Command
              |
              +- .DeleteAsync(Xb.Db.ResultRow row,
              |               params string[] keyColumns)
              |   Execute [DELETE] SQL-Command on async
              |
              +- .ReplaceUpdate(List<Xb.Db.ResultRow> drsAfter,
              |                 List<Xb.Db.ResultRow> drsBefore = null,
              |                 params string[] excludeColumnsOnUpdate)
              |   Update the difference of new-rows and old-rows
              |
              +- .ReplaceUpdate(Xb.Db.ResultTable dtAfter,
              |                 Xb.Db.ResultTable dtBefore = null,
              |                 params string[] excludeColumnsOnUpdate)
              |   Update the difference of new-rows and old-rows
              |
              +- .ReplaceUpdateAsync(List<Xb.Db.ResultRow> drsAfter,
              |                      List<Xb.Db.ResultRow> drsBefore = null,
              |                      params string[] excludeColumnsOnUpdate)
              |   Update the difference of new-rows and old-rows on async
              |
              +- .ReplaceUpdateAsync(Xb.Db.ResultTable dtAfter,
                                     Xb.Db.ResultTable dtBefore = null,
                                     params string[] excludeColumnsOnUpdate)
                  Update the difference of new-rows and old-rows on async


## Licence
Xb.File.Tree:  
[MIT Licence](https://github.com/ume05rw/Xb.Db/blob/master/LICENSE)

Xb.Net.SmbTree:  
[LGPL2.1](https://github.com/ume05rw/Xb.File.Tree/blob/master/LICENSE)
## Author

[Do-Be's](http://dobes.jp)
