namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Named Hoare-style obligation: { Pre } Transform { Post } as declared in ADR facts or config contracts.
/// </summary>
public sealed record HoareObligation(
    string Id,
    string Precondition,
    string Transform,
    string Postcondition,
    string RawExpression);
