using System.Text.Json;

namespace AIGuiders.Platform.Routing;

/// <summary>Test/live dispatch override — inject fake JSON from DocumentEditPlane or product backend.</summary>
public delegate object DispatchCallOverride(IReadOnlyDictionary<string, JsonElement> args);
