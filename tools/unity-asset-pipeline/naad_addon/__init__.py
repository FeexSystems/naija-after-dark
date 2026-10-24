"""NAAD Asset Uploader — a lean Blender add-on for Unity Cloud Asset Manager.

Derived from Unity's public "Asset Manager for Blender" example
(tools/unity-cloud-asset-manager-for-blender, GPL-2.0). The NAAD additions are:

* one command (`View3D > NAAD > Asset Pipeline > Upload`) instead of the
  example's free-text org/project/asset/version dialog — the target comes from
  add-on preferences plus the name grammar, so the artist confirms and hits OK;
* the NAAD name grammar is enforced and can be generated from the selected
  objects (`<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>`);
* district + venue cloud tags are derived from the same choices, so an asset
  cannot land in Asset Manager untagged or mistagged;
* Asset Manager collections mirror districts (`ND/APT`, `IS/NCL`, ...);
* the uploaded FBX is also written to a configured intake folder under the Unity
  project so the editor imports it on focus.

See docs/art-pipeline.md for the convention and the full flow.
"""

bl_info = {
    "name": "NAAD Asset Uploader (Unity Cloud)",
    "author": "Naija After Dark",
    "version": (0, 1, 0),
    "blender": (3, 0, 0),
    "location": "3D Viewport > NAAD",
    "description": "Export FBX, render preview, and publish NAAD assets to Unity Cloud Asset Manager",
    "category": "Import-Export",
}

import os  # noqa: E402
import shutil  # noqa: E402

import bpy  # noqa: E402

from . import rules  # noqa: E402

PREFERENCES_RDNAMES = ("addon_naad_asset_uploader", __package__)


def _prefs(context):
    try:
        return context.preferences.addons[PREFERENCES_RDNAMES[0]].preferences
    except KeyError:
        return context.preferences.addons[PREFERENCES_RDNAMES[1]].preferences


DISTRICT_ITEMS = [(code, name) for code, (_tag, name) in rules.DISTRICTS.items()]
VENUE_ITEMS = [(code, name) for code, (_tag, name, _dist) in rules.VENUES.items()]
CATEGORY_ITEMS = [(code, name) for code, name in rules.CATEGORIES.items()]
SIZE_ITEMS = [("", "None", "No size suffix")] + [("%s" % k, k, "size-%s" % rules.SIZE_TAGS[k])
                                                 for k in ("S", "M", "L", "XL")]


# ---------------------------------------------------------------------------
# Preferences
# ---------------------------------------------------------------------------

class NAADAddonPreferences(bpy.types.AddonPreferences):
    bl_idname = __package__

    org_id: bpy.props.StringProperty(name="Organization ID")
    project_id: bpy.props.StringProperty(name="Asset Manager project ID")
    intake_dir: bpy.props.StringProperty(
        name="Unity intake folder",
        subtype="DIR_PATH",
        description="Absolute path to game/unity/Assets/Diagnostics/AssetImports",
    )
    copy_to_asset_store: bpy.props.BoolProperty(
        name="Copy into Assets before upload",
        description="Also drop the FBX in the Unity intake folder so the Unity editor imports it",
        default=True,
    )

    def draw(self, context):
        layout = self.layout
        layout.prop(self, "org_id")
        layout.prop(self, "project_id")
        layout.prop(self, "copy_to_asset_store")
        layout.prop(self, "intake_dir")


# ---------------------------------------------------------------------------
# Scene state shared by the dialog
# ---------------------------------------------------------------------------

def recommended_name(settings):
    name, _problems = rules.suggest_asset_name(
        settings.category,
        settings.part_name or os.path.splitext(bpy.data.filepath and os.path.basename(bpy.data.filepath) or "")[0],
        settings.district,
        settings.version_tag,
        settings.size or None,
    )
    return name or ""


