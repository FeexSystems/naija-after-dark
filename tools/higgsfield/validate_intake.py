#!/usr/bin/env python3
"""Validate Higgsfield exports before they enter the NAAD Unity project.

Pure Python standard library. This tool validates metadata and file provenance;
it does not certify visual quality, rig quality, legal rights, or Unity importability.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path
from typing import Any

REPO_ROOT = Path(__file__).resolve().parents[2]
DEFAULT_MANIFEST = Path(__file__).with_name("asset-intake.json")
STAGING_ROOT = Path("tools/higgsfield/staging")
ALLOWED_FORMATS = {".glb", ".fbx", ".blend", ".png", ".jpg", ".jpeg", ".wav", ".mp3", ".mp4"}
CATEGORIES = {"APT", "CHR", "INT", "NCL", "PRP", "STR", "SYU", "BCH", "VHC"}
DISTRICTS = {"ND", "ML", "IS", "CR", "CS"}
VENUES = {"APT", "SYU", "STR", "NCL", "BCH"}
SCENES = {"Bootstrap", "Apartment", "CityStreet", "SuyaSpot", "Nightclub", "Beach"}
STATUSES = {"planned", "generated", "staged", "validated", "approved_for_unity", "rejected"}
LICENSE_STATES = {"unknown", "pending_review", "cleared", "restricted"}
NAME_RE = re.compile(
    r"^(?P<category>[A-Z]{3})_(?P<part>[A-Za-z0-9]+)_(?P<district>[A-Z]{2})_v(?P<version>\d{3})$"
)
SHA_RE = re.compile(r"^[a-f0-9]{64}$")


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def validate_manifest(data: dict[str, Any], repo_root: Path = REPO_ROOT) -> list[str]:
    errors: list[str] = []
    if data.get("schema_version") != "1.0.0":
        errors.append("schema_version must be '1.0.0'")
    if not isinstance(data.get("assets"), list):
        return errors + ["assets must be an array"]

    seen_ids: set[str] = set()
    seen_names: set[str] = set()
    staging_root = repo_root / STAGING_ROOT

    for index, asset in enumerate(data["assets"]):
        prefix = f"assets[{index}]"
        if not isinstance(asset, dict):
            errors.append(f"{prefix} must be an object")
            continue

        for field in ("asset_id", "asset_name", "category", "district", "target_scene", "status"):
            if not asset.get(field):
                errors.append(f"{prefix}.{field} is required")

        asset_id = asset.get("asset_id")
        if asset_id:
            if asset_id in seen_ids:
                errors.append(f"{prefix}.asset_id duplicates {asset_id}")
            seen_ids.add(asset_id)

        name = asset.get("asset_name", "")
        if name:
            if name in seen_names:
                errors.append(f"{prefix}.asset_name duplicates {name}")
            seen_names.add(name)
            match = NAME_RE.fullmatch(name)
            if not match:
                errors.append(f"{prefix}.asset_name does not match NAAD naming grammar: {name}")
            else:
                if match.group("category") not in CATEGORIES:
                    errors.append(f"{prefix}.asset_name has unsupported category {match.group('category')}")
                if match.group("district") not in DISTRICTS:
                    errors.append(f"{prefix}.asset_name has unsupported district {match.group('district')}")
                if asset.get("category") != match.group("category"):
                    errors.append(f"{prefix}.category does not match asset_name")
                if asset.get("district") != match.group("district"):
                    errors.append(f"{prefix}.district does not match asset_name")

        if asset.get("category") not in CATEGORIES:
            errors.append(f"{prefix}.category is not a supported NAAD category")
        if asset.get("district") not in DISTRICTS:
            errors.append(f"{prefix}.district is not a supported NAAD district")
        if asset.get("venue") and asset["venue"] not in VENUES:
            errors.append(f"{prefix}.venue is not a registered venue code")
        if asset.get("target_scene") not in SCENES:
            errors.append(f"{prefix}.target_scene is not in the NAAD scene registry")

        status = asset.get("status")
        if status not in STATUSES:
            errors.append(f"{prefix}.status must be one of {', '.join(sorted(STATUSES))}")
            continue
        if status == "rejected":
            continue
        if status == "planned":
            continue

        for field in ("provider", "generation_job_id", "brief", "source_format", "license_status"):
            if not asset.get(field):
                errors.append(f"{prefix}.{field} is required from generated status onward")

        source_format = asset.get("source_format", "").lower()
        if source_format and source_format not in {ext.lstrip(".") for ext in ALLOWED_FORMATS}:
            errors.append(f"{prefix}.source_format is unsupported: {source_format}")

        license_status = asset.get("license_status")
        if license_status not in LICENSE_STATES:
            errors.append(f"{prefix}.license_status is invalid")

        if status in {"staged", "validated", "approved_for_unity"}:
            staged_file = asset.get("staged_file")
            if not staged_file:
                errors.append(f"{prefix}.staged_file is required for {status}")
                continue
            relative = Path(staged_file)
            if relative.is_absolute() or not _inside(repo_root / relative, staging_root):
                errors.append(f"{prefix}.staged_file must remain under {STAGING_ROOT.as_posix()}")
                continue
            file_path = repo_root / relative
            if not file_path.is_file():
                errors.append(f"{prefix}.staged_file does not exist: {staged_file}")
                continue
            if file_path.suffix.lower() not in ALLOWED_FORMATS:
                errors.append(f"{prefix}.staged_file has unsupported extension: {file_path.suffix}")
            if source_format and file_path.suffix.lower() != "." + source_format:
                errors.append(f"{prefix}.source_format does not match staged_file extension")

            if status in {"validated", "approved_for_unity"}:
                expected_hash = asset.get("sha256", "")
                if not SHA_RE.fullmatch(expected_hash):
                    errors.append(f"{prefix}.sha256 must be a 64-character lowercase SHA-256")
                elif _sha256(file_path) != expected_hash:
                    errors.append(f"{prefix}.sha256 does not match staged file")
                if not isinstance(asset.get("polygon_count"), int) or asset["polygon_count"] < 0:
                    errors.append(f"{prefix}.polygon_count must be a non-negative integer")
                if not asset.get("validation_notes"):
                    errors.append(f"{prefix}.validation_notes must record the technical review")

            if status == "approved_for_unity":
                if license_status != "cleared":
                    errors.append(f"{prefix}: approved_for_unity requires license_status='cleared'")
                if not asset.get("license_evidence"):
                    errors.append(f"{prefix}.license_evidence is required before Unity approval")
                if not asset.get("approved_by") or not asset.get("approved_at"):
                    errors.append(f"{prefix}.approved_by and approved_at are required before Unity approval")

    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, default=DEFAULT_MANIFEST)
    parser.add_argument("--repo-root", type=Path, default=REPO_ROOT)
    args = parser.parse_args()
    manifest_path = args.manifest if args.manifest.is_absolute() else args.repo_root / args.manifest
    try:
        data = json.loads(manifest_path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        print(f"ERROR: cannot read manifest {manifest_path}: {exc}", file=sys.stderr)
        return 2

    errors = validate_manifest(data, args.repo_root.resolve())
    if errors:
        for error in errors:
            print(f"ERROR: {error}")
        print(f"FAIL: {len(errors)} intake issue(s)")
        return 1

    print(f"PASS: {len(data['assets'])} asset record(s); no intake metadata errors")
    print("NOTE: This is a metadata/provenance gate, not a Unity import or visual-quality certification.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
