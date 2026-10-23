# Blender as Python Module (`bpy`) — NAAD Setup

This folder is the practical companion to the official
“Building Blender as a Python Module” docs you pasted.
For NAIJA AFTER DARK we use `bpy` for **headless automation**:
naming validation, FBX export checks, preview renders — without
opening the Blender UI.

## Do you need to build from source?

**Almost certainly not.** Building `bpy` from source (`make bpy`
/ `bpy_module.cmake` / `make_bpy_wheel.py`) takes 30 min – several
hours, ~20 GB disk, and Visual Studio + CMake + SVN deps on Windows.

Prefer this order:

1. **Option A — `pip install bpy` (5 min, recommended).**
   The Blender Foundation publishes prebuilt `bpy` wheels on PyPI
   for matching Python versions. This is the same binary you would
   build, just prebuilt.
2. **Option B — run headless Blender (`blender --background --python`).**
   Zero install risk; uses your existing Blender install. Best when
   pip wheels don't cover your Python version.
3. **Option C — build from source** (the doc you pasted).
   Only if you need a custom Blender branch, debug build, or an
   unreleased Python combo.

## Option A — pip install (Windows, PowerShell)

```powershell
# 1. Check your Python — must match the bpy wheel you install.
python --version
# bpy 5.x (current) -> Python 3.13 ONLY (Requires: ==3.13.*)
# bpy 4.5 LTS       -> Python 3.11
# bpy 4.2 LTS       -> Python 3.11
# bpy 3.6 LTS       -> Python 3.10 (archived, use --extra-index-url, see below)

# 2. Automated setup (creates venv, installs bpy, runs smoke test):
.\tools\blender-bpy\setup-bpy.ps1 -PythonVersion 3.13

# 3. Or manually:
py -3.13 -m venv .venv-bpy
.venv-bpy\Scripts\Activate.ps1
pip install --upgrade pip
pip install bpy          # latest, e.g. bpy==5.2.*
python tools\blender-bpy\validate_bpy.py
```

> Why `pip install bpy` fails with "No matching distribution":
> PyPI only keeps the **current LTS window**. The latest `bpy`
> (5.2.x, Oct 2026) is **cp313-only** — if your `python` is 3.11/3.12,
> pip sees zero compatible files and reports "from versions: none".
> Either install Python 3.13, or pin an archived LTS from Blender's
> own index:
>
> ```powershell
> # bpy 4.2 LTS (cp311) from the Blender archive index:
> pip install "bpy==4.2.*" --extra-index-url https://download.blender.org/pypi/
> ```

> Windows trap from the docs: **never use a Debug build with a
> stock Python** — Debug needs `python311_d.dll`. The pip wheels
> are Release builds, so this trap doesn't apply to Option A.

Verify import:

```powershell
python -c "import bpy; print(bpy.app.version_string)"
# expected: 4.2.x (or whatever you installed)
```

## Option B — headless Blender (no bpy install)

If `pip install bpy` has no wheel for your Python (common on
3.12+), reuse the Blender you already have:

```powershell
# find Blender
Get-Command blender -ErrorAction SilentlyContinue
# typically: C:\Program Files\Blender Foundation\Blender 4.2\blender.exe

blender --background --python tools\blender-bpy\validate_bpy.py
```

The script in this folder is written to run **both ways**:
`python validate_bpy.py` (bpy module) and
`blender --background --python validate_bpy.py` (embedded interpreter).

## Option C — build from source (your pasted doc, Windows path)

Only if A/B can't work. Short Windows translation:

```powershell
# prerequisites: VS2022, CMake 3.20+, Git, SVN, ~20GB free
git clone https://github.com/blender/blender.git
cd blender
make update                # downloads libs
cmake -C build_files/cmake/config/bpy_module.cmake -S . -B ..\build_windows_Bpy_x64_Release -DCMAKE_BUILD_TYPE=Release
cmake --build ..\build_windows_Bpy_x64_Release --config Release -- /m
python build_files/utils/make_bpy_wheel.py ..\build_windows_Bpy_x64_Release\bin\Release --build-dir ..\build_windows_Bpy_x64_Release --output-dir ..\bpy-wheel
pip install ..\bpy-wheel\bpy-*.whl
```

Or the quick `make` wrapper (needs GNU make / cygwin on Windows,
native on Linux):

```bash
make bpy   # Linux/macOS — produces bin/bpy/
```

Then install per the doc: on Windows just copy/xcopy the built
`bin\bpy` folder into `site-packages`, or `pip install` the wheel
you made with `make_bpy_wheel.py`.

## Python-version lock (important)

From your doc's Troubleshooting — restated because it bites everyone:

> `bpy` is **locked to the CPython it was built against**.
> Blender 4.2 + Python 3.11 => `import bpy` on 3.10/3.12 **will fail**.
> Match `python --version` to Blender's Python first.

Check Blender's Python without opening it:

```powershell
blender --background --python-expr "import sys; print(sys.version)"
```

## What NAAD uses bpy for

- `validate_bpy.py` — smoke test + NAAD naming check
  (`<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>`, see
  `tools/unity-asset-pipeline/naad_addon/rules.py`).
- Headless FBX export parity with the in-Blender add-on
  (`NAAD_OT_upload` in `naad_addon/__init__.py`).
- CI-friendly asset checks without a display/GPU
  (same restriction as `blender --background`).

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `ImportError: DLL load failed` on `import bpy` | Python arch mismatch (32 vs 64-bit) or missing MSVC redist. Use 64-bit Python matching the wheel, install latest `vc_redist.x64.exe`. |
| `python311.dll not found` / `_d.dll` | You grabbed a Debug build. Reinstall Release wheel / rebuild with `-DCMAKE_BUILD_TYPE=Release`. |
| `No matching distribution for bpy` | Your Python is too new/old for published wheels. Either `py -3.11` or fall back to Option B headless. |
| `bpy.ops.render.render` fails headless | No GPU / EEVEE needs GPU on Linux; use Cycles CPU or skip render in CI. Same limit as `blender --background`. |
| Add-on `import bpy` fails in plain Python | Expected — `bpy` only exists inside Blender or with the bpy module installed. `rules.py` is intentionally bpy-free so tests run without Blender. |
