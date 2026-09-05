# Liang Tools

A collection of Unity editor and runtime utilities, distributed as a UPM package.

- Package name: `com.liang.tools`
- Minimum Unity version: `2022.3`

## Installation

### Package Manager (Git URL)

1. Open **Window → Package Manager**.
2. Click **+ → Install package from git URL…**
3. Paste the URL.

This repository is private, so use SSH. The machine needs an SSH key that has
access to the repository, loaded in `ssh-agent` or the macOS keychain — Unity
runs `git` with terminal prompts disabled and cannot ask for credentials.

```
git@github.com:nguyennam1706/com.liang.tools.git#v0.1.1
```

`ssh://git@github.com/nguyennam1706/com.liang.tools.git#v0.1.1` works as well.

An HTTPS URL only works if a Git credential helper already holds a token for
`github.com`; otherwise Unity reports
`fatal: could not read Username for 'https://github.com'`.

Omit the `#v0.1.1` suffix to track the tip of `main` instead of a release.

### manifest.json

Add the entry directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.liang.tools": "git@github.com:nguyennam1706/com.liang.tools.git#v0.1.1"
  }
}
```

Unity resolves a Git dependency once and locks the commit hash in
`Packages/packages-lock.json`; to move to a newer tag, change the `#tag` and let
Unity re-resolve.

### Local checkout

Clone the repository anywhere and add it as a local package via **+ → Install package from disk…**, selecting the `package.json` at the repository root.

## Usage

`Tools → Liang Tools → About` opens a window showing the resolved package version.

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
git tag v0.1.1
git push origin main --tags
```

Consumers install that exact tag with `#v0.1.1`.
