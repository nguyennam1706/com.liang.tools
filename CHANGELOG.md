# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
