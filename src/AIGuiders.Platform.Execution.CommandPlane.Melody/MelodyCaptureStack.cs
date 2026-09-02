using AIGuiders.Platform.IntermediateRepresentation.Melody;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Melody;

/// <summary>Capture transition after keyboard input in chord mode (GUIDERS-ADR-0024).</summary>
public enum MelodyCaptureTransitionKind
{
    EnterSubRoot,
    MatchCommand,
    AwaitMoreInput,
    NoMatch,
    Cancel,
}

/// <summary>One recursive capture frame — catalog node + consumed prefix in that node.</summary>
public sealed record MelodyCaptureFrame(
    string NodeId,
    string ConsumedPrefix,
    MelodyLineProfile Profile = MelodyLineProfile.PureByNote);

/// <summary>Recursive melody capture stack (same SM, deeper catalog nodes).</summary>
public sealed class MelodyCaptureStack
{
    readonly List<MelodyCaptureFrame> _frames = [];

    public IReadOnlyList<MelodyCaptureFrame> Frames => _frames;

    public MelodyCaptureFrame? Current => _frames.Count == 0 ? null : _frames[^1];

    public int Depth => _frames.Count;

    public void Reset(MelodyCaptureFrame root) => Reset([root]);

    public void Reset(IEnumerable<MelodyCaptureFrame> frames)
    {
        _frames.Clear();
        _frames.AddRange(frames);
    }

    public void Push(MelodyCaptureFrame frame) => _frames.Add(frame);

    public bool TryPop(out MelodyCaptureFrame? frame)
    {
        if (_frames.Count == 0)
        {
            frame = null;
            return false;
        }

        frame = _frames[^1];
        _frames.RemoveAt(_frames.Count - 1);
        return true;
    }
}
