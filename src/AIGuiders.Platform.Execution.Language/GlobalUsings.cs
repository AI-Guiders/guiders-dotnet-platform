// GUIDERS-FSHARP-ADR-0003 §4.6 cutover: LRC seam types SSOT in Modeling.Language.
global using ILanguageBackend = AIGuiders.Platform.Modeling.Language.ILanguageBackend;
global using ProjectHint = AIGuiders.Platform.Modeling.Language.ProjectHint;
global using RenameSymbolRequest = AIGuiders.Platform.Modeling.Language.RenameSymbolRequest;
global using LanguageRequest = AIGuiders.Platform.Modeling.Language.LanguageRequest;
global using DiagnosticsResult = AIGuiders.Platform.Modeling.Language.DiagnosticsResult;
global using DocumentSymbolsResult = AIGuiders.Platform.Modeling.Language.DocumentSymbolsResult;
global using LanguageNavigation = AIGuiders.Platform.Modeling.Language.LanguageNavigation;
global using FindUsagesResult = AIGuiders.Platform.Modeling.Language.FindUsagesResult;
global using CompletionsResult = AIGuiders.Platform.Modeling.Language.CompletionsResult;
global using SymbolAtPositionResult = AIGuiders.Platform.Modeling.Language.SymbolAtPositionResult;
global using RenameSymbolResult = AIGuiders.Platform.Modeling.Language.RenameSymbolResult;
global using LanguageSymbol = AIGuiders.Platform.Modeling.Language.LanguageSymbol;
global using SourceSpan = AIGuiders.Platform.Modeling.Language.SourceSpan;
