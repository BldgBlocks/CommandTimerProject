# .NET 10.0 Upgrade Report

## Project target framework modifications

| Project name                        | Old Target Framework | New Target Framework | Commits          |
|:------------------------------------|:--------------------:|:--------------------:|:-----------------|
| CommandTimer.Core.csproj            | net8.0               | net10.0              | 638f0428         |

## All commits

| Commit ID | Description                                                        |
|:----------|:-------------------------------------------------------------------|
| f5ba2545  | Commit upgrade plan                                                |
| 638f0428  | Update target framework to net10.0 in CommandTimer.Core.csproj     |

## Next steps

- Consider upgrading `CommandTimer.Desktop.csproj` to .NET 10.0 as well to keep the solution consistent.
- Verify the application builds and runs end-to-end after the upgrade.
- Review any .NET 10.0 preview breaking changes that may affect Avalonia UI compatibility.
