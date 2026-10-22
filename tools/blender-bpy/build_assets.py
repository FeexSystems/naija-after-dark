"""Entry point for Blender: builds NAAD assets headlessly.

Usage (always from the repo root):

  blender --background --python tools/blender-bpy/build_assets.py -- [options]

Options:
  --out <dir>       output root (default: game/unity/Assets/NAAD/Content/Models)
  --only <prefix>   build only assets whose name starts with any comma list prefix
                    e.g. --only PRP_Bar,NCL_  (default: everything)
  --category <CAT>  same as --only with a single category code (APT, STR, SYU,
                    NCL, BCH, VHC, CHR)
  --list            print the asset registry and exit
  --jobs <n>        accepted for parity with CI; building is sequential
  --no-preview      skip the `_previews/*.png` QA renders (roughly 5x faster)
  --quiet           only print the final summary line

Each asset is built from scratch (`wm.read_factory_settings`), exported to
`<out>/<DISTRICT>/<Name>.glb` + `.fbx`, measured, and appended to
`<out>/naad_asset_manifest.json` (written after every run, filtered or not).
"""

import json
import os
import sys
import traceback

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)
import bpy  # noqa: E402  (must exist: run inside Blender)

import assets_apartment  # noqa: E402
import assets_beach  # noqa: E402
import assets_characters  # noqa: E402
import assets_nightclub  # noqa: E402
import assets_street  # noqa: E402
import assets_suya  # noqa: E402
import assets_vehicles  # noqa: E402
import naad_kit  # noqa: E402

ALL_SPECS = []
for module in (assets_apartment, assets_street, assets_suya, assets_nightclub,
               assets_beach, assets_vehicles, assets_characters):
    ALL_SPECS.extend(module.SPECS)

REGISTRY = {spec.name: spec for spec in ALL_SPECS}


def repo_root():
    return os.path.abspath(os.path.join(HERE, "..", ".."))


def parse_args(argv):
    if "--" in argv:
        args = argv[argv.index("--") + 1:]
    else:
        args = argv[1:]
    opts = {
        "out": os.path.join(repo_root(), "game", "unity", "Assets", "NAAD",
                            "Content", "Models"),
        "only": None,
        "list": False,
        "quiet": False,
        "no_preview": False,
        "jobs": 1,
    }
    i = 0
    while i < len(args):
        arg = args[i]
        if arg in ("--out", "--only", "--category", "--jobs") and i + 1 < len(args):
            value = args[i + 1]
            i += 2
            if arg == "--out":
                opts["out"] = os.path.abspath(value)
            elif arg == "--only":
                opts["only"] = [v for v in value.split(",") if v]
            elif arg == "--category":
                opts["only"] = [v + "_" for v in value.split(",") if v]
            else:
                opts["jobs"] = int(value)
            continue
        if arg == "--list":
            opts["list"] = True
        elif arg == "--no-preview":
            opts["no_preview"] = True
        elif arg == "--quiet":
            opts["quiet"] = True
        i += 1
    return opts


def selected(only):
    if not only:
        return ALL_SPECS
    picked = []
    for prefix in only:
        picked.extend([s for s in ALL_SPECS if s.name.startswith(prefix)])
    seen = set()
    unique = []
    for spec in picked:
        if spec.name not in seen:
            seen.add(spec.name)
            unique.append(spec)
    return unique


def main():
    opts = parse_args(sys.argv)
    specs = selected(opts["only"])
    if opts["list"]:
        for spec in specs:
            print("[NAAD] REGISTRY %s %s" % (spec.name, spec.specs))
        print("[NAAD] REGISTRY TOTAL %d" % len(specs))
        return 0

    if not specs:
        print("[NAAD] FAIL: selector matched no assets: %s" % opts["only"])
        return 2
    return build_all(specs, opts)


def build_all(specs, opts):
    manifest = []
    failures = []
    # The kit reads `--out`/`--no-preview` from the same argv; pass the resolved
    # paths explicitly so a filtered run can never write outside `opts["out"]`.
    kit_args = ["build"]
    for prefix in (opts["only"] or []):
        kit_args.extend(["--only", prefix])
    if opts["no_preview"]:
        kit_args.append("--no-preview")

    for spec in specs:
        out_dir = os.path.join(opts["out"], spec.district)
        try:
            report = naad_kit.build(spec, kit_args, out_dir)
            report["district"] = spec.district
            report["note"] = spec.note
            manifest.append(report)
        except Exception as err:  # keep going: one bad builder must not hide the rest
            failures.append(spec.name)
            print("[NAAD] ERROR %s: %s" % (spec.name, err))
            traceback.print_exc()

    print("[NAAD] SUMMARY built=%d failed=%d of %d selected"
          % (len(manifest), len(failures), len(specs)))
    all_assets = not opts["only"] or len(specs) == len(ALL_SPECS)
    if manifest and (all_assets or len(opts["only"]) == 1):
        # Only a complete run owns `naad_asset_manifest.json`; a filtered rerun writes
        # a `.partial` sidecar so a smoke build can never shrink the committed manifest.
        path = os.path.join(opts["out"],
                            "naad_asset_manifest.json" if all_assets
                            else "naad_asset_manifest.partial.json")
        os.makedirs(os.path.dirname(path), exist_ok=True)
        payload = {
            "generator": "tools/blender-bpy/build_assets.py",
            "blender": getattr(bpy.app, "version_string", "?"),
            "registry": len(ALL_SPECS),
            "count": len(manifest),
            "assets": sorted(manifest, key=lambda r: r["name"]),
        }
        with open(path, "w", encoding="utf-8") as handle:
            json.dump(payload, handle, indent=2)
        print("[NAAD] WROTE %s" % path)
    if failures:
        print("[NAAD] FAILED %s" % ", ".join(failures))
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
