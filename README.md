# Liang Tools

A collection of Unity editor and runtime utilities, distributed as a UPM package.

- Package name: `com.liang.tools`
- Minimum Unity version: `2022.3`

## Installation

### Package Manager (Git URL)

1. Open **Window → Package Manager**.
2. Click **+ → Install package from git URL…**
3. Paste one of:

```
https://github.com/nguyennam1706/com.liang.tools.git
```

Pin to a released version (recommended for production):

```
https://github.com/nguyennam1706/com.liang.tools.git#v1.3.0
```

The SSH remote `git@github.com:nguyennam1706/com.liang.tools.git` works too, and
is the only option if the repository ever goes private again — Unity runs `git`
with terminal prompts disabled, so an HTTPS URL to a private repo fails with
`could not read Username for 'https://github.com'`.

### manifest.json

Add the entry directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.liang.tools": "https://github.com/nguyennam1706/com.liang.tools.git#v1.3.0"
  }
}
```

Git installs require Git to be available on the machine's `PATH`. Unity resolves
a Git dependency once and locks the commit hash in `Packages/packages-lock.json`;
to move to a newer tag, change the `#tag` and let Unity re-resolve.

### Local checkout

Clone the repository anywhere and add it as a local package via **+ → Install package from disk…**, selecting the `package.json` at the repository root.

## Tools

### Scene Switcher

Opens any scene in the project without going through the Project window.

- **Main toolbar dropdown** — sits next to the Play / Pause / Step controls and
  shows the active scene, on every supported Unity version.
- **Alt+O** — the same dropdown wherever the focus is.
- **Alt+P** — jump back to the scene you came from.
- **Tools → Liang Tools → Scenes** — the same commands as menu items.

Right-clicking the toolbar dropdown gives Refresh and Settings. Switching a modified scene prompts to save first.

Configure it in **Project Settings → Liang Tools → Scene Switcher**:

| Setting | Effect |
| --- | --- |
| `Scene Source` | List scenes from Build Settings, the whole project, or a hand-picked set |
| `Override Play Mode Start Scene` | Always enter Play mode from a fixed scene, whatever is open |

Settings live in `ProjectSettings/LiangToolsSceneSwitcher.asset`, so the tool
adds nothing to `Assets/` and the config is shared through version control.

Unity only opened the main toolbar to public API in 6000.3, so the dropdown is
registered two ways:

| Editor | Mechanism |
| --- | --- |
| 6000.3 and newer | `[MainToolbarElement]`, refreshed through `MainToolbar.Refresh` |
| 2022.3 – 6000.2 | An `IMGUIContainer` added to the toolbar's `ToolbarZonePlayMode`, reached by reflection |

The reflection path checks every step and degrades to a single warning if a
future editor changes those internals; the switcher stays reachable through
`Alt+O`, the Tools menu, and an opt-in Scene View overlay. Everything else —
`SettingsProvider`, `EditorSceneManager.playModeStartScene`, `ShortcutManager` —
is public API on all versions. Scenes referenced by GUID survive
renames and moves; the scene list is cached and rebuilt only when the build
settings or a `.unity` asset actually change.

### Time Scale

A pause toggle, a slider and a reset button on the main toolbar, right of the
Scene Switcher, driving `Time.timeScale`.

The slider **snaps to 0.5 steps** over a 0 – 2 range by default, so it lands on
0, 0.5, 1, 1.5, 2 rather than an arbitrary 1.37. Range and step size are set in
**Project Settings → Liang Tools → Time Scale**; a step of 0 makes it
continuous.

- Pause stores the previous speed and restores it on resume.
- The value follows changes made by gameplay code.
- Unity resets `Time.timeScale` when Play mode starts, so the chosen scale is
  reapplied then (switchable in settings).

| Shortcut | Action |
| --- | --- |
| `Alt+T` | Reset to 1 |
| `Alt+;` | Toggle pause |
| `Alt+[` / `Alt+]` | One step slower / faster |

The same commands live under `Tools → Liang Tools → Time Scale`.

### Debug Overlay

