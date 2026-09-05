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
https://github.com/nguyennam1706/com.liang.tools.git#v0.2.0
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
    "com.liang.tools": "https://github.com/nguyennam1706/com.liang.tools.git#v0.2.0"
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

- **Scene View overlay** — a dropdown in the Scene View showing the active scene.
  Enable it from the Scene View overlay menu if it is hidden.
- **Alt+O** — the same dropdown wherever the focus is.
- **Alt+P** — jump back to the scene you came from.
- **Tools → Liang Tools → Scenes** — the same commands as menu items.

Holding **Alt** while clicking the overlay opens scenes additively instead of
replacing the open one. Switching a modified scene prompts to save first.

Configure it in **Project Settings → Liang Tools → Scene Switcher**:

| Setting | Effect |
| --- | --- |
| `Scene Source` | List scenes from Build Settings, the whole project, or a hand-picked set |
| `Override Play Mode Start Scene` | Always enter Play mode from a fixed scene, whatever is open |

Settings live in `ProjectSettings/LiangToolsSceneSwitcher.asset`, so the tool
adds nothing to `Assets/` and the config is shared through version control.

The implementation uses only public editor APIs — `Overlay`, `SettingsProvider`,
`EditorSceneManager.playModeStartScene`, `ShortcutManager` — so it does not break
when Unity reworks its internal toolbar. Scenes referenced by GUID survive
renames and moves; the scene list is cached and rebuilt only when the build
settings or a `.unity` asset actually change.

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
git tag v0.2.0
git push origin main --tags
```

Consumers install that exact tag with `#v0.2.0`.
