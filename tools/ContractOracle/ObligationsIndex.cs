#nullable enable
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AIGuiders.Platform.Tools.ContractOracle;

public sealed class ObligationsIndexDocument
{
    public int Version { get; set; }
    public List<ObligationEntry> Obligations { get; set; } = [];
}

public sealed class ObligationEntry
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Adr { get; set; } = "";
    public string? Semantics { get; set; }
    public string? Source { get; set; }
    public string? Tool { get; set; }
}

public static class ObligationsIndexLoader
{
    public static ObligationsIndexDocument Load(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<ObligationsIndexDocument>(yaml)
            ?? throw new InvalidOperationException("Obligations index YAML deserialized to null.");
    }
}
