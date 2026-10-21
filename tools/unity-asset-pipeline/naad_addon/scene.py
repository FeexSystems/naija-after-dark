"""Export + preview rendering inside Blender.

Kept apart from the operator code so the dialog has no scene knowledge and the
export settings live in exactly one function.
"""

import os
import tempfile
from typing import Optional

import bpy

FBX_UNIT_SCALE = 1.0
PREVIEW_SIZE = 512


def export_fbx(file_path: str, embed_textures: bool) -> str:
    """Write the current scene to `file_path`.

    `embed_textures=True` uses path_mode='COPY' + embed_textures, i.e. textures are
    copied next to the FBX and referenced relatively — the Blender FBX exporter has
    no true binary embedding for textures, which is what the AM4B example relies on.
    """
    os.makedirs(os.path.dirname(file_path), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=file_path,
        path_mode="COPY" if embed_textures else "AUTO",
        embed_textures=embed_textures,
        apply_scale_options="FBX_SCALE_ALL",
    )
    return file_path


def render_preview(file_path: str, size: int = PREVIEW_SIZE) -> Optional[str]:
    """Render the scene camera to PNG at `file_path`.

    Returns None (instead of raising) when the scene has no camera, so the caller
    can still upload the model and report a warning — the example aborted the whole
    upload with `RuntimeError: Cannot render, no camera`.
    """
    scene = bpy.context.scene
    if scene.camera is None:
        return None
    previous = (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.image_settings.file_format,
        scene.render.filepath,
    )
    try:
        scene.render.resolution_x = size
        scene.render.resolution_y = size
        scene.render.resolution_percentage = 100
        scene.render.image_settings.file_format = "PNG"
        scene.render.filepath = file_path
        bpy.ops.render.render(write_still=True)
    finally:
        (scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage,
         scene.render.image_settings.file_format, scene.render.filepath) = previous
    return file_path


def export_bundle(asset_name: str, embed_textures: bool, with_preview: bool):
    """Export to a fresh temp dir. Caller owns cleanup; returns (dir, fbx, preview|None)."""
    temp_dir = tempfile.mkdtemp(prefix="naad_asset_")
    fbx_path = export_fbx(os.path.join(temp_dir, asset_name + ".fbx"), embed_textures)
    preview_path = None
    if with_preview:
        preview_path = render_preview(os.path.join(temp_dir, "thumbnail.png"))
    return temp_dir, fbx_path, preview_path
