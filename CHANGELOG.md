# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.6.2] - 2026-09-17

### Fixed

- 1.6.1 disabled the active `EventSystem` while the overlay was open, which broke
  any project whose code calls `EventSystem.current`. `EventSystem.current`
  returns the first entry of a list the component removes itself from in
  `OnDisable`, so disabling it makes the property null and calls such as
  `EventSystem.current.IsPointerOverGameObject()` throw a
  `NullReferenceException` every frame. The overlay now disables the scene's
  raycasters (`BaseRaycaster`, covering `GraphicRaycaster` and the physics
  raycasters) and leaves the `EventSystem` untouched.

### Notes

- With raycasters off, `IsPointerOverGameObject()` reports false, so gameplay
  that reads input directly still sees taps on the overlay. Guard that code with
  `LiangDebug.IsOpen`.

## [1.6.1] - 2026-09-17

### Fixed

- Clicks on the open overlay also reached the game underneath. IMGUI has no
  raycast target and draws on a pass of its own, so uGUI received the same tap.
  The active `EventSystem` is now disabled while the overlay is open — located by
  reflection so ugui stays an optional dependency — and restored on close, on
  destroy, and if the overlay is torn down while still open.
- Mouse and touch events the panel's own controls did not claim are now consumed,
  so a tap on an empty part of the overlay no longer falls through to other IMGUI.

### Added

- `LiangDebug.IsOpen`, for gameplay that reads input directly instead of through
  uGUI and therefore cannot be shielded from outside. It is always `false` in a
  build compiled without the overlay.

## [1.6.0] - 2026-09-17

### Changed

- The open gesture now only accepts taps in the **two top corners** — about a
  quarter of the width and the top 15% of the height. Previously the screen was
  split down the middle, so a tap anywhere in either half counted and normal
  play could open the overlay by accident.
- A tap outside both corners is ignored rather than counted as a miss, so
  gameplay touches no longer break a half-finished sequence.
- **Breaking:** `ScreenHalf` is now `ScreenCorner`, with `Left` / `Right`
  becoming `TopLeft` / `TopRight`, and `TapStep.Half` becoming `TapStep.Corner`.
  Only code passing a custom pattern to `TapGesture` is affected.

## [1.5.1] - 2026-09-14

### Changed

- Clear PlayerPrefs and Recompile now sit immediately left of the Play / Pause /
  Step controls rather than at the far left of the toolbar. They use `Middle`
  dock index -2 and -1; `defaultDockIndex` is a sort key rather than an insert
  position, so negative values sort ahead of the play controls at index 0. Below
  6000.3 they are inserted at the start of `ToolbarZonePlayMode` instead of
  appended.
- `LegacyMainToolbar.Register` takes a `prepend` flag, so one zone can host a
  group at each end.

## [1.5.0] - 2026-09-14

### Added

- Clear PlayerPrefs and Recompile buttons on the left side of the main toolbar,
  on the far side of the Play mode controls from the other tools. On 6000.3 they
  use the `Left` dock zone; older editors attach to `ToolbarZoneLeftAlign`.
- `LegacyMainToolbar` can now host tools in more than one toolbar zone, rather
  than always adding them beside the Play mode controls.
- Clear PlayerPrefs confirms first and warns when Play mode is running, since the
  game can write keys straight back.
- Recompile calls `CompilationPipeline.RequestScriptCompilation` and disables
  itself while compiling or in Play mode, where Unity would drop the request. The
  toolbar element is rebuilt on `compilationStarted`, `compilationFinished` and
  play mode changes so the state stays current.
- Both commands appear under `Tools → Liang Tools` and are registered with the
  Shortcut Manager without a default binding.

## [1.4.0] - 2026-09-05

### Added

- Logs page: read the session's log on the device. Capture is off by default —
  no listener is registered, so a normal player's log calls cost nothing — and
  the choice is remembered so the next run captures from startup. Filter by
  type, tap a row for the full message and stack, copy the whole log out.
- `DebugLogStore` holds the last 300 lines in a fixed ring buffer behind a lock,
  since logs can arrive off the main thread. Identical consecutive lines
  collapse into a repeat count; timestamps are formatted on read and cached per
  second; plain logs drop their stack.
- Error and warning counts show on the Logs tab through a new optional
  `IDebugPage.Badge`.
- `DebugUi` gains `TextBlock`, `Copy`, `Table` (header plus clickable rows,
  returning the tapped index), a `DebugTone` overload of `Row` that colours the
  value, and `Button(label, question)` for a two-press confirm.

### Changed

- Reworked the overlay's appearance. It no longer inherits `GUI.skin`, which was
  what made it look like a built-in editor window dropped into the game: grey
  boxes, square borders and tabs that read as ordinary buttons.
  - Backgrounds are generated as antialiased nine-slice rounded rectangles, so
    corners stay crisp at any size and on any DPI.
  - A header with the title over a live subtitle (fps, frame time, page count,
    app version), and a scrim behind the panel so a bright scene does not fight
    the text.
  - Tabs are pills, the active one filled with the accent colour, and they wrap
    onto further rows instead of shrinking when a project registers many pages.
  - Section headers are flat with an accent bar down the left rather than
    buttons, so a page reads as structure instead of a stack of controls.
  - Key/value rows alternate background, the key is muted and the value is
    right-aligned; toggles are ON/OFF pills instead of checkboxes.
  - The closed-overlay FPS readout is colour-coded by frame rate, and copy
    confirmations appear as an accent toast.

### Fixed

- The overlay's host object outlived Play mode. It was created with
  `HideFlags.HideAndDontSave`, which carries `DontSaveInEditor`, and Unity does
  not clean those up when Play mode ends — so the object survived into edit mode
  and a new one was added on every run. It now uses `HideFlags.HideInHierarchy`,
  removes itself if it ever finds itself outside Play mode, and clears its
  static instance on domain reload.
- `DebugLogStore` kept its `Application.logMessageReceivedThreaded` listener
  after Play mode ended, capturing the editor's own log lines into the ring
  buffer. It now detaches and clears on `Application.quitting`, which the editor
  raises when Play mode exits.

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
