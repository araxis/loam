# ADR-0001 — Project name: Loam

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** Project owner

## Context

The working folder is `D:\Projects\Avalonia`. The library "maps" reference onto Avalonia.
Candidate names included legacy-prefixed options and `Loam`. The legacy names borrow another
brand and could imply official affiliation or create trademark friction.

## Decision

The product is named **Loam** (loam = rich, fertile soil — an earthy pun that is fully original).

- Solution: `Loam.sln`
- Root namespace / package prefix: `Loam` (`Loam`, `Loam.Icons`, optionally `Loam.Charts`).
- The repository folder remains `Avalonia` for now (renaming would disturb workspace memory
  paths). Renaming the folder later is cosmetic and optional.

## Consequences

- ✅ Zero trademark risk; clean, brandable NuGet IDs.
- ✅ Free to mirror the reference API's *API* without implying we are reference.
- ⚠️ The name does not self-explain "reference for Avalonia" — the README tagline and the
  reference→Loam mapping table must carry that message.
- A short README disclaimer will state Loam is an independent, unofficial, reference-*inspired*
  library and is not affiliated with the reference project.

## Alternatives considered

- **Legacy-prefixed names** — clearest intent but borrow another brand.
- **Short portmanteau** — less clear, mild brand echo.
