#nullable enable

namespace AIGuiders.Platform.Combinations;

/// <summary>Combines baseline with one overlay layer. Policy lives in the delegate implementation.</summary>
public delegate T Combinator<T>(T baseline, T overlay);
