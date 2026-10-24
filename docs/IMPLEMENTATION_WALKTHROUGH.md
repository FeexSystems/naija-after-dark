# NAIJA AFTER DARK Implementation Walkthrough

This walkthrough provides a practical guide to the newly implemented features from the megaplan: dialogue branching system, Unity CLI automation, MCP integration, and documentation updates.

## Table of Contents

1. [Dialogue System Overview](#dialogue-system-overview)
2. [Unity CLI Automation](#unity-cli-automation)
3. [MCP Integration](#mcp-integration)
4. [Testing and Verification](#testing-and-verification)
5. [Common Workflows](#common-workflows)

---

## Dialogue System Overview

### What Was Implemented

The dialogue system has been upgraded from a minimal ambient-line coordinator to a full branching dialogue system with:

- **Dialogue trees** with conditional responses
- **Condition system** (reputation, seen dialogues, relationships)
- **Dialogue memory** with PlayerPrefs persistence
- **ScriptableObject authoring** for Unity Editor
- **Server-authoritative validation** through Supabase Edge Function

### Key Components

#### 1. DialogueData.cs (`game/unity/Assets/NAAD/Dialogue/DialogueData.cs`)

Contains all data structures for dialogue:

```csharp
DialogueNode         // Single dialogue node with text, speaker, emotion, responses
DialogueResponse      // Player response option with conditions and effects
DialogueCondition      // Base class for conditions
ReputationCondition    // Requires minimum reputation
SeenCondition         // Tracks seen dialogues
RelationshipCondition  // Requires minimum relationship with faction
IDialogueConditionContext // Interface for condition evaluation
```

#### 2. DialogueManager.cs (`game/unity/Assets/NAAD/Gameplay/DialogueManager.cs`)

Extended from minimal to full branching:

- `StartDialogue(string dialogueId, string npcId)` - Start a dialogue tree
- `SelectResponse(int responseIndex)` - Choose a response
- `LoadNode(string nodeId)` - Load a specific dialogue node
- Server-side command routing via SupabaseDialogueService
- Implements `IDialogueConditionContext` for condition evaluation

#### 3. DialogueBubbleManager.cs (`game/unity/Assets/NAAD/UI/DialogueBubbleManager.cs`)

Multi-bubble management system:

- Object pooling for performance
- Screen-space positioning with bounds clamping
- Auto-hide after configurable lifetime
- Supports multiple simultaneous bubbles for conversation history

#### 4. DialogueMemory.cs (`game/unity/Assets/NAAD/Dialogue/DialogueMemory.cs`)

Local memory system:

- Tracks seen dialogues per NPC
- Conversation history (last 20 conversations per NPC)
- Interaction counts
- Persists to PlayerPrefs
- Singleton pattern for global access

#### 5. DialogueGraph.cs (`game/unity/Assets/NAAD/Dialogue/DialogueGraph.cs`)

ScriptableObject for Unity Editor authoring:

- Create dialogue trees in Unity Editor
- Export/import to JSON for version control
- Runtime conversion to DialogueNode dictionary
- Menu: Create > NAAD > Dialogue Graph

#### 6. TundeNPC.cs (`game/unity/Assets/NAAD/NPCs/TundeNPC.cs`)

Updated to integrate with DialogueManager:

- `InteractWithPlayer()` now calls DialogueManager.StartDialogue()
- `EndConversation()` handles dialogue lifecycle
- Added `SetStartingDialogue(string dialogueId)` for configuration

### Using the Dialogue System

#### Step 1: Create a Dialogue Graph

1. In Unity Editor, right-click in Project window
2. Select Create > NAAD > Dialogue Graph
3. Name it (e.g., "TundeGreetingGraph")
4. In Inspector:
   - Set Graph ID (e.g., "tunde_greeting")
   - Set Graph Name (e.g., "Tunde Greeting")
   - Add DialogueNodeData entries
   - For each node, set:
     - Node ID (e.g., "node_1")
     - Speaker ID (e.g., "tunde")
     - Text content
     - Emotion (from expanded enum)
     - Responses with conditions

#### Step 2: Configure TundeNPC

1. Select TundeNPC GameObject in scene
2. In Inspector:
   - Set Starting Dialogue ID (e.g., "tunde_greeting")
   - Ensure DialogueManager component exists in scene
   - Ensure IDialogueService is configured

#### Step 3: Test Dialogue in Play Mode

1. Enter Play Mode
2. Approach TundeNPC
3. Press Interact (E key or your interaction key)
4. Dialogue bubble should appear with responses
5. Select a response
6. Dialogue should advance based on conditions

#### Step 4: Test Conditions

Create different dialogue paths with conditions:

```csharp
// ReputationCondition: requires reputation >= 50
ReputationCondition condition = new ReputationCondition(50);

// SeenCondition: only show if dialogue not seen
SeenCondition condition = new SeenCondition("seen_dialogue", false);

// RelationshipCondition: requires faction relationship >= 75
RelationshipCondition condition = new RelationshipCondition("faction_a", 75);
```

### Server-Authoritative Architecture

**Important:** The dialogue system is designed with server-authoritative validation:

1. **Client-side:** Runs branching logic locally for responsiveness
2. **Server-side:** All authoritative mutations (wallet, inventory, reputation) go through Supabase Edge Function
3. **Validation:** Edge Function validates commands before committing to PostgreSQL
4. **Fallback:** If server rejects a command, local changes are reverted

Example command flow:

```
Player selects response → DialogueManager.SelectResponse()
→ Apply local preview (reputation += 10)
→ Send command to SupabaseDialogueService
→ Edge Function validates and applies
→ If rejected: revert local changes
```

---

## Unity CLI Automation

### What Was Implemented

Six Editor methods and three batch scripts for automated Unity operations:

#### Editor Methods

1. **VerifyProject** (`Editor.VerifyProject.Execute`)
   - Verifies project structure
   - Checks required folders and scripts
   - Generates verification report

2. **VerifyScenes** (`Editor.VerifyScenes.Execute`)
   - Loads all scenes
   - Checks for required components (Camera, Light)
   - Verifies build settings

3. **VerifyUI** (`Editor.VerifyUI.Execute`)
   - Checks UI controllers
   - Verifies UI prefabs
   - Validates data contracts

4. **VerifyServices** (`Editor.VerifyServices.Execute`)
   - Checks SupabaseConfig
   - Verifies service wiring
   - Structural verification only (no connectivity tests in batch mode)

5. **BuildMethods** - Four build methods:
   - `BuildAndroid()` - Release APK
   - `BuildAndroidDevelopment()` - Dev APK with debugging
   - `BuildAndroidAAB()` - Play Store AAB
   - `BuildAndroidWithProfiling()` - Profiling build

6. **PackageMethods** - Two package methods:
   - `ListPackages()` - List all installed packages
   - `AddPackage()` - Add a package by name/Git URL
   - `ReimportTextures()` - Reimport all textures

#### Batch Scripts

Located in `game/unity/scripts/`:

1. **verify.bat** - Full verification pipeline
   - Runs all verification commands in sequence
   - Exits with error code if any check fails

2. **build-android.bat** - Android builds
   - `build-android.bat` - Release APK
   - `build-android.bat dev` - Development APK
   - `build-android.bat aab` - Play Store AAB
   - `build-android.bat profile` - Profiling build

3. **test.bat** - Test execution
   - `test.bat` - PlayMode tests
   - `test.bat edit` - EditMode tests
   - `test.bat firstnight` - First Night smoke test

### Using Unity CLI Commands

#### Manual Execution

```bash
# From game/unity directory:
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.VerifyProject.Execute -logFile "Logs\ProjectVerification.log"

# Build Android APK:
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -buildTarget Android -buildTargetPlayer Android -executeMethod NAAD.Editor.BuildMethods.BuildAndroid -logFile "Logs\AndroidBuild.log"

# List packages:
unity -quit -batchmode -projectPath "F:\naija-after-dark\game\unity" -executeMethod NAAD.Editor.PackageMethods.ListPackages -logFile "Logs\PackageList.log"
```

#### Using Batch Scripts

```bash
# From game/unity directory:
cd scripts
verify.bat
build-android.bat dev
test.bat edit
```

### CI/CD Integration

GitHub Actions workflow (`.github/workflows/naad-unity.yml`) has been updated:

- **verify-project-structure** job uses `NAAD.Editor.VerifyProject.Execute`
- **list-packages** job uses `NAAD.Editor.PackageMethods.ListPackages`
- **verify-ui** job (new) uses `NAAD.Editor.VerifyUI.Execute`
- **verify-services** job (new) uses `NAAD.Editor.VerifyServices.Execute`

**Required Secrets (configure in GitHub repository settings):**

- `UNITY_EMAIL` and `UNITY_PASSWORD` (or `UNITY_LICENSE`)
- `NAAD_TEST_EMAIL` and `NAAD_TEST_PASSWORD` (for First Night test)

---

## MCP Integration

### What Was Implemented

MCP (Model Context Protocol) integration documentation and command registry.

#### MCP Servers Configured

1. **GitHub MCP Server** (`github-mcp-server`)
   - Purpose: Issue/PR automation
   - Tools: create_pull_request, add_comment_to_pending_review, list_issues, etc.
   - Note: No `AssignCodingAgent` tool exists; use PR reviewers, labels, or assignees instead

2. **Supabase MCP Server** (`supabase-mcp-server`)
   - Purpose: Supabase project management
   - Tools: list_projects, get_project, search_projects
   - Note: Project creation may incur costs

3. **Playwright MCP** (`mcp-playwright`)
   - Purpose: Browser automation testing
   - Tools: navigate, screenshot, click, fill, evaluate

4. **Unity MCP** (`unity-mcp`)
   - Purpose: Unity Editor automation
   - Endpoint: `http://localhost:17890/mcp/rpc`
   - Plugin: `com.ivanmurzak.unity.mcp`
   - Note: Requires Unity Editor to be running

### MCP Command Registry

Located at `.devin/mcp-registry.json`:

```json
{
  "github-mcp-server": {
    "purpose": "GitHub issue/PR automation",
    "tools": ["create_pull_request", "add_comment_to_pending_review", "list_issues"],
    "enabled": true
  },
  "supabase-mcp-server": {
    "purpose": "Supabase project management",
    "tools": ["list_projects", "get_project"],
    "enabled": true
  },
  "mcp-playwright": {
    "purpose": "Browser automation testing",
    "tools": ["navigate", "screenshot", "click"],
    "enabled": true
  },
  "unity-mcp": {
    "purpose": "Unity Editor automation",
    "tools": ["json_rpc"],
    "enabled": true
  }
}
```

### Using MCP Tools

#### GitHub MCP Example

```javascript
// Create a PR with reviewers
await mcp_call_tool({
  server_name: "github-mcp-server",
  tool_name: "create_pull_request",
  arguments: {
    owner: "feexsystems",
    repo: "naija-after-dark",
    title: "Implement dialogue branching",
    body: "Full dialogue branching system as per Megaplan Track A",
    head: "feature/dialogue-branching",
    base: "main",
    reviewers: ["devin-ai-bot"]
  }
})
```

#### Unity MCP Setup

1. Open Unity Editor
2. Ensure `com.ivanmurzak.unity.mcp` package is installed (check Packages/manifest.json)
3. MCP server starts automatically on port 17890
4. Verify with: `curl http://localhost:17890/mcp/rpc`

**Note:** Unity MCP endpoint must be reachable for MCP tools to work.

### Documentation

- **MCP Integration Guide:** `docs/MCP_INTEGRATION.md`
- **Agent Integration Guide:** `docs/AGENT_INTEGRATION.md`

---

## Testing and Verification

### Dialogue System Testing

#### Unit Tests (Editor-only)

1. In Unity Editor, go to menu: NAAD > Tests > Run Dialogue Tests
2. This runs `DialogueTestRunner` which tests:
   - Dialogue data structures
   - Condition evaluation
   - Dialogue tree traversal
   - Dialogue memory system

#### Integration Testing (Play Mode)

1. Create a DialogueGraph ScriptableObject
2. Configure TundeNPC with starting dialogue ID
3. Enter Play Mode
4. Interact with TundeNPC
5. Verify:
   - Dialogue bubble appears
   - Responses are displayed based on conditions
   - Selecting responses advances dialogue
   - Memory system tracks seen dialogues

#### Testing Server-Side Validation

To test server-authoritative validation:

1. Configure SupabaseDialogueService with valid credentials
2. Start dialogue with TundeNPC
3. Select a response that changes reputation
4. Check console for server command logs
5. Verify reputation change persists after game restart

### Unity CLI Testing

#### Verification Pipeline

```bash
cd game/unity/scripts
verify.bat
```

This runs all verification commands and exits with error code if any fail.

#### Build Testing

```bash
cd game/unity/scripts
build-android.bat dev
```

This builds a development APK with debugging enabled.

### CI/CD Testing

1. Push changes to GitHub
2. GitHub Actions workflow triggers on push to main
3. Workflow runs:
   - Unity verification
   - Project structure verification
   - Animation verification
   - UI verification (new)
   - Service verification (new)
   - Package listing
4. Check Actions tab for results and logs

---

## Common Workflows

### Workflow 1: Create a New Dialogue

1. **Create DialogueGraph:**
   - Right-click in Project window > Create > NAAD > Dialogue Graph
   - Add dialogue nodes and responses
   - Set conditions if needed

2. **Export to JSON:**
   - Select DialogueGraph asset
   - Click "Export to JSON" button (custom editor or manual)
   - Commit JSON to git for version control

3. **Configure NPC:**
   - Select NPC GameObject
   - Set Starting Dialogue ID in Inspector
   - Ensure DialogueManager exists in scene

4. **Test:**
   - Enter Play Mode
   - Interact with NPC
   - Verify dialogue flow

### Workflow 2: Run Full Verification

```bash
cd F:\naija-after-dark\game\unity\scripts
verify.bat
```

Check `Logs/` directory for verification reports:
- `ProjectVerification.log`
- `SceneVerification.log`
- `EnvironmentVerification.log`
- `AnimationVerification.log`
- `NavigationVerification.log`
- `UIVerification.log`
- `ServiceVerification.log`

### Workflow 3: Build for Android

```bash
cd F:\naija-after-dark\game\unity\scripts
build-android.bat dev
```

APK will be in `Builds/Android/NAAD-Dev.apk`

### Workflow 4: Use Agent Skills

#### task-analysis Skill

For planning Unity features:

```javascript
// Invoke task-analysis for Unity feature development
await skill({
  command: "invoke",
  skill: "task-analysis",
  arguments: {
    intent: "T2C",
    technicalSolution: "docs/DIALOGUE_SPEC.md"
  }
})
```

#### game-development Skill

For automated game testing:

```bash
cd F:\naija-after-dark\game\unity
node C:\Users\ENGR\BILLI\.claude\skills\game-development\game.mjs detect
node C:\Users\ENGR\BILLI\.claude\skills\game-development\game.mjs run
node C:\Users\ENGR\BILLI\.claude\skills\game-development\game.mjs logs --errors
node C:\Users\ENGR\BILLI\.claude\skills\game-development\game.mjs shot
```

---

## Troubleshooting

### Dialogue System Issues

**Dialogue not appearing:**
- Check DialogueManager component exists in scene
- Check IDialogueService is configured
- Check console for errors
- Verify TundeNPC has starting dialogue ID set

**Conditions not working:**
- Verify condition evaluation context is implemented
- Check reputation/relationship values
- Use DialogueTestRunner to test conditions in isolation

**Memory not persisting:**
- Check PlayerPrefs are accessible
- Check DialogueMemory singleton is not destroyed
- Verify save key is correct

### Unity CLI Issues

**Command not found:**
- Verify Unity CLI is in PATH
- Use full path: `/c/Users/ENGR BILLI/AppData/Local/Unity/bin/unity`
- Check Unity version matches project (6000.6.5f1)

**Editor method not found:**
- Verify namespace matches file name
- Check file is in Editor folder
- Verify Execute() method is static

**Build fails:**
- Check Android SDK is installed
- Verify build settings are correct
- Check logs for specific error

### MCP Issues

**Unity MCP not reachable:**
- Start Unity Editor
- Check port 17890 is listening: `netstat -an | grep 17890`
- Verify MCP plugin is installed in Packages/manifest.json

**GitHub MCP authentication error:**
- Configure GitHub token in environment variables
- Check token has required permissions (repo, issues, pull_requests)

---

## Next Steps

### Immediate Actions

1. **Configure GitHub Actions secrets** (if not already done)
2. **Test dialogue system in Unity Editor**
3. **Start Unity Editor to enable Unity MCP**
4. **Review dependency vulnerability** (GitHub Dependabot alert)

### Future Enhancements

- Add visual node graph editor for DialogueGraph
- Implement dialogue editor window for easier authoring
- Add dialogue preview in Editor mode
- Integrate with backend tests for Edge Function validation
- Add automated dialogue testing in CI/CD

---

## Additional Resources

- **Project Documentation:** `AGENTS.md`, `docs/ARCHITECTURE.md`, `docs/PROJECT_PLAN.md`
- **Unity Guide:** `game/unity/AGENTS.md`
- **MCP Integration:** `docs/MCP_INTEGRATION.md`
- **Agent Integration:** `docs/AGENT_INTEGRATION.md`
- **Unity CLI Commands:** `game/unity/Assets/NAAD/UNITY_CLI_COMMANDS.md`
- **Gate Status:** `docs/GATE_STATUS.md`
