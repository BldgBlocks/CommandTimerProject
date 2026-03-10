# .NET 10.0 Upgrade Report

## Project target framework modifications

| Project name                        | Old Target Framework | New Target Framework | Commits              |
|:------------------------------------|:--------------------:|:--------------------:|:---------------------|
| CommandTimer.Core.csproj            | net8.0               | net10.0              | 638f0428             |
| CommandTimer.Desktop.csproj         | net8.0               | net10.0              | c4975540             |

## All commits

| Commit ID | Description                                                        |
|:----------|:-------------------------------------------------------------------|
| f5ba2545  | Commit upgrade plan                                                |
| 638f0428  | Update target framework to net10.0 in CommandTimer.Core.csproj     |
| ebbed86d  | Commit upgrade plan                                                |
| c4975540  | Update target framework to net10.0 in CommandTimer.Desktop.csproj  |

## Next steps

- Verify the application builds and runs end-to-end after the upgrade.
- Review any .NET 10.0 preview breaking changes that may affect Avalonia UI compatibility.
- Test cross-platform packaging via PupNet to ensure the .NET 10.0 runtime is correctly bundled.