def recommended_tags(settings):
    return rules.build_tags(
        settings.category, settings.district, settings.venue, settings.size or None,
        extra=[t for t in settings.extra_tags.split() if t],
    )


class NAADUploadSettings(bpy.types.PropertyGroup):
    category: bpy.props.EnumProperty(name="Category", items=CATEGORY_ITEMS, default="PRP")
    district: bpy.props.EnumProperty(name="District", items=DISTRICT_ITEMS,
                                     default=rules.DEFAULT_DISTRICT)
    venue: bpy.props.EnumProperty(name="Venue", items=VENUE_ITEMS, default=rules.DEFAULT_VENUE)
    size: bpy.props.EnumProperty(name="Size", items=SIZE_ITEMS, default=0)
    part_name: bpy.props.StringProperty(
        name="Part",
        description="Free text part, e.g. 'Booth 03'. If empty the .blend file name is used",
    )
    asset_name: bpy.props.StringProperty(
        name="Asset name",
        description="Cloud asset name. Regenerate to follow the NAAD grammar",
    )
    version_tag: bpy.props.IntProperty(name="Revision", default=1, min=1, max=999)
    description: bpy.props.StringProperty(name="Description")
    extra_tags: bpy.props.StringProperty(
        name="Extra tags", description="Space-separated, lowercase, e.g. 'hero landmark'"
    )
    embed_textures: bpy.props.BoolProperty(name="Embed textures", default=True)
    with_preview: bpy.props.BoolProperty(name="Render preview", default=True)
    asset_id: bpy.props.StringProperty(name="Asset ID")
    asset_version: bpy.props.StringProperty(name="Version")

    def suggested_tags(self):
        return recommended_tags(self)


# ---------------------------------------------------------------------------
# Operators
# ---------------------------------------------------------------------------

class NAAD_OT_generate_name(bpy.types.Operator):
    bl_idname = "naad_asset.generate_name"
    bl_label = "Generate name from selection"
    bl_description = "Fill Part from the active/selected objects and rebuild the asset name"
    bl_options = {"REGISTER", "UNDO"}

    def execute(self, context):
        settings = context.scene.naad_upload
        objects = [o for o in context.selected_objects]
        active = context.view_layer.objects.active
        if active is not None and active not in objects:
            objects.insert(0, active)

        if objects:
            base = objects[0].name
            others = [o.name for o in objects[1:]]
            settings.part_name = base if not others else "%sSet" % base
        elif not settings.part_name:
            stem = os.path.splitext(os.path.basename(bpy.data.filepath or "Asset"))[0]
            settings.part_name = stem

        name = recommended_name(settings)
        if not name:
            self.report({"ERROR"}, "Cannot build a NAAD name from '%s'" % settings.part_name)
            return {"CANCELLED"}
        settings.asset_name = name
        return {"FINISHED"}


class NAAD_OT_validate_name(bpy.types.Operator):
    bl_idname = "naad_asset.validate_name"
    bl_label = "Validate name and tags"

    def execute(self, context):
        settings = context.scene.naad_upload
        problems = []
        if rules.split_asset_name(settings.asset_name) is None:
            problems.append("name '%s' must look like NCL_Booth_IS_v001" % settings.asset_name)
        problems.extend(rules.validate_tags(settings.suggested_tags()))
        if problems:
            for problem in problems:
                self.report({"WARNING"}, problem)
            return {"CANCELLED"}
        self.report({"INFO"}, "Name and %d tags are valid" % len(settings.suggested_tags()))
        return {"FINISHED"}


class NAAD_OT_login(bpy.types.Operator):
    bl_idname = "naad_asset.login"
    bl_label = "Sign in to Unity Cloud"
    bl_description = "Open the Unity sign-in flow and come back to Blender"

    def execute(self, context):
        from . import ucam
        try:
            ucam.login()
        except Exception as err:
            self.report({"ERROR"}, str(err))
            return {"CANCELLED"}
        self.report({"INFO"}, "Signed in to Unity Cloud")
        return {"FINISHED"}


