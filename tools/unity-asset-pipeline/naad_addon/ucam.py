"""Unity Cloud Asset Manager transport for NAAD.

Lifted in shape from Unity's public Asset Manager for Blender example
(tools/unity-cloud-asset-manager-for-blender/Source/uc_asset_manager.py, GPL-2.0),
then trimmed:

* no `from unity_cloud.models import *` (namespace pollution; SDK version drift)
* no module-global `is_initialized` flag that goes stale when the add-on is
  reloaded without Blender restarting
* no auto browser redirect after upload (the artist stays in Blender)
* no hard-coded free-text commit message ("Updated model via Blender plugin")

Every SDK call lives here and nowhere else, so an SDK bump only touches this file.
"""

import pathlib
from typing import Sequence

SDK_IMPORT_ERROR = None

try:  # Blender does not bundle unity-cloud; see docs/art-pipeline.md#unity-cloud-python-sdk
    import unity_cloud as _ucam
except Exception as err:  # pragma: no cover - only exercised inside Blender
    _ucam = None
    SDK_IMPORT_ERROR = err


class AssetManagerError(RuntimeError):
    pass


def _sdk():
    if _ucam is None:
        raise AssetManagerError(
            "unity-cloud SDK is not installed in Blender's Python: %s" % (SDK_IMPORT_ERROR,)
        )
    return _ucam


# ---------------------------------------------------------------------------
# Session
# ---------------------------------------------------------------------------

def initialize():
    ucam = _sdk()
    ucam.initialize()
    ucam.identity.user_login.use()
    return True


def uninitialize():
    if _ucam is not None:
        try:
            _ucam.uninitialize()
        except Exception as err:  # already torn down by Blender shutdown
            print("[NAAD] unity_cloud uninitialize failed: %s" % err)


def is_logged_in() -> bool:
    if _ucam is None:
        return False
    state = _ucam.identity.user_login.get_authentication_state()
    return state == _ucam.identity.user_login.Authentication_State.LOGGED_IN


def login():
    ucam = _sdk()
    if not is_logged_in():
        ucam.identity.user_login.login()


def logout():
    if is_logged_in():
        _ucam.identity.user_login.logout(True)


# ---------------------------------------------------------------------------
# Listing
# ---------------------------------------------------------------------------

def get_organizations() -> list:
    return list(_sdk().identity.get_organization_list())


def get_projects(org_id: str) -> list:
    return list(_sdk().identity.get_project_list(org_id))


def get_assets(org_id: str, project_id: str) -> list:
    return list(_sdk().assets.get_asset_list(org_id, project_id))


def get_asset_versions(org_id: str, project_id: str, asset_id: str) -> list:
    return list(_sdk().assets.search_versions_in_asset(org_id, project_id, asset_id))


def get_collections(org_id: str, project_id: str) -> list:
    return list(_sdk().assets.list_collections(org_id, project_id))


# ---------------------------------------------------------------------------
# Write path: create -> update -> payload -> freeze
# ---------------------------------------------------------------------------

def create_or_new_version(org_id: str, project_id: str, asset_id, version, name: str):
    """`asset_id` None => brand-new asset; otherwise a new unfrozen version.

    A frozen version is the normal parent: the example opens a new unfrozen
    version from the selected one so history is preserved. Fail early with an
    actionable message instead of leaking an SDK exception into the UI.
    """
    ucam = _sdk()
    if asset_id is None:
        return ucam.assets.create_asset(ucam.assets.AssetCreation(name, ucam.assets.AssetType.MODEL_3D),
                                        org_id, project_id)
    if version is None:
        raise AssetManagerError("no version selected for asset %s" % asset_id)
    return ucam.assets.create_unfrozen_asset_version(org_id, project_id, asset_id, version)


def update_asset(org_id: str, project_id: str, asset_id: str, version: str, name: str,
                 description: str, tags: Sequence[str], collection: str = None):
    ucam = _sdk()
    ucam.assets.update_asset(
        ucam.assets.AssetUpdate(name, ucam.assets.AssetType.MODEL_3D, description, list(tags)),
        org_id, project_id, asset_id, version,
    )
    if collection:
        ucam.assets.link_assets_to_collection(org_id, project_id, collection, [asset_id])


def publish_payload(org_id: str, project_id: str, asset_id: str, version: str,
                    dataset_tag: str, file_name: str, path: str):
    """Replace the single file in the `dataset_tag` dataset (Source | Preview).

    Mirrors the example: one payload file per dataset (a second FBX in the same
    dataset would be ambiguous), so the dataset is emptied first. Automatic
    transformations stay disabled to match the example's verified behaviour — the
    preview PNG and the Source FBX are uploaded verbatim.
    """
    ucam = _sdk()
    datasets = ucam.assets.get_dataset_list(org_id, project_id, asset_id, version)
    dataset = next((d for d in datasets if dataset_tag in (d.system_tags or [])), None)
    if dataset is None:
        raise AssetManagerError("dataset '%s' not found on version %s" % (dataset_tag, version))

    for existing in ucam.assets.get_file_list(org_id, project_id, asset_id, version, dataset.id):
        ucam.assets.remove_file(org_id, project_id, asset_id, version, dataset.id, existing.path)

    extension = pathlib.Path(path).suffix
    upload = ucam.assets.FileUploadInformation(
        org_id, project_id, asset_id, version, dataset.id, path, file_name + extension,
    )
    ucam.assets.upload_file(upload, disable_automatic_transformations=True)


def freeze_asset(org_id: str, project_id: str, asset_id: str, version: str, message: str):
    _sdk().assets.freeze_asset_version(org_id, project_id, asset_id, version, message)


def open_asset_details(org_id: str, project_id: str, asset_id: str, version: str):
    _sdk().interop.open_browser_to_asset_details(org_id, project_id, asset_id, version)
