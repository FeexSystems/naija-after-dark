# Unity MCP Defines Are Self-Managed

**Gotcha (verified 2026-11-02):** you cannot durably remove
`UNITY_MCP_READY;UNITY_MCP_DEPS_32` from `ProjectSettings.asset` by hand while the
`com.ivanmurzak.unity.mcp.*` packages are installed.

## What happens

Editing `scriptingDefineSymbols` to strip those defines from Android, iPhone, WebGL
and the console targets works on disk — the commit is fine. Then Unity reloads the
domain and the MCP plugin writes them straight back, across **every** build target.

Observed directly: after a manual strip and a recompile, `ProjectSettings.asset`
reverted to:

```
Android: UNITY_MCP_READY;UNITY_MCP_DEPS_32
iPhone:  UNITY_MCP_READY;UNITY_MCP_DEPS_32
WebGL:   UNITY_MCP_READY;UNITY_MCP_DEPS_32
...
```

## Why it matters

`com.ivanmurzak.unity.mcp` ships a **Runtime** assembly whose asmdef has
`includePlatforms: []` — no platform restriction — so it compiles into player builds,
and the defines are what gate that code. The Runtime assembly is only JSON converters
and utils (no socket server), so this is build bloat rather than an exposed port.
But it is not removable while the packages are installed.

## The only real fixes

1. Remove the MCP packages from `Packages/manifest.json` for release builds and
   re-add them locally for development. This is the only approach that holds.
2. Accept the defines and strip the MCP assemblies from the player build some other
   way (managed stripping / an assembly-level `includePlatforms` edit in an embedded
   package — note PackageCache is read-only, so this requires embedding the package).

Decided 2026-11-02: keep the packages for now, accept the defines. Revisit before the
first release build (Gate 16).

Related: [Mutation Surface](/architecture/mutation-surface.md).
