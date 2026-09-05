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
| `SceneSwitcherToolbar` | Unity 6000.3+. Main toolbar dropdown via `[MainToolbarElement]`, docked `Middle` at index 1 — immediately right of the Play mode controls, which register as `Middle` index 0. Redrawn with `MainToolbar.Refresh(path)`, which re-invokes the factory method |
| `SceneSwitcherLegacyToolbar` | Unity below 6000.3. Adds an `IMGUIContainer` to the `ToolbarZonePlayMode` element found through `UnityEditor.Toolbar.m_Root`. Re-attaches after play mode changes, since the toolbar is rebuilt then |
| `SceneSwitcherOverlay` | Unity below 6000.3. Opt-in Scene View overlay, kept as a safety net if the reflection path ever fails |

Only `SceneSwitcherLegacyToolbar` touches Unity internals, and every step is
null-checked; failure logs one warning per session and leaves the shortcuts and
menu working. The injection point is the one used by Scene Switcher Pro (MIT)
and the older ToolbarExtender pattern.
| `SceneSwitcherMenu` | Menu items and `ShortcutManager` bindings |
| `SceneSwitcherSettingsProvider` | Project Settings page |

Scenes are stored as GUIDs rather than paths, so renaming or moving a scene does
not break the list. Nothing here reflects into Unity internals.

## Time Scale

`Editor/TimeScale` drives `Time.timeScale` from the main toolbar.

| Type | Responsibility |
| --- | --- |
| `TimeScaleSettings` | Range, step size and play mode behaviour, persisted under `ProjectSettings/`. `Snap` rounds to the nearest step measured from `minimum`, then clamps |
| `TimeScaleService` | Reads, snaps and clamps `Time.timeScale`; remembers the pre-pause speed in `EditorPrefs`; polls for changes made by gameplay code; reapplies the chosen scale when Play mode starts, since Unity resets it then |
| `TimeScaleToolbar` | Unity 6000.3+. Three elements at `Middle` index 2/3/4: `MainToolbarToggle` (pause), `MainToolbarSlider`, `MainToolbarButton` (reset) |
| `TimeScaleLegacyToolbar` | Unity below 6000.3. IMGUI slider registered with `LegacyMainToolbar` |
| `TimeScaleMenu` | Menu items and the `Alt+T` reset shortcut |
| `TimeScaleSettingsProvider` | Project Settings page |

`MainToolbarSlider`'s constructor takes `(content, value, min, max, onChanged,
rounded)` — the value comes *before* the range, which the private field order
does not suggest.

The three elements are separate because `MainToolbarCustom`, the only element
type accepting an arbitrary `VisualElement`, is internal to Unity. Below 6000.3
the IMGUI path draws the whole cluster in one container instead.

## Shared toolbar host

`Editor/Toolbar/LegacyMainToolbar` holds the single reflection path used below
Unity 6000.3: it finds `UnityEditor.Toolbar`, reads `m_Root`, and adds one
`IMGUIContainer` to `ToolbarZonePlayMode`. Tools register a draw callback rather
than each reaching into Unity internals themselves. It re-attaches after play
mode changes, since the toolbar is rebuilt then, and warns once per session if
any step fails.

## Debug Overlay

`Runtime/Debug` is the only part of the package that ships in a player build.

`DebugOverlay`, `FpsPage` and `SystemInfoPage` — everything that draws or runs —
are wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG`. The
rest (`LiangDebug`, `IDebugPage`, `DebugUi`, `DebugSkin`, `FpsCounter`,
`TapGesture`) always compiles so that calling code needs no `#if` of its own; it
is inert without the overlay. Verified by compiling the runtime assembly with
`UNITY_EDITOR` removed: 23.5 KB drops to 14.8 KB and the overlay types are gone.

`DebugDefineInstaller` writes `LIANG_TOOLS_DEBUG` into the project's scripting
defines the first time the package loads, so release builds keep the overlay.
`DebugDefineSettings` records that it ran, in `ProjectSettings/`, so a define
removed by hand is not written back. `DebugDefines` reads and writes the symbol
per `NamedBuildTarget`, and `DebugOverlaySettingsProvider` exposes it.

| Type | Responsibility |
| --- | --- |
| `LiangDebug` | Static entry point: page registry (sorted by `Order`), open/close, `IsAvailable` |
| `IDebugPage` | What a page implements: `Title`, `Order`, `Draw(DebugUi)` |
| `DebugUi` | Immediate-mode builder — `Section`, `Row`, `CopyRow`, `Button`, `Toggle`, `Slider`. Collapsed sections are remembered per title |
| `DebugSkin` | Styles built once, scaled by `Screen.dpi` so the overlay stays readable on a phone |
| `DebugOverlay` | `MonoBehaviour` bootstrapped by `[RuntimeInitializeOnLoadMethod]`, `DontDestroyOnLoad`, owns the FPS sampler, and reads the open gesture from `Event.current` so it is independent of the project's input backend |
| `TapGesture` | The open sequence, as a flattened list of screen halves. A tap that breaks the sequence restarts it immediately if it matches the first step, rather than forcing a wait for the timeout. No UnityEngine dependency, so it is tested directly |
| `FpsCounter` | Ring-buffer sampler with a running sum, so `Average` costs one add and one subtract per frame rather than a scan |
| `FpsPage`, `SystemInfoPage` | The two built-in pages |

The builder API mirrors the shape of a screen-declares-its-own-widgets debugger:
a page describes rows and sections in `Draw` instead of wiring prefabs. IMGUI was
chosen over uGUI or UI Toolkit because it needs no prefab, scene, font or
`PanelSettings` asset — a package with zero asset dependencies installs cleanly
into any render pipeline.

## Adding a new tool

Runtime code goes under `Runtime/` in the `LiangTools` namespace. Editor-only code goes under `Editor/` in `LiangTools.Editor`; it may reference runtime types, never the reverse.

## Tests

Test assemblies are constrained to `UNITY_INCLUDE_TESTS`, so they compile only in projects that include the Test Framework. To run them from a consuming project, add the package to `testables` in `Packages/manifest.json`:

```json
{
  "testables": ["com.liang.tools"]
}
```