class NAAD_OT_logout(bpy.types.Operator):
    bl_idname = "naad_asset.logout"
    bl_label = "Sign out"
    bl_description = "Forget the Unity Cloud session on this machine"

    def execute(self, context):
        from . import ucam
        ucam.logout()
        context.scene.naad_upload.asset_id = ""
        context.scene.naad_upload.asset_version = ""
        return {"FINISHED"}


class NAAD_OT_reset(bpy.types.Operator):
    bl_idname = "naad_asset.reset_target"
    bl_label = "Upload as new asset"
    bl_description = "Forget the linked Asset Manager asset; the next upload creates a new one"

    def execute(self, context):
        settings = context.scene.naad_upload
        settings.asset_id = ""
        settings.asset_version = ""
        return {"FINISHED"}


class NAAD_OT_relink_asset(bpy.types.Operator):
    bl_idname = "naad_asset.relink_asset"
    bl_label = "Link to existing asset"
    bl_description = "Choose an existing Asset Manager asset and open a new version from it"
    bl_property = "asset_id"

    items = []

    asset_id: bpy.props.EnumProperty(name="Asset", items=lambda self, context: self.items)

    def invoke(self, context, event):
        from . import ucam
        prefs = _prefs(context)
        try:
            assets = ucam.get_assets(prefs.org_id, prefs.project_id)
        except Exception as err:
            self.report({"ERROR"}, str(err))
            return {"CANCELLED"}
        live = sorted((a for a in assets if a.id == context.scene.naad_upload.asset_id or not a.is_frozen),
                      key=lambda a: a.name.lower())
        items = [(a.id, a.name, a.id) for a in live]
        if not items:
            self.report({"WARNING"}, "No assets in this Asset Manager project")
            return {"CANCELLED"}
        self.items = items
        context.window_manager.invoke_search_popup(self)
        return {"RUNNING_MODAL"}

    def execute(self, context):
        from . import ucam
        prefs = _prefs(context)
        settings = context.scene.naad_upload
        try:
            versions = ucam.get_asset_versions(prefs.org_id, prefs.project_id, self.asset_id)
        except Exception as err:
            self.report({"ERROR"}, str(err))
            return {"CANCELLED"}
        if not versions:
            self.report({"ERROR"}, "Asset has no versions to branch from")
            return {"CANCELLED"}
        latest = max(versions, key=lambda v: getattr(v, "frozen_sequence_number", 0) or 0)
        settings.asset_id = self.asset_id
        settings.asset_version = latest.version
        self.report({"INFO"}, "Linked to %s (parent version %s)" % (latest.name, latest.version))
        return {"FINISHED"}


class NAAD_OT_open_asset(bpy.types.Operator):
    bl_idname = "naad_asset.open_asset"
    bl_label = "Open asset in Asset Manager"

    def execute(self, context):
        from . import ucam
        prefs = _prefs(context)
        settings = context.scene.naad_upload
        if not settings.asset_id:
            self.report({"WARNING"}, "No asset linked yet")
            return {"CANCELLED"}
        ucam.open_asset_details(prefs.org_id, prefs.project_id, settings.asset_id,
                                settings.asset_version)
        return {"FINISHED"}


