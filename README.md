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
https://github.com/nguyennam1706/com.liang.tools.git#v0.1.2
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
    "com.liang.tools": "https://github.com/nguyennam1706/com.liang.tools.git#v0.1.2"
  }
}
```

Git installs require Git to be available on the machine's `PATH`. Unity resolves
a Git dependency once and locks the commit hash in `Packages/packages-lock.json`;
to move to a newer tag, change the `#tag` and let Unity re-resolve.

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
git tag v0.1.2
git push origin main --tags
```

Consumers install that exact tag with `#v0.1.2`.
