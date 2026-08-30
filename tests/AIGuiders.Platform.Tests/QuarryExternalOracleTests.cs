#nullable enable
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Tools.QuarryOracle;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class QuarryExternalOracleTests
{
    [Fact]
    public void Neovim_external_oracle_matches_platform_parser()
    {
        if (!ExternalOracleClient.TryResolveNeovim(out _, out _))
        {
            return;
        }

        Assert.True(ExternalOracleClient.TryCompareWireOnce(
            "neovim-kbd",
            NeovimNotationReader.Instance,
            "<C-k> s p",
            out _,
            out _,
            out var error), error);
    }

    [Fact]
    public void Emacs_external_oracle_matches_platform_parser()
    {
        if (!ExternalOracleClient.TryResolveEmacs(out _, out _))
        {
            return;
        }

        Assert.True(ExternalOracleClient.TryCompareWireOnce(
            "emacs-kbd",
            EmacsNotationReader.Instance,
            "C-x C-f",
            out _,
            out _,
            out var error), error);
    }
}