class NAAD_OT_upload(bpy.types.Operator):
    bl_idname = "naad_asset.upload"
    bl_label = "Upload to Asset Manager"
    bl_description = "Export the scene as FBX, render the preview and publish a frozen version"

    @classmethod
    def poll(cls, context):
        return context.scene is not None

    def invoke(self, context, event):
        settings = context.scene.naad_upload
        if not settings.asset_name:
            bpy.ops.naad_asset.generate_name()
        # Derive venue/district defaults the first time an artist opens the dialog.
        if not settings.venue:
            settings.venue = rules.DEFAULT_VENUE
        return context.window_manager.invoke_props_dialog(self, width=420)

    def draw(self, context):
        settings = context.scene.naad_upload
        layout = self.layout

        box = layout.box()
        box.label(text="Where (drives tags + collection)")
        row = box.row(align=True)
        row.prop(settings, "district", text="")
        row.prop(settings, "venue", text="")
        box.label(text="Collection: %s" % rules.collection_for(settings.district, settings.venue),
                  icon="FILE_FOLDER")

        box = layout.box()
        box.label(text="What")
        row = box.row(align=True)
        row.prop(settings, "category", text="")
        row.prop(settings, "size", text="")
        box.prop(settings, "part_name")
        box.prop(settings, "version_tag", text="Revision")
        row = box.row(align=True)
        row.prop(settings, "asset_name")
        row.operator("naad_asset.generate_name", text="", icon="FILE_REFRESH")
        row = box.row(align=True)
        row.operator("naad_asset.validate_name", text="Validate name + tags", icon="CHECKMARK")

        box = layout.box()
        box.label(text="Metadata")
        box.prop(settings, "description")
        box.prop(settings, "extra_tags")
        box.label(text="Tags: %s" % " ".join(settings.suggested_tags()), icon="TAG")

        box = layout.box()
        box.label(text="Publish")
        box.prop(settings, "embed_textures")
        box.prop(settings, "with_preview")
        if settings.asset_id:
            box.label(text="Updating asset %s (new version from %s)"
                      % (settings.asset_id[:8], settings.asset_version[:8]), icon="LINKED")
        else:
            box.label(text="Creating a new Asset Manager asset", icon="ADD")

        prefs = _prefs(context)
        if not prefs.org_id or not prefs.project_id:
            box.label(text="Set org + project IDs in add-on preferences", icon="ERROR")

    def execute(self, context):
        from . import scene, ucam

        settings = context.scene.naad_upload
        prefs = _prefs(context)

        name = settings.asset_name.strip()
        if rules.split_asset_name(name) is None:
            self.report({"ERROR"}, "Name '%s' does not follow the NAAD grammar "
                                   "(CAT_Part_DISTRICT_vNNN)" % name)
            return {"CANCELLED"}
        tags = settings.suggested_tags()
        problems = rules.validate_tags(tags)
        if problems:
            self.report({"ERROR"}, "; ".join(problems))
            return {"CANCELLED"}
        if not prefs.org_id or not prefs.project_id:
            self.report({"ERROR"}, "Set the Asset Manager organization and project in add-on preferences")
            return {"CANCELLED"}
        if not ucam.is_logged_in():
            self.report({"ERROR"}, "Not signed in — run NAAD > Sign in to Unity Cloud first")
            return {"CANCELLED"}

        # An asset name is a single file stem + optional extension, so a slash in
        # it would either break the FBX export or escape the temp dir.
        if any(ch in name for ch in "/\\:"):
            self.report({"ERROR"}, "Asset name '%s' must not contain /, \\ or :" % name)
            return {"CANCELLED"}

        collection = rules.collection_for(settings.district, settings.venue)
        temp_dir = None
        preview_path = None
        copy_failed = None
        try:
            temp_dir, fbx_path, preview_path = scene.export_bundle(
                name, settings.embed_textures, settings.with_preview)

            asset = ucam.create_or_new_version(
                prefs.org_id, prefs.project_id,
                settings.asset_id or None, settings.asset_version or None, name)
            ucam.update_asset(prefs.org_id, prefs.project_id, asset.id, asset.version,
                              name, settings.description, tags, collection)
            ucam.publish_payload(prefs.org_id, prefs.project_id, asset.id, asset.version,
                                 "Source", name, fbx_path)
            if preview_path and os.path.exists(preview_path):
                ucam.publish_payload(prefs.org_id, prefs.project_id, asset.id, asset.version,
                                     "Preview", "thumbnail", preview_path)
            ucam.freeze_asset(prefs.org_id, prefs.project_id, asset.id, asset.version,
                              "NAAD uploader: %s v%s from Blender" % (name, rules.VERSION))

            settings.asset_id = asset.id
            settings.asset_version = asset.version

            if prefs.copy_to_asset_store and prefs.intake_dir:
                try:
                    _copy_into_unity(prefs.intake_dir, fbx_path, name)
                except OSError as err:
                    copy_failed = "copy into Unity intake failed: %s" % err
        except Exception as err:
            self.report({"ERROR"}, "Upload failed: %s" % err)
            return {"CANCELLED"}
        finally:
            if temp_dir:
                shutil.rmtree(temp_dir, ignore_errors=True)

        if copy_failed:
            self.report({"WARNING"}, "%s published to Asset Manager, but %s" % (name, copy_failed))
        elif not preview_path:
            self.report({"WARNING"}, "%s uploaded, but no camera in scene: preview skipped" % name)
        elif prefs.copy_to_asset_store and prefs.intake_dir:
            self.report({"INFO"}, "%s published (%d tags, %s) + copied to Unity intake"
                        % (name, len(tags), collection))
        else:
            self.report({"INFO"}, "%s published (%d tags, %s)" % (name, len(tags), collection))
        return {"FINISHED"}


