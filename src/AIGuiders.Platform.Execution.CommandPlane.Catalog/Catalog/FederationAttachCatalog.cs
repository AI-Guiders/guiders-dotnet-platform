#nullable disable

using AIGuiders.Platform.Modeling.CommandPlane;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog;

/// <summary>Federation attach command descriptors (plan §4.1).</summary>
public static class FederationAttachCatalog
{
    public const string AttachCommandId = "federation.attach";
    public const string VerbSuggestionId = AttachSchemaCatalog.VerbSuggestionId;

    public static CommandDescriptor RootDescriptor =>
        CommandDescriptors.Describe(AttachCommandId)
            .Domain("federation")
            .Object("relation")
            .Intent("attach")
            .Path("attach")
            .ArgTail($"picker:{VerbSuggestionId}")
            .Help("Attach typed RelationSpec edge to session graph")
            .Surfaces("slash.bar", "palette")
            .Build();

    public static IReadOnlyList<CommandDescriptor> VerbDescriptors()
    {
        var list = new List<CommandDescriptor>();

        foreach (var schema in AttachSchemaCatalog.schemas)
        {
            var wire = AttachSchemaCatalog.verbWireName(schema.Verb);
            var firstStep = schema.Steps?.Cast<AttachSchemaStep>().FirstOrDefault();
            var builder = CommandDescriptors.Describe($"{AttachCommandId}.{wire}")
                .Domain("federation")
                .Object("relation")
                .Intent($"attach-{wire}")
                .Path($"attach {wire}")
                .Help($"Attach via {wire} verb")
                .Surfaces("slash.bar", "palette");

            if (firstStep is not null)
            {
                builder = builder.ArgTail($"picker:{AttachSchemaModule.stepSuggestionId(firstStep.Id)}");
            }

            list.Add(builder.Build());
        }

        return list;
    }

    public static IReadOnlyList<CommandDescriptor> AllDescriptors()
    {
        var all = new List<CommandDescriptor> { RootDescriptor };
        all.AddRange(VerbDescriptors());
        return all;
    }
}