An in-game overlay for builds and Play mode, drawn with IMGUI so the package
carries no prefabs, scenes or art. It ships two pages and takes your own.

Opening it, in Play mode:

- Tap **once on the left half, twice on the right, then three times on the
  left**, each tap within 2 seconds of the last. Taps are read from IMGUI
  events, so this works whichever input backend the project uses.
- Press `Alt+D` in the editor, or use `Tools → Liang Tools → Debug Overlay`.
- Call `LiangDebug.Toggle()` from your own code or your own input binding.
- Turn on *Show a button to reopen this overlay* on the FPS page to keep a small
  `≡` button in the top-right corner; the choice persists in `PlayerPrefs`.

The sequence is `TapGesture.DefaultPattern`; pass your own `TapStep[]` to
`new TapGesture(...)` to change it.

The overlay exists only during Play mode — it is bootstrapped by
`[RuntimeInitializeOnLoadMethod]`, so nothing shows in edit mode.

**FPS** — current, average, min and max frame rate plus frame time, a reset
button, and target frame rate / VSync / time scale with 30 · 60 · uncapped
shortcuts. A compact FPS readout can stay on screen while the overlay is
closed; that choice persists in `PlayerPrefs`.

**System** — application identity (bundle ID, version, Unity version, install
mode), device (model, OS, processor, memory, battery), graphics (API, vendor,
shader level), screen (resolution, DPI, safe area, refresh rate) and managed
heap size, with a copy button on the values worth pasting into a bug report.
Buttons for `GC.Collect` and `Resources.UnloadUnusedAssets`.

Adding a page:

```csharp
public sealed class EconomyPage : IDebugPage
{
    public string Title => "Economy";
    public int Order => 20;

    public void Draw(DebugUi ui)
    {
        if (ui.Section("Wallet"))
        {
            ui.Row("Coins", Wallet.Coins.ToString());
            ui.CopyRow("Player ID", Wallet.PlayerId);
            if (ui.Button("Add 1000")) Wallet.Add(1000);
        }
        ui.EndSection();
    }
}

LiangDebug.Register(new EconomyPage());
```

### What reaches a release build

`DebugOverlay` and the built-in pages are wrapped in
`#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG`. Without that define
a non-development build contains no overlay, no drawing code and no `OnGUI`.

**On first import the package adds `LIANG_TOOLS_DEBUG` to the project's
Scripting Define Symbols**, for Standalone, Android, iOS, WebGL, tvOS and
Windows Store — so release builds do include the debug menu. It is written once
and never re-applied, so removing the define by hand sticks.

Manage it in **Project Settings → Liang Tools → Debug Overlay**: one toggle for
all targets, or per target. Turning it off restores the stripped behaviour.

> A release build submitted to a store will carry the debug overlay while this
> define is on. Turn it off before a public release if that is not intended.

What stays compiled either way is the small façade: `LiangDebug` (whose `Open`,
`Close` and `Toggle` become empty), `IDebugPage`, `DebugUi`, `FpsCounter` and
`TapGesture`. They are inert, and keeping them means your own `IDebugPage`
implementations and `LiangDebug.Toggle()` calls still compile without you
wrapping each one in `#if`.

Measured on this package's runtime assembly: 23.5 KB with the overlay, 14.8 KB
without. `LiangDebug.IsAvailable` reports which of the two you are in.

### About window

`Tools → Liang Tools → About` reports the resolved package version.

## Samples

Import from the Package Manager window, under the package's **Samples** section.

## Layout

```
Runtime/          Liang.Tools assembly, shipped in builds
Editor/           Liang.Tools.Editor assembly, editor-only
Tests/            Test assemblies, gated behind UNITY_INCLUDE_TESTS
Documentation~/   Excluded from the asset database by the ~ suffix
Samples~/         Imported on demand via the Package Manager
```

## Releasing

1. Update `version` in `package.json` and add a `CHANGELOG.md` entry.
2. Commit, then tag and push:

```
git tag v1.3.0
git push origin main --tags
```

Consumers install that exact tag with `#v1.3.0`.