def _copy_into_unity(intake_dir: str, fbx_path: str, name: str):
    target = bpy.path.abspath(intake_dir)
    os.makedirs(target, exist_ok=True)
    destination = os.path.join(target, name + ".fbx")
    shutil.copy2(fbx_path, destination)
    bpy.ops.file.make_paths_relative()
    return destination


# ---------------------------------------------------------------------------
# Panel + menu
# ---------------------------------------------------------------------------

class NAAD_PT_asset_pipeline(bpy.types.Panel):
    bl_idname = "NAAD_PT_asset_pipeline"
    bl_label = "Asset Pipeline"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "NAAD"

    def draw(self, context):
        from . import ucam

        layout = self.layout
        settings = context.scene.naad_upload
        prefs = _prefs(context)

        row = layout.row(align=True)
        if ucam.is_logged_in():
            row.operator("naad_asset.logout", text="Sign out", icon="USER")
        else:
            row.operator("naad_asset.login", text="Sign in to Unity Cloud", icon="USER")
        layout.label(text="Org: %s" % (prefs.org_id[:8] or "<unset>"))
        layout.label(text="Project: %s" % (prefs.project_id[:8] or "<unset>"))

        layout.separator()
        box = layout.box()
        box.label(text=settings.asset_name or "<no name yet>", icon="MESH_DATA")
        box.label(text="%s" % rules.collection_for(settings.district, settings.venue))
        if settings.asset_id:
            box.label(text="Linked: %s" % settings.asset_id[:8], icon="LINKED")
            row = box.row(align=True)
            row.operator("naad_asset.open_asset", icon="URL")
            row.operator("naad_asset.reset_target", icon="LOOP_BACK")
        else:
            box.operator("naad_asset.relink_asset", icon="LINKED")

        layout.separator()
        layout.operator("naad_asset.upload", icon="EXPORT")


classes = (
    NAADAddonPreferences,
    NAADUploadSettings,
    NAAD_OT_generate_name,
    NAAD_OT_validate_name,
    NAAD_OT_login,
    NAAD_OT_logout,
    NAAD_OT_reset,
    NAAD_OT_relink_asset,
    NAAD_OT_open_asset,
    NAAD_OT_upload,
    NAAD_PT_asset_pipeline,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.naad_upload = bpy.props.PointerProperty(type=NAADUploadSettings)

    try:
        from . import ucam
        ucam.initialize()
    except Exception as err:
        # Missing SDK / no credentials must not block registering the add-on.
        print("[NAAD] unity_cloud not initialized: %s" % err)


def unregister():
    from . import ucam
    ucam.uninitialize()

    del bpy.types.Scene.naad_upload
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
