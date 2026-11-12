"""Tests for the Higgsfield intake gate. Run: python -m unittest discover -s tools/higgsfield/tests -v"""

import hashlib
import tempfile
import unittest
from pathlib import Path

from validate_intake import STAGING_ROOT, validate_manifest


def planned_asset():
    return {
        "asset_id": "NAAD-PRP-001",
        "asset_name": "PRP_MenuBoard_ML_v001",
        "category": "PRP",
        "district": "ML",
        "venue": "SYU",
        "target_scene": "SuyaSpot",
        "status": "planned",
        "brief": "A small menu board."
    }


class PlannedManifestTests(unittest.TestCase):
    def test_starter_manifest_shape_is_valid(self):
        data = {"schema_version": "1.0.0", "assets": [planned_asset()]}
        self.assertEqual(validate_manifest(data, Path("/tmp/naad")), [])

    def test_rejects_name_category_mismatch(self):
        asset = planned_asset()
        asset["category"] = "SYU"
        errors = validate_manifest({"schema_version": "1.0.0", "assets": [asset]}, Path("/tmp/naad"))
        self.assertTrue(any("category does not match asset_name" in error for error in errors))

    def test_rejects_unregistered_scene(self):
        asset = planned_asset()
        asset["target_scene"] = "SuyaMarket"
        errors = validate_manifest({"schema_version": "1.0.0", "assets": [asset]}, Path("/tmp/naad"))
        self.assertTrue(any("scene registry" in error for error in errors))

    def test_rejects_duplicate_identity(self):
        asset = planned_asset()
        duplicate = dict(asset)
        errors = validate_manifest({"schema_version": "1.0.0", "assets": [asset, duplicate]}, Path("/tmp/naad"))
        self.assertTrue(any("duplicates" in error for error in errors))


class StagedAssetTests(unittest.TestCase):
    def test_staged_requires_file_inside_staging_root(self):
        asset = planned_asset()
        asset.update({
            "status": "staged", "provider": "Higgsfield", "generation_job_id": "job-123",
            "brief": "A small menu board.", "source_format": "glb",
            "license_status": "pending_review", "staged_file": "game/unity/Assets/mesh.glb"
        })
        errors = validate_manifest({"schema_version": "1.0.0", "assets": [asset]}, Path("/tmp/naad"))
        self.assertTrue(any("must remain under" in error for error in errors))

    def test_approved_requires_cleared_rights_and_evidence(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            staged = root / STAGING_ROOT / "menu.glb"
            staged.parent.mkdir(parents=True)
            staged.write_bytes(b"test-glb")
            asset = planned_asset()
            asset.update({
                "status": "approved_for_unity", "provider": "Higgsfield",
                "generation_job_id": "job-123", "source_format": "glb",
                "license_status": "pending_review",
                "staged_file": str((STAGING_ROOT / "menu.glb").as_posix()),
                "sha256": hashlib.sha256(b"test-glb").hexdigest(),
                "polygon_count": 1200, "validation_notes": "Mesh reviewed."
            })
            errors = validate_manifest({"schema_version": "1.0.0", "assets": [asset]}, root)
            self.assertTrue(any("license_status='cleared'" in error for error in errors))
            self.assertTrue(any("license_evidence" in error for error in errors))


if __name__ == "__main__":
    unittest.main(verbosity=2)
