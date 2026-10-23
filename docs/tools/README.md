# Tools Documentation

This directory contains documentation for the various tools and integrations used in the NAAD project.

## Available Tools

### Blender Integration
- **[blender-integration.md](./blender-integration.md)** - Complete guide to Blender integration including MCP server, dcc-bridge, and the NAAD Blender add-on for Unity Cloud Asset Manager.

### Unity Asset Pipeline
- **[unity-asset-pipeline.md](./unity-asset-pipeline.md)** - Documentation for the Unity asset pipeline tools and workflows.

### Art Pipeline
- **[art-pipeline.md](../art-pipeline.md)** - Overall art production pipeline and asset naming conventions.

## Tool Configuration Files

### MCP Server Configuration
- `tools/mcp-config.json` - Reference MCP server configuration for Blender integration

### DCC Bridge Setup Scripts
- `tools/dcc-bridge-setup.ps1` - Windows PowerShell script for dcc-bridge setup
- `tools/setup-dcc-bridge.sh` - Linux/macOS shell script for dcc-bridge setup

## Quick Start

1. **For AI Agent Integration:**
   - Copy `tools/mcp-config.json` to your agent's configuration directory
   - Restart your AI agent (Claude Desktop, Cursor, etc.)

2. **For DCC Automation:**
   - Run `tools/dcc-bridge-setup.ps1` (Windows) or `tools/setup-dcc-bridge.sh` (Linux/macOS)
   - Open your DCC application (Blender, Maya, 3ds Max)
   - Use `dcc status` and `dcc run` commands

3. **For Unity Asset Upload:**
   - Install the NAAD Blender add-on from `tools/unity-asset-pipeline/naad_addon/`
   - Configure Unity Cloud credentials in Blender add-on preferences
   - Use the NAAD panel in Blender to upload assets

## Support

For issues or questions about these tools:
- Check the individual tool documentation
- Review the main project README
- Contact the development team
