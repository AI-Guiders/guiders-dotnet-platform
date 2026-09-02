#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Argument;
using AIGuiders.Platform.IntermediateRepresentation.Command;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Fluent authoring for <see cref="CommandDescriptor"/> (GUIDERS-ADR-0045).</summary>
public static class CommandDescriptors
{
    public static CommandDescriptorBuilder Describe(string commandId) =>
        new(commandId);
}

/// <summary>Mutable builder — call <see cref="Build"/> once.</summary>
public sealed class CommandDescriptorBuilder
{
    readonly string _commandId;
    string _domain = "";
    string _object = "";
    string _intent = "";
    string _path = "";
    IReadOnlyList<string> _pathAliases = [];
    string? _help;
    string? _group;
    string _argTail = "optional";
    ArgumentNotationProfile? _argumentNotation;
    string? _argHint;
    IReadOnlyList<CommandPickerChoice> _argPickerChoices = [];
    IReadOnlyList<ArgConstructorBinding> _argConstructors = [];
    IReadOnlyList<string> _surfaces = [];
    IReadOnlyList<string> _scope = [];
    IReadOnlyList<string> _requiredCapabilities = [];
    string? _tier;
    string? _pluginId;
    bool _requiresDestructiveConfirm;

    internal CommandDescriptorBuilder(string commandId) =>
        _commandId = commandId;

    public CommandDescriptorBuilder Domain(string domain) { _domain = domain; return this; }
    public CommandDescriptorBuilder Object(string @object) { _object = @object; return this; }
    public CommandDescriptorBuilder Intent(string intent) { _intent = intent; return this; }
    public CommandDescriptorBuilder Path(string path) { _path = path; return this; }
    public CommandDescriptorBuilder PathAliases(params string[] aliases) { _pathAliases = aliases; return this; }
    public CommandDescriptorBuilder PathAliases(IReadOnlyList<string> aliases) { _pathAliases = aliases; return this; }
    public CommandDescriptorBuilder Help(string? help) { _help = help; return this; }
    public CommandDescriptorBuilder Group(string? group) { _group = group; return this; }
    public CommandDescriptorBuilder ArgTail(string argTail) { _argTail = argTail; return this; }
    public CommandDescriptorBuilder ArgumentNotation(ArgumentNotationProfile? notation) { _argumentNotation = notation; return this; }
    public CommandDescriptorBuilder ArgHint(string? argHint) { _argHint = argHint; return this; }
    public CommandDescriptorBuilder ArgPickerChoices(IReadOnlyList<CommandPickerChoice> choices) { _argPickerChoices = choices; return this; }
    public CommandDescriptorBuilder ArgConstructors(IReadOnlyList<ArgConstructorBinding> constructors) { _argConstructors = constructors; return this; }
    public CommandDescriptorBuilder Surfaces(params string[] surfaces) { _surfaces = surfaces; return this; }
    public CommandDescriptorBuilder Surfaces(IReadOnlyList<string> surfaces) { _surfaces = surfaces; return this; }
    public CommandDescriptorBuilder Scope(params string[] scope) { _scope = scope; return this; }
    public CommandDescriptorBuilder Scope(IReadOnlyList<string> scope) { _scope = scope; return this; }
    public CommandDescriptorBuilder RequiredCapabilities(params string[] capabilities) { _requiredCapabilities = capabilities; return this; }
    public CommandDescriptorBuilder Tier(string? tier) { _tier = tier; return this; }
    public CommandDescriptorBuilder PluginId(string? pluginId) { _pluginId = pluginId; return this; }
    public CommandDescriptorBuilder RequiresDestructiveConfirm(bool value = true) { _requiresDestructiveConfirm = value; return this; }

    public CommandDescriptor Build() =>
        new()
        {
            Domain = _domain,
            Object = _object,
            Intent = _intent,
            CommandId = _commandId,
            Path = _path,
            PathAliases = _pathAliases,
            Help = _help,
            Group = _group,
            ArgTail = _argTail,
            ArgumentNotation = _argumentNotation,
            ArgHint = _argHint,
            ArgPickerChoices = _argPickerChoices,
            ArgConstructors = _argConstructors,
            Surfaces = _surfaces,
            Scope = _scope,
            RequiredCapabilities = _requiredCapabilities,
            Tier = _tier,
            PluginId = _pluginId,
            RequiresDestructiveConfirm = _requiresDestructiveConfirm,
        };
}
