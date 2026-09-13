namespace AIGuiders.Platform.Authoring.Emit;

public sealed record GdlDiagnostic(string Code, string Message, int Line = 1);
