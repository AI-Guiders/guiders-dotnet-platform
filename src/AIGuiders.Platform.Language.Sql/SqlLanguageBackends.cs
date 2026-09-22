using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Language.Sql;

static class SqlLanguageActivation
{
    internal static bool IsSqlPath(string path) =>
        AIGuiders.Platform.Execution.Language.LanguagePathRules.ResolveLanguageId(path)
        == Modeling.Language.LanguageIds.Sql;

    internal static bool MatchesDialectHint(ProjectHint hint, string dialectLanguageId, params string[] aliases)
    {
        var session = hint.SessionDefaultLanguageId?.Trim();
        if (string.IsNullOrEmpty(session))
        {
            return false;
        }

        if (session.Equals(dialectLanguageId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var alias in aliases)
        {
            if (session.Equals(alias, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool MatchesGenericHint(ProjectHint hint)
    {
        var session = hint.SessionDefaultLanguageId?.Trim();
        if (string.IsNullOrEmpty(session)
            || session.Equals(Modeling.Language.LanguageIds.Sql, StringComparison.OrdinalIgnoreCase)
            || session.Equals("generic", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return session.StartsWith("sql.", StringComparison.OrdinalIgnoreCase) is false
            && session.Equals("postgres", StringComparison.OrdinalIgnoreCase) is false
            && session.Equals("tsql", StringComparison.OrdinalIgnoreCase) is false
            && session.Equals("mssql", StringComparison.OrdinalIgnoreCase) is false;
    }
}

public abstract class SqlLanguageBackendBase(string languageId) : ILanguageBackend
{
    public string LanguageId => languageId;

    protected abstract bool CanHandleSql(string path, ProjectHint hint);

    public bool CanHandle(string path, ProjectHint hint) => CanHandleSql(path, hint);

    public Task<DiagnosticsResult> GetDiagnosticsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new DiagnosticsResult { Diagnostics = [] });

    public Task<DocumentSymbolsResult> GetDocumentSymbolsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptySymbols(req.FilePath));

    public Task<LanguageNavigation> GoToDefinitionAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptyNavigation(req.FilePath));

    public Task<FindUsagesResult> FindUsagesAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new FindUsagesResult { References = [] });

    public Task<CompletionsResult> GetCompletionsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new CompletionsResult { Items = [] });

    public Task<SymbolAtPositionResult> GetSymbolAtPositionAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptySymbol(req.FilePath));

    public Task<RenameSymbolResult> RenameSymbolAsync(RenameSymbolRequest req, CancellationToken ct) =>
        Task.FromResult(new RenameSymbolResult
        {
            OldName = "",
            NewName = req.NewName,
            SymbolKind = "",
            Applied = false,
            Message = "",
            Files = [],
            Changes = [],
        });

    static DocumentSymbolsResult EmptySymbols(string path) => new()
    {
        Root = new LanguageSymbol
        {
            Name = Path.GetFileName(path),
            Kind = "file",
            Span = EmptySpan(path),
            Container = "",
            Children = [],
        },
    };

    static LanguageNavigation EmptyNavigation(string path) => new()
    {
        Definition = EmptySpan(path),
        Declarations = [EmptySpan(path)],
    };

    static SymbolAtPositionResult EmptySymbol(string path) => new()
    {
        Kind = "",
        Name = "",
        QualifiedName = "",
        Span = EmptySpan(path),
    };

    static SourceSpan EmptySpan(string path) => new()
    {
        Path = path,
        Line = 1,
        Column = 1,
        EndLine = 1,
        EndColumn = 1,
    };
}

/// <summary>Generic SQL dialect (<c>language.sql</c>).</summary>
public sealed class SqlLanguageBackend()
    : SqlLanguageBackendBase(Modeling.Language.LanguageIds.Sql)
{
    protected override bool CanHandleSql(string path, ProjectHint hint) =>
        SqlLanguageActivation.IsSqlPath(path) && SqlLanguageActivation.MatchesGenericHint(hint);
}

/// <summary>PostgreSQL dialect (<c>language.sql.postgres</c>).</summary>
public sealed class SqlPostgresLanguageBackend()
    : SqlLanguageBackendBase(Modeling.Language.LanguageIds.SqlPostgres)
{
    protected override bool CanHandleSql(string path, ProjectHint hint) =>
        SqlLanguageActivation.IsSqlPath(path)
        && SqlLanguageActivation.MatchesDialectHint(hint, Modeling.Language.LanguageIds.SqlPostgres, "postgres");
}

/// <summary>Microsoft SQL dialect (<c>language.sql.mssql</c>).</summary>
public sealed class SqlMssqlLanguageBackend()
    : SqlLanguageBackendBase(Modeling.Language.LanguageIds.SqlMssql)
{
    protected override bool CanHandleSql(string path, ProjectHint hint) =>
        SqlLanguageActivation.IsSqlPath(path)
        && SqlLanguageActivation.MatchesDialectHint(hint, Modeling.Language.LanguageIds.SqlMssql, "mssql", "tsql");
}
