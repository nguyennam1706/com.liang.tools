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

`DebugOverlay`, `FpsPage`, `LogPage` and `SystemInfoPage` — everything that
draws or runs — are wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG`. The
rest (`LiangDebug`, `IDebugPage`, `DebugUi`, `DebugSkin`, `FpsCounter`,
`TapGesture`, `DebugLogStore`) always compiles so that calling code needs no `#if` of its own; it
is inert without the overlay. Verified by compiling the runtime assembly with
both defines removed: 34.3 KB drops to 23.0 KB and the overlay types are gone.
`DebugLogStore` survives, but its `[RuntimeInitializeOnLoadMethod]` is gated too,
so nothing touches the ring buffer and it is never allocated.

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
| `DebugSkin` | Every style, built once and scaled by `Screen.dpi`. Nothing inherits `GUI.skin`. Backgrounds are generated textures: `Rounded` builds a nine-slice rounded rectangle of side `2r+1` with `border = r`, so the single middle pixel stretches while the corners stay sharp; corner coverage comes from the signed distance to the shape's edge, clamped to one pixel, which is what antialiases it |
| `InputBlocker` | Disables every active `BaseRaycaster` while the overlay is open, so uGUI finds nothing under the pointer. The type is resolved with `Type.GetType` rather than referenced, keeping ugui an optional dependency; each disabled `Behaviour` is remembered so it can be switched back on, including when the overlay is destroyed mid-session. It must not disable the `EventSystem` instead: `EventSystem.current` reads the first entry of a list the component leaves in `OnDisable`, so that turns the property null and makes `EventSystem.current.IsPointerOverGameObject()` throw in any project that calls it |
| `DebugOverlay` | `MonoBehaviour` bootstrapped by `[RuntimeInitializeOnLoadMethod]`, `DontDestroyOnLoad`, owns the FPS sampler, and reads the open gesture from `Event.current` so it is independent of the project's input backend |
| `TapGesture` | The open sequence, as a flattened list of screen corners. A tap that breaks the sequence restarts it immediately if it matches the first step, rather than forcing a wait for the timeout. No UnityEngine dependency, so it is tested directly. `DebugOverlay.TryResolveCorner` does the hit test — IMGUI coordinates start top-left, so the corner band is small y — and returns false for anything outside the two corners, which the gesture treats as "not a tap" rather than a miss |
| `FpsCounter` | Ring-buffer sampler with a running sum, so `Average` costs one add and one subtract per frame rather than a scan |
| `DebugLogStore` | Session log: a fixed 300-entry ring buffer behind a lock, since `Application.logMessageReceivedThreaded` fires off-thread. Repeated lines collapse into a counter instead of taking a slot; timestamp text is built on read and cached per second, never on the capture path; row arrays are pooled because the page re-reads every repaint; plain logs drop their stack. A snapshot of the last read keeps a tapped row index resolving to the same entry when newer lines arrive |
| `FpsPage`, `LogPage`, `SystemInfoPage` | The three built-in pages |

The builder API mirrors the shape of a screen-declares-its-own-widgets debugger:
a page describes rows and sections in `Draw` instead of wiring prefabs. IMGUI was
chosen over uGUI or UI Toolkit because it needs no prefab, scene, font or
`PanelSettings` asset — a package with zero asset dependencies installs cleanly
into any render pipeline.

## Editor Utilities

`Editor/Utilities` holds the two one-shot project actions.

| Type | Responsibility |
| --- | --- |
| `EditorActions` | The actions themselves plus `CanRecompile`, so the toolbar, the legacy toolbar and the menu validator all agree on when Recompile is available |
| `UtilitiesToolbar` | Unity 6000.3+. Two `MainToolbarButton` elements in the `Middle` dock zone at index -2 and -1, ahead of the play controls |
| `UtilitiesLegacyToolbar` | Unity below 6000.3. Two IMGUI buttons registered with `LegacyMainToolbar`, prepended to `ToolbarZonePlayMode` |
| `UtilitiesMenu` | Menu items and unbound `ShortcutManager` entries |

Unity's `Middle` dock zone contains only `Play Mode Controls`, at index 0, so
sitting to its left means a negative index. That is safe because
`defaultDockIndex` is a sort key rather than an insert position: Unity's own
`Left` zone has three elements sharing index 11 while holding five in total,
which `List.Insert` would throw on. The setter itself is a bare `stfld` with no
clamp.

Recompile is disabled rather than hidden while unavailable: Unity silently drops
a compilation request during Play mode or an in-flight compile, so a button that
looked live but did nothing would be worse than a greyed-out one.

## Adding a new tool

Runtime code goes under `Runtime/` in the `LiangTools` namespace. Editor-only code goes under `Editor/` in `LiangTools.Editor`; it may reference runtime types, never the reverse.

## Tests

Test assemblies are constrained to `UNITY_INCLUDE_TESTS`, so they compile only in projects that include the Test Framework. To run them from a consuming project, add the package to `testables` in `Packages/manifest.json`:

```json
{
  "testables": ["com.liang.tools"]
}
```
