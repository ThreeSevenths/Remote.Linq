# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased vNext][vnext-unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security


## [7.1.0][7.1.0] - 2022-11-11

### Changed
- Added optional result mapper argument to all _RemoteQueryable.Factory_ methods.
- Re-ordered method arguments to be consistent for all _RemoteQueryable.Factory_ methods.

### Removed
- Removed various types and methods previously marked as obsolete.

### Fixed
- Fixed issue with subqueries with EF Core [#112][issue#112]


## [7.0.0][7.0.0] - 2021-09-29
### Added
- Added support for [async queryable (Ix.NET)][async-queryable].
- Added support for [async streams][async-streams] ([IAsyncDisposable][iasyncdisposable]).
- Added support for [filtered include][ef-filtered-include] queryable extensions.
- Added support for [protobuf-net v2][protobuf-net-v2] serialization.
- Added support for _System.Text.Json_ serialization.
- Introduced `IExpressionTranslatorContext` interface to bundle parameterization options.
- Introduced `QueryArgumentAttribute` to annotate types to prevent local evaluation (i.e. substitution of constant expression value) when translating expressions.
- Introduced `QueryMarkerFunctionAttribute` to annotate methods to prevent local evaluation (i.e. execution of the method) when translating expressions.
### Changed
- Migrated to [nullable reference types][nullable-references].
- Moved _async_ queryable extension methods to namespace _Remote.Linq.Async_.
- Moved expression _execute_ extension methods to namespace _Remote.Linq.ExpressionExecution_.
- Moved `Include` and `ThenInclude` queryable extensions to namespace _Remote.Linq.Include_.
- Moved types `Query` and `Query<T>` to namespace _Remote.Linq.SimpleQuery_.
- Revised `RemoteQueryable.Factory` methods:
  - Renamed methods to `CreateQueryable`, `CreateAsyncQueryable`, `CreateAsyncStreamQueryable`, etc.
  - Introduced `IExpressionToRemoteLinqContext` argument for parameterization.
- Revised expression execution methods and types:
  - Introduced `IExpressionFromRemoteLinqContext` argument for parameterization.
### Fixed
- Various minor API improvements and bug fixes.

## [6.3.1][6.3.1] - 2021-08-29
### Fixed
- Fixed issue with async `IQueryable` extensions methods
  - `SequenceEqualAsync` (without comparer)
  - `SumAsync` (for Int64)

## [6.3.0][6.3.0] - 2021-01-16
### Added
- Added target framework `netstandard2.1` for _Remote.Linq.EntityFramework_.
- Added support for `ThenInclude` queryable extensions (_EF6_ and _EFCore_).
### Removed
- Dropped unused dependency on _System.Runtime.Serialization.Formatters_.


[vnext-unreleased]: https://github.com/6bee/Remote.Linq/compare/7.1.0...main
[7.1.0]: https://github.com/6bee/Remote.Linq/compare/7.0.0...7.1.0
[7.0.0]: https://github.com/6bee/Remote.Linq/compare/6.3.1...7.0.0
[6.3.1]: https://github.com/6bee/Remote.Linq/compare/6.3.0...6.3.1
[6.3.0]: https://github.com/6bee/Remote.Linq/compare/6.2.3...6.3.0

[issue#112]: https://github.com/6bee/Remote.Linq/issues/112

[async-queryable]: https://www.nuget.org/packages/System.Linq.Async.Queryable/
[async-streams]: https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/generate-consume-asynchronous-stream
[ef-filtered-include]: https://docs.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include
[iasyncdisposable]: https://docs.microsoft.com/en-us/dotnet/api/system.iasyncdisposable
[nullable-references]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[protobuf-net-v2]: https://www.nuget.org/packages/protobuf-net/2.4.6
