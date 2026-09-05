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
https://github.com/nguyennam1706/com.liang.tools.git#v1.2.0
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
    "com.liang.tools": "https://github.com/nguyennam1706/com.liang.tools.git#v1.2.0"
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
git tag v1.2.0
git push origin main --tags
```

Consumers install that exact tag with `#v1.2.0`.
