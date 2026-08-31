#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.IntermediateRepresentation.Command;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Federation recipe: registry catalog + expanded rows + optional scope (GUIDERS-ADR-0045).</summary>
public static class CommandCatalogAssembly
{
  public static CommandCatalogIndex Build<TContext>(
      PlatformCommandRegistry<TContext> registry,
      IEnumerable<CommandDescriptor> expandedRows,
      IReadOnlyList<string>? activeScope = null,
      params ICommandSource[] additionalSources)
      where TContext : ICommandContext
  {
      Func<CommandDescriptor, bool>? scopePredicate = activeScope is { Count: > 0 }
          ? descriptor => CommandScopeFilter.Matches(descriptor, activeScope)
          : null;

      var sources = new List<ICommandSource>
      {
          RegistryCatalogBuilder.ToCommandSource(registry, predicate: scopePredicate),
      };

      var rows = expandedRows as IReadOnlyList<CommandDescriptor> ?? expandedRows.ToList();
      if (scopePredicate is not null)
      {
          rows = rows.Where(scopePredicate).ToList();
      }

      if (rows.Count > 0)
      {
          sources.Add(CommandSource.From(rows, "expanded"));
      }

      sources.AddRange(additionalSources);
      return CommandCatalogComposer.Build(sources);
  }
}
