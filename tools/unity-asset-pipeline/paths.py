"""Paths shared by the packer and the add-on. Pure stdlib, no bpy imports."""

import os

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
UNITY_INTAKE_DIR = os.path.join(
    REPO_ROOT, "game", "unity", "Assets", "Diagnostics", "AssetImports"
)


def load_rules():
    """Import naad_addon/rules.py by path so the packer never seeds sys.path."""
    import importlib.util

    path = os.path.join(os.path.dirname(__file__), "naad_addon", "rules.py")
    spec = importlib.util.spec_from_file_location("naad_rules", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module
