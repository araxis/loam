# ADR-0004 — Target frameworks & Avalonia 12

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** Project owner / engineering

## Context

Installed SDKs (verified 2026-06-02): .NET 10.0.300, .NET 11.0.100-preview.4, plus a 10.0 RC.
Latest stable Avalonia is **12.0.4** (12.1 released 2026-05-06). Loam is a reusable control
library that should run on every Avalonia target: Windows/macOS/Linux desktop, iOS/Android,
and WebAssembly.

## Decision

- **Avalonia version:** 12.x (pin a specific 12.0.x via central package management; track 12.1).
  Source-verify APIs against the pinned version — do **not** rely on `master`.
- **`Loam` library TFM:** `net8.0` (broadest supported by Avalonia 12; avoids forcing consumers
  onto preview SDKs). Revisit multi-targeting `netstandard2.0` only if a real consumer needs it.
- **Sample gallery (`Loam.Gallery`) TFM:** `net8.0` desktop head first (revised from net9.0 in
  Phase 1 — net8.0 LTS, runtime present, fully runnable); add mobile/browser heads later to prove
  cross-platform.
- **Tests (`Loam.Tests`) TFM:** `net8.0`, using `Avalonia.Headless.XUnit`.
- **Build hygiene:** `Nullable=enable`, `LangVersion=latest`, `ImplicitUsings=enable`,
  `TreatWarningsAsErrors=true`, analyzers on, central package management
  (`Directory.Packages.props`), shared `Directory.Build.props`, `.editorconfig`.
- The library has **no platform-specific code**; platform concerns stay in the consuming app or
  behind small abstractions.

## Consequences

- ✅ One library binary runs everywhere Avalonia 12 runs.
- ✅ Consumers on stable .NET 8/9 SDKs can use Loam without preview tooling.
- ⚠️ Avalonia 12 is newer than the avalonia-csharp-ui-senior skill's v11 references — always check
  v12 docs/source for exact APIs.

## Follow-ups

- Pin the exact Avalonia 12.0.x patch in Phase 1 and record it in `findings/`.
