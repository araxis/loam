# ADR-0001 — Project name: Loam

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** Project owner

## Context

The working folder is `D:\Projects\MudAvalonia`. The library "maps" MudBlazor onto Avalonia.
Candidate names: `MudAvalonia`, `AvaloniaMud`, `Muda`, `Loam`. The `Mud*` names borrow the
MudBlazor brand and could imply official affiliation or create trademark friction.

## Decision

The product is named **Loam** (loam = rich, fertile soil — a "mud" pun that is fully original).

- Solution: `Loam.sln`
- Root namespace / package prefix: `Loam` (`Loam`, `Loam.Icons`, optionally `Loam.Charts`).
- The repository folder remains `MudAvalonia` for now (renaming would disturb workspace memory
  paths). Renaming the folder later is cosmetic and optional.

## Consequences

- ✅ Zero trademark risk; clean, brandable NuGet IDs.
- ✅ Free to mirror MudBlazor's *API* without implying we are MudBlazor.
- ⚠️ The name does not self-explain "MudBlazor for Avalonia" — the README tagline and the
  MudBlazor→Loam mapping table must carry that message.
- A short README disclaimer will state Loam is an independent, unofficial, MudBlazor-*inspired*
  library and is not affiliated with the MudBlazor project.

## Alternatives considered

- **MudAvalonia / AvaloniaMud** — clearest intent but borrows the `Mud` brand.
- **Muda** — portmanteau; less clear, mild brand echo.
