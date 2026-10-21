# Blender Integration for NAAD Asset Pipeline

This document describes how Blender is integrated into the NAAD Unity asset pipeline through multiple mechanisms: MCP server, dcc-bridge, and the custom NAAD Blender add-on.

## Overview

NAAD uses Blender as the primary 3D content creation tool. Assets created in Blender are exported to Unity Cloud Asset Manager and automatically imported into the Unity project.

## Integration Methods

### 1. MCP Server Integration (AI Agent Control)

The MCP (Model Context Protocol) server allows AI agents (like Claude Desktop or Cursor) to execute code in Blender directly.

**Configuration:**

Add to your agent's MCP configuration (e.g., `claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "blender": {
      "command": "npx",
      "args": ["-y", "blender-mcp-runner"]
    }
  }
}
```

**Usage:**
- AI agents can now execute Blender Python scripts
- Automate Blender operations through natural language
- Integrate Blender into AI-assisted workflows

**Project Config:**
The project includes a reference config at `tools/mcp-config.json`.

### 2. dcc-bridge Integration (DCC Automation)

dcc-bridge provides a unified interface to control multiple DCC applications (Blender, Maya, 3ds Max) from the command line.

**Installation:**

```bash
pip install dcc-bridge
```

**Setup:**

Run the appropriate setup script:
- Windows: `tools/dcc-bridge-setup.ps1`
- Linux/macOS: `tools/setup-dcc-bridge.sh`

Or manually configure:

```bash
# Configure Blender
dcc setup blender

# Configure Maya (optional)
dcc setup maya --version 2024

# Configure 3ds Max (optional)
dcc setup 3dsmax
```

**Usage:**

```bash
# Check running DCC instances
dcc status

# Execute code in running DCC
dcc run code "print('hello')"

# Execute a script file
dcc run file /path/to/script.py
```

### 3. NAAD Blender Add-on (Unity Cloud Asset Manager)

The custom NAAD add-on (`tools/unity-asset-pipeline/naad_addon/`) provides direct integration with Unity Cloud Asset Manager.

**Features:**
- One-click asset upload to Unity Cloud
- NAAD naming convention enforcement (`<CAT>_<Part>_<DISTRICT>[_<size>]_v<NNN>`)
- Automatic district/venue tagging
- FBX export with preview rendering
- Unity intake folder synchronization

**Installation:**

1. In Blender, go to `Edit > Preferences > Add-ons > Install...`
2. Navigate to `tools/unity-asset-pipeline/naad_addon/`
3. Select `__init__.py` and install
4. Enable the "NAAD Asset Uploader (Unity Cloud)" add-on

**Configuration:**

In Blender add-on preferences:
- Set Organization ID
- Set Asset Manager project ID
- Configure Unity intake folder path (e.g., `game/unity/Assets/Diagnostics/AssetImports`)
- Enable/disable automatic copy to Unity asset store

**Workflow:**

1. Create or select 3D objects in Blender
2. Open `3D Viewport > NAAD > Asset Pipeline > Upload`
3. Fill in asset metadata:
   - District and venue (drives tags + collection)
   - Category and size
   - Part name (auto-generated from selection)
   - Revision number
   - Description and extra tags
4. Click "Upload to Asset Manager"
5. Add-on exports FBX, renders preview, and publishes to Unity Cloud
6. If enabled, FBX is copied to Unity intake folder for automatic import

**Naming Convention:**

Assets must follow the NAAD grammar: `CAT_Part_DISTRICT[_size]_vNNN`

Examples:
- `PRP_Booth_ND_v001` (Prop, Booth, North District, version 1)
- `ENV_Building_IS_M_v002` (Environment, Building, Island State, Medium, version 2)

## Asset Pipeline Flow

```
Blender (Creation)
    ↓
NAAD Add-on (Export + Tag)
    ↓
Unity Cloud Asset Manager (Storage + Versioning)
    ↓
Unity Intake Folder (Optional sync)
    ↓
Unity Editor (Automatic import)
    ↓
Game Assets (Runtime)
```

## Troubleshooting

### MCP Server Issues
- Ensure `npx` is installed (comes with Node.js)
- Verify `blender-mcp-runner` can be downloaded
- Check agent configuration file syntax

### dcc-bridge Issues
- Ensure Python 3.8+ is installed
- Verify dcc-bridge installation: `pip show dcc-bridge`
- Check DCC application is running before using `dcc run`
- For Maya/3ds Max, ensure correct version is installed

### NAAD Add-on Issues
- Verify Unity Cloud credentials: Sign in via `NAAD > Sign in to Unity Cloud`
- Check org/project IDs in add-on preferences
- Ensure intake folder path is absolute and valid
- Validate asset name follows NAAD grammar before upload

## Related Documentation

- [Art Pipeline Documentation](../art-pipeline.md)
- [Unity Asset Pipeline](../unity-asset-pipeline.md)
- [NAAD Naming Conventions](../naming-conventions.md)
