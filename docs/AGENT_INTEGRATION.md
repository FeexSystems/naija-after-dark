# Agent Integration Guide for NAIJA AFTER DARK

## Overview

This document describes how to use available agent skills for NAAD project development, including task-analysis, game-development, and auto-cr skills.

## Available Agent Skills

### task-analysis Skill

**Purpose:** Intelligent task analysis and planning for development workflows

**Intent Classifications:**

1. **R2C (Requirements to Code)** - Complete development from requirements
   - Use case: New features requiring full development cycle
   - Flow: Requirements → Code analysis → Technical solution → Implementation
   - Execution: Serial

2. **T2C (Tech Solution to Code)** - Implementation from technical solution
   - Use case: Clear technical spec requiring implementation
   - Flow: Technical solution → Task breakdown → Implementation
   - Execution: Mixed (UI/API parallel, logic serial)

3. **D2C (Design to Code)** - UI implementation from design specs
   - Use case: Pure UI development from visual designs
   - Flow: Design spec → UI implementation → Visual verification
   - Execution: Serial

4. **Chat** - Q&A and consultation
   - Use case: Documentation queries, minor code changes
   - Flow: Direct interaction
   - Execution: Immediate

**Unity-Specific Adaptation:**

For NAAD Unity development, use T2C mode with Unity-specific task templates:

```yaml
intent: T2C
technicalSolution: docs/FEATURE_SPEC.md
unitySpecific:
  - Task: Create Unity ScriptableObject
    - File: Assets/NAAD/Dialogue/DialogueGraph.asset
    - Type: ScriptableObject
  - Task: Implement Editor script
    - File: Assets/NAAD/Editor/DialogueGraphEditor.cs
    - Type: Editor script
```

**Usage Example:**
```javascript
// Invoke task-analysis for Unity feature
await skill({
  command: "invoke",
  skill: "task-analysis",
  arguments: {
    intent: "T2C",
    technicalSolution: "docs/DIALOGUE_BRANCHING_SPEC.md"
  }
})
```

### game-development Skill

**Purpose:** Automated game testing and verification

**Setup:**

Ensure `.codegpt-game.json` exists in Unity project root:

```json
{
  "launch": "F:\\naija-after-dark\\game\\unity\\NAAD.exe",
  "log": "F:\\naija-after-dark\\game\\unity\\Logs\\Player.log",
  "sourceRoots": [
    "F:\\naija-after-dark\\game\\unity\\Assets\\NAAD"
  ]
}
```

**Commands:**

- `node <skill-dir>/game.mjs detect` - Detect game engine
- `node <skill-dir>/game.mjs init` - Initialize configuration
- `node <skill-dir>/game.mjs run` - Run the game
- `node <skill-dir>/game.mjs logs --errors` - Check for errors
- `node <skill-dir>/game.mjs shot` - Take screenshot

**Unity Workflow:**

1. **Detect Game:**
   ```bash
   node game.mjs detect
   ```
   Output: Detected Unity project at F:\naija-after-dark\game\unity

2. **Initialize:**
   ```bash
   node game.mjs init
   ```
   Creates/updates `.codegpt-game.json`

3. **Run and Verify:**
   ```bash
   node game.mjs run
   node game.mjs logs --errors
   node game.mjs shot
   ```

**When to Use:**
- Visual verification of dialogue UI
- Testing NPC interactions
- Checking console for runtime errors
- Screenshot capture for documentation

### auto-cr Skill

**Purpose:** Automated code review and PR management

**Capabilities:**
- Create code reviews
- Pull review comment lists
- Analyze comments with AI suggestions
- Trigger fixes and push code

**Usage Example:**
```javascript
// Invoke auto-cr for PR review
await skill({
  command: "invoke",
  skill: "auto-cr",
  arguments: {
    pullRequest: 123,
    action: "review"
  }
})
```

**Integration with GitHub MCP:**
- Use GitHub MCP to create PR
- Use auto-cr to review PR
- Use GitHub MCP to add review comments

## Agent Workflow Patterns

### Pattern 1: Feature Development Workflow

1. **Plan with task-analysis:**
   ```javascript
   skill("task-analysis", { intent: "T2C", technicalSolution: "spec.md" })
   ```

2. **Implement code:**
   - Write Unity scripts
   - Create ScriptableObjects
   - Integrate with existing systems

3. **Test with game-development:**
   ```bash
   node game.mjs run
   node game.mjs logs --errors
   node game.mjs shot
   ```

4. **Review with auto-cr:**
   ```javascript
   skill("auto-cr", { action: "review" })
   ```

5. **Create PR with GitHub MCP:**
   ```javascript
   mcp_call_tool("github-mcp-server", "create_pull_request", {...})
   ```

### Pattern 2: Bug Fix Workflow

1. **Analyze bug:**
   - Use game-development to reproduce
   - Check logs with `logs --errors`

2. **Fix code:**
   - Modify Unity scripts
   - Test fix locally

3. **Verify fix:**
   ```bash
   node game.mjs run
   node game.mjs logs --errors
   ```

4. **Push and PR:**
   - Commit changes
   - Create PR with GitHub MCP

### Pattern 3: Documentation Update Workflow

1. **Identify outdated docs:**
   - Use task-analysis in Chat mode to query

2. **Update docs:**
   - Edit documentation files
   - Verify accuracy

3. **Commit and PR:**
   - Commit documentation changes
   - Create PR with GitHub MCP

## Unity-Specific Task Templates

### Dialogue System Task Template

```yaml
intent: T2C
technicalSolution: docs/DIALOGUE_BRANCHING_SPEC.md
tasks:
  - name: Create dialogue data structures
    files:
      - Assets/NAAD/Dialogue/DialogueData.cs
    complexity: medium
  - name: Implement DialogueManager branching
    files:
      - Assets/NAAD/Gameplay/DialogueManager.cs
    complexity: high
  - name: Create DialogueBubbleManager
    files:
      - Assets/NAAD/UI/DialogueBubbleManager.cs
    complexity: medium
```

### NPC System Task Template

```yaml
intent: T2C
technicalSolution: docs/NPC_BEHAVIOR_SPEC.md
tasks:
  - name: Update TundeNPC integration
    files:
      - Assets/NAAD/NPCs/TundeNPC.cs
    complexity: medium
  - name: Implement state transitions
    files:
      - Assets/NAAD/NPCs/BehaviorStateMachine.cs
    complexity: high
```

## Best Practices

1. **Use task-analysis First:** Always start with task-analysis for planning
2. **Verify with game-development:** Never skip visual verification
3. **Automate Reviews:** Use auto-cr for consistent code review
4. **Document Everything:** Keep documentation in sync with code
5. **Test Before PR:** Always run Unity CLI tests before creating PR
6. **Use MCP for Automation:** Leverage MCP tools for repetitive tasks

## Troubleshooting

### task-analysis Not Finding Project Files

**Solution:** Ensure skill is in the correct working directory and has read access to project files.

### game-development Cannot Find Game

**Solution:** Verify `.codegpt-game.json` exists and paths are correct.

### auto-cr Cannot Access GitHub

**Solution:** Ensure GitHub token is configured and has required permissions.

## Integration with CI/CD

Agent skills can be integrated with GitHub Actions:

```yaml
- name: Run game-development verification
  run: |
    node /path/to/game.mjs run
    node /path/to/game.mjs logs --errors

- name: Auto-review PR
  uses: ./.github/actions/auto-cr
  with:
    pull_number: ${{ github.event.pull_request.number }}
```
