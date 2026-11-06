# Compiling NAAD C# Without the Unity Editor

**Technique (verified 2026-11-05):** you can typecheck all of `Assets/NAAD` runtime
C# in ~5 s with `dotnet build`, against Unity's own assemblies, without opening the
Editor or holding the project lock. Useful when the Unity MCP bridge is down or
another Editor instance already owns the project.

## Setup

Create a scratch project **outside the repo** (`C:\NAAD\_csharp-check`) so it never
becomes a Unity asset:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>9</LangVersion>
    <Nullable>disable</Nullable>
    <AssemblyName>NAADCheck</AssemblyName>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <NoWarn>CS0168;CS0219;CS0414;CS0649;CS0169;CS1591;CS0618</NoWarn>
    <DefineConstants>UNITY_2023_1_OR_NEWER;UNITY_6000_0_OR_NEWER;UNITY_STANDALONE</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="C:/NAAD/naija-after-dark/game/unity/Assets/NAAD/**/*.cs"
             Exclude="C:/NAAD/naija-after-dark/game/unity/Assets/NAAD/**/Editor/**/*.cs" />
  </ItemGroup>
  <ItemGroup>
    <!-- every Library/ScriptAssemblies/*.dll EXCEPT Assembly-CSharp*.dll -->
    <!-- every Editor/Data/Managed/UnityEngine/*.dll -->
    <!-- Library/PackageCache/com.unity.ext.nunit@*/net472/unity-custom/nunit.framework.dll -->
  </ItemGroup>
</Project>
```

Then `dotnet build`.

## The two things that will bite you

1. **Do not reference `Assembly-CSharp.dll` or `Assembly-CSharp-Editor.dll`.** They
   already contain the NAAD types you are compiling, so every type resolves twice.
   Exclude them and compile the sources instead.
2. **`nunit.framework.dll` must be referenced** — `Gameplay/FirstNight/FirstNightTests.cs`
   lives in `Assembly-CSharp` and uses NUnit attributes. Without it you get ~39
   `CS0246: TestAttribute` errors that look like real breakage.

Also: `Editor/` folders are excluded because they need `UnityEditor.dll` and the
`UNITY_EDITOR` define. If you need to check editor tooling, add `UnityEditor.dll`
from `Editor/Data/Managed/` and define `UNITY_EDITOR`.

## Checking Editor scripts too

For `Editor/` code, add `UnityEditor.dll` and define `UNITY_EDITOR`:

```xml
<DefineConstants>UNITY_EDITOR;UNITY_2023_1_OR_NEWER;UNITY_6000_0_OR_NEWER;UNITY_STANDALONE</DefineConstants>
<Compile Include=".../Assets/NAAD/**/*.cs" />   <!-- no Editor exclusion -->
```

**Trap:** `Editor/Data/Managed/UnityEngine/` also contains `UnityEditor.*Module.dll`
(70 of them). Referencing those *and* the monolithic `UnityEditor.dll` gives
`CS0433: The type 'MenuItem' exists in both 'UnityEditor.CoreModule' and 'UnityEditor'`.
Exclude `UnityEditor*.dll` from the module glob and keep `UnityEditor.dll`.

Keep `#if NAAD_LEGACY_EDITOR_TOOLS` files out by simply not defining that symbol —
which is also what the real build does, so the check stays honest.

## What it does and does not prove

It catches syntax errors, wrong member names, missing `using`s and type mismatches
— it found a missing `using NAAD.UI.Scripts;` in `MainHudController` immediately.

It does **not** prove the Unity-side contract: no serialization, no scene/prefab
wiring, no `[SerializeField]` population, no package-version behaviour. Load the
project in the Editor before claiming those.

Related: [Gate Status & Doc Drift](/architecture/gate-status-drift.md).
