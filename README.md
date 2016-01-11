### SQL Server connection dialog

A simple C# dialog for editing SQL Server connection properties. It edit a `SqlConnectionStringBuilder` properties for `DataSource`, `IntegratedSecurity`, `UserID` and `Password`.

```csharp
  SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
  // Set desired properties on the connection
  scsb.IntegratedSecurity = true;
  scsb.InitialCatalog = "master";
  // Display the connection dialog
  DataConnectionDialog dlg = new DataConnectionDialog(scsb);
  if (DialogResult.OK == dlg.ShowDialog())
  {
    // Use the connection properties
    using(SqlConnection conn = new SqlConnection(dlg.ConnectionStringBuilder))
    {
      conn.Open();
      //...
    }
  }
```
###NuGet
Availble on NuGet as [com.rusanu.dataconnectiondialog](https://www.nuget.org/packages/com.rusanu.dataconnectiondialog/).
