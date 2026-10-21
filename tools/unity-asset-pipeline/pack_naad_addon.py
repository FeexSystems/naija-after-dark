"""Pack naad_addon/ into a Blender-installable ZIP.

Adapted from tools/unity-cloud-asset-manager-for-blender/Scripts/pack-addon.py
(GPL-2.0). Differences from the example:

* the archive name + add-on folder come from one constant, so the folder Blender
  expects (`naad_addon/`) and the file name cannot drift;
* an explicit file list instead of "everything in Source/" — no `__pycache__`,
  no `*.pyc`, no scratch files leaking into the zip;
* the `unity_cloud` wheel is *not* bundled. The example downloaded SDK wheels from
  a Unity-hosted URL at build time; the SDK now ships on a Unity Artifactory pip
  index, which the add-on or the artist installs separately
  (see docs/art-pipeline.md#unity-cloud-python-sdk).

Usage:
    python tools/unity-asset-pipeline/pack_naad_addon.py [-o OUTPUT_DIR]
"""

import argparse
import os
import sys
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
ADDON_FOLDER = "naad_addon"
# Explicit zip order: __init__ first, everything else alphabetically.
ADDON_FILES = ("__init__.py", "rules.py", "scene.py", "ucam.py")
EXCLUDED_SUFFIXES = (".pyc", ".pyo")


def build(output_dir=None):
    """Returns (zip_path, [members]). Raises FileNotFoundError on a missing source."""
    sys.path.insert(0, HERE)
    from paths import load_rules

    version = load_rules().VERSION
    source = os.path.join(HERE, ADDON_FOLDER)
    missing = [name for name in ADDON_FILES if not os.path.exists(os.path.join(source, name))]
    if missing:
        raise FileNotFoundError("missing add-on sources: %s" % ", ".join(missing))

    output_dir = os.path.abspath(output_dir or os.path.join(HERE, "dist"))
    os.makedirs(output_dir, exist_ok=True)
    zip_path = os.path.join(output_dir, "naad_asset_uploader-%s.zip" % version)

    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as archive:
        for file_name in ADDON_FILES:
            archive.write(os.path.join(source, file_name),
                          "%s/%s" % (ADDON_FOLDER, file_name))
        for dirpath, dirnames, filenames in os.walk(source):
            dirnames[:] = sorted(d for d in dirnames if d != "__pycache__")
            for filename in sorted(filenames):
                if filename.endswith(EXCLUDED_SUFFIXES):
                    continue
                full = os.path.join(dirpath, filename)
                rel = os.path.relpath(full, source).replace("\\", "/")
                if rel not in ADDON_FILES:
                    archive.write(full, "%s/%s" % (ADDON_FOLDER, rel))

    with zipfile.ZipFile(zip_path) as archive:
        return zip_path, sorted(archive.namelist())


def main(argv=None):
    parser = argparse.ArgumentParser(description="Package the NAAD Asset Uploader add-on")
    parser.add_argument("-o", "--output",
                        help="Folder for the ZIP (default: tools/unity-asset-pipeline/dist)")
    args = parser.parse_args(argv)

    try:
        zip_path, members = build(args.output)
    except FileNotFoundError as err:
        print("ERROR: %s" % err, file=sys.stderr)
        return 1

    print("Addon zip file created: %s" % zip_path)
    for member in members:
        print("  + %s" % member)
    return 0


if __name__ == "__main__":
    sys.exit(main())
