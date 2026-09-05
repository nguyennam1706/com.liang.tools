# Liang Tools

## Overview

Liang Tools ships two assemblies:

| Assembly | Platform | Purpose |
| --- | --- | --- |
| `Liang.Tools` | All | Runtime utilities, included in player builds |
| `Liang.Tools.Editor` | Editor | Editor windows and authoring tooling |

Both are auto-referenced, so scripts in `Assembly-CSharp` can use them without editing an asmdef. Code that lives in its own assembly definition must add `Liang.Tools` to its references.

## Adding a new tool

Runtime code goes under `Runtime/` in the `LiangTools` namespace. Editor-only code goes under `Editor/` in `LiangTools.Editor`; it may reference runtime types, never the reverse.

## Tests

Test assemblies are constrained to `UNITY_INCLUDE_TESTS`, so they compile only in projects that include the Test Framework. To run them from a consuming project, add the package to `testables` in `Packages/manifest.json`:

```json
{
  "testables": ["com.liang.tools"]
}
```
