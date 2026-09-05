# Liang Tools

## Overview

Liang Tools ships two assemblies:

| Assembly | Platform | Purpose |
| --- | --- | --- |
| `Liang.Tools` | All | Runtime utilities, included in player builds |
| `Liang.Tools.Editor` | Editor | Editor windows and authoring tooling |

Both are auto-referenced, so scripts in `Assembly-CSharp` can use them without editing an asmdef. Code that lives in its own assembly definition must add `Liang.Tools` to its references.

## Scene Switcher

`Editor/Scenes` holds the scene switching tool, split so each piece stays
testable on its own:

| Type | Responsibility |
| --- | --- |
| `SceneSwitcherSettings` | `ScriptableSingleton` persisted under `ProjectSettings/` |
| `SceneCatalog` | Builds and caches the scene list; invalidated by `EditorBuildSettings.sceneListChanged` and an `AssetPostprocessor` watching `.unity` assets |
| `SceneSwitcherService` | Opening scenes, save prompts, previous-scene tracking via `SessionState`, menu construction |
| `SceneSwitcherToolbar` | Main toolbar dropdown via `[MainToolbarElement]`, docked `Middle` at index 1 — immediately right of the Play mode controls, which occupy `Middle` index 0. Unity 6000.3+ only |
| `SceneSwitcherOverlay` | Scene View overlay fallback for Unity older than 6000.3, gated behind `#if !UNITY_6000_3_OR_NEWER` |
| `SceneSwitcherMenu` | Menu items and `ShortcutManager` bindings |
| `SceneSwitcherSettingsProvider` | Project Settings page |

Scenes are stored as GUIDs rather than paths, so renaming or moving a scene does
not break the list. Nothing here reflects into Unity internals.

## Adding a new tool

Runtime code goes under `Runtime/` in the `LiangTools` namespace. Editor-only code goes under `Editor/` in `LiangTools.Editor`; it may reference runtime types, never the reverse.

## Tests

Test assemblies are constrained to `UNITY_INCLUDE_TESTS`, so they compile only in projects that include the Test Framework. To run them from a consuming project, add the package to `testables` in `Packages/manifest.json`:

```json
{
  "testables": ["com.liang.tools"]
}
```
