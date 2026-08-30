#nullable enable



using AIGuiders.Platform.Combinations.Overlay;

using AIGuiders.Platform.Configurations.Workspace;



namespace AIGuiders.Platform.Combinations.Workspace;



/// <summary>Readable overlay recipes for <see cref="WorkspaceDocument"/> (GUIDERS-ADR-0031).</summary>

public static class WorkspaceOverlay

{

    public static OverlayPolicy<WorkspaceDocument> FieldOverlay { get; } =

        OverlayProfile.For<WorkspaceDocument>("workspace.field-overlay", CombinationSemantics.FieldOverlay)

            .When(o => o.Workspace is not null, profile => profile

                .MergeSection(

                    d => d.Workspace,

                    (d, section) => new WorkspaceDocument { Workspace = section },

                    section => section

                        .FieldOverlay(

                            s => s.Adr,

                            (s, adr) => new WorkspaceSection

                            {

                                Adr = adr,

                                Features = s.Features,

                                Correspondence = s.Correspondence,

                            },

                            adr => adr

                                .Field(a => a.AutoInclude, (a, v) => a.AutoInclude = v)

                                .Field(a => a.MaxRelated, (a, v) => a.MaxRelated = v)

                                .Field(a => a.RootDir, (a, v) => a.RootDir = v)

                                .Field(a => a.Map, (a, v) => a.Map = v))

                        .ReplaceWhenPresent(

                            s => s.Features,

                            (s, features) => new WorkspaceSection

                            {

                                Adr = s.Adr,

                                Features = features,

                                Correspondence = s.Correspondence,

                            })

                        .ReplaceWhenPresent(

                            s => s.Correspondence,

                            (s, correspondence) => new WorkspaceSection

                            {

                                Adr = s.Adr,

                                Features = s.Features,

                                Correspondence = correspondence,

                            })

                        .ReplaceWhenPresent(

                            s => s.ExploreCorr,

                            (s, exploreCorr) => new WorkspaceSection

                            {

                                Adr = s.Adr,

                                Features = s.Features,

                                Correspondence = s.Correspondence,

                                ExploreCorr = exploreCorr,

                            })))

            .Build();

}


