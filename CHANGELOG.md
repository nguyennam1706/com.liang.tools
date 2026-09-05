# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.1] - 2026-09-05

### Fixed

- The debug overlay could not be opened at all in 1.3.0. `OnGUI` and
  `DrawFpsOverlay` were deleted by mistake while removing the gesture progress
  dots, so nothing was drawn and no taps were read. `Alt+D` still set the open
  flag, which is why it looked like the overlay was simply not showing.

## [1.3.0] - 2026-09-05

### Added

- Debug Overlay: a runtime IMGUI overlay with an FPS page and a System page,
  opened by a left/right tap sequence — once left, twice right, three times left
  — `Alt+D` in the editor, or `LiangDebug.Toggle()`. Games register their own pages through `IDebugPage` and
  build rows with `DebugUi`.
- FPS page reporting current, average, min and max frame rate, frame time, and
  target frame rate / VSync controls, plus an optional compact readout that
  stays on screen while the overlay is closed.
- System page covering application, device, graphics, screen and managed heap,
  with copy buttons on the identifiers worth pasting into a bug report.
- An optional `≡` handle in the top-right corner for reopening the overlay,
  for cases where a corner tap is awkward.
- On first import the package adds the `LIANG_TOOLS_DEBUG` scripting define to
  Standalone, Android, iOS, WebGL, tvOS and Windows Store, so release builds
  include the debug menu. It is applied once, so removing it by hand sticks.
  **Project Settings → Liang Tools → Debug Overlay** toggles it for all targets
  or per target.
- Tests for the FPS ring buffer.

### Notes

- `DebugOverlay` and the built-in pages are wrapped in
  `#if UNITY_EDITOR || DEVELOPMENT_BUILD || LIANG_TOOLS_DEBUG`, so a release
  build contains no overlay and no `OnGUI` at all. The `LiangDebug` façade,
  `IDebugPage`, `DebugUi`, `FpsCounter` and `TapGesture` stay compiled and inert
  so calling code does not need its own `#if`. With `LIANG_TOOLS_DEBUG` set —
  the default after import — release builds include the overlay too.
- The gesture reads IMGUI events rather than an input backend, so it works under
  the legacy Input Manager, the Input System package, or both. The pattern is
  configurable through `TapGesture`.

## [1.2.0] - 2026-09-05

### Added

- Time Scale: a pause toggle, slider and reset button on the main toolbar,
  driving `Time.timeScale`. The slider snaps to 0.5 steps across a 0 – 2 range
  by default; range and step are configurable in **Project Settings → Liang
  Tools → Time Scale**, and a step of 0 gives a continuous slider.
- Pause remembers the previous speed and restores it on resume; the chosen scale
  is reapplied when Play mode starts, because Unity resets `Time.timeScale`
  there.
- Shortcuts `Alt+T` (reset), `Alt+;` (pause), `Alt+[` / `Alt+]` (one step), plus
  matching items under `Tools → Liang Tools → Time Scale`.
- Tests covering the snapping and clamping arithmetic.

### Changed

- The reflection used to reach the pre-6000.3 main toolbar moved into a shared
  `LegacyMainToolbar` host, so tools register a draw callback instead of each
  walking Unity internals.

## [1.1.0] - 2026-09-05

### Added

- The main toolbar dropdown now works below Unity 6000.3 as well, by adding an
  `IMGUIContainer` to the toolbar's `ToolbarZonePlayMode`. It re-attaches after
  play mode changes and logs a single warning if the internals it relies on ever
  move.

### Fixed

- The Unity 6000.3 dropdown now redraws through `MainToolbar.Refresh`. Assigning
  `MainToolbarElement.content` on its own did not update the toolbar.

### Changed

- The Scene View overlay is opt-in rather than shown by default; it exists only
  as a fallback when the toolbar cannot be reached.

## [1.0.0] - 2026-09-05

First release. Earlier `0.x` tags were removed.

### Added

- Package layout: `Runtime` (`Liang.Tools`), `Editor` (`Liang.Tools.Editor`),
  test assemblies gated behind `UNITY_INCLUDE_TESTS`, `Samples~` and
  `Documentation~`.
- `Tools → Liang Tools → About`, reporting the resolved package version.
- Scene Switcher: a dropdown on the main toolbar, immediately right of the
  Play / Pause / Step controls, listing every scene and showing the active one.
  Registered through the `[MainToolbarElement]` API introduced in Unity 6000.3;
  on older editors it falls back to a Scene View overlay.
- `Alt+O` opens the switcher from anywhere, `Alt+P` returns to the previous
  scene, and the same commands appear under `Tools → Liang Tools → Scenes`.
- Scene source selectable between Build Settings, the whole project, or a
  hand-picked list, configured in **Project Settings → Liang Tools → Scene
  Switcher** and stored in `ProjectSettings/LiangToolsSceneSwitcher.asset`.
- Optional Play mode start scene override, backed by
  `EditorSceneManager.playModeStartScene`.
