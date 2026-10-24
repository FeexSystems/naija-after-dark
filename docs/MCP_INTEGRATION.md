# MCP Integration Guide for NAIJA AFTER DARK

## Overview

This document describes the Model Context Protocol (MCP) ecosystem integration for the NAAD project, including GitHub, Supabase, Playwright, and Unity MCP servers.

## Available MCP Servers

### GitHub MCP Server

**Purpose:** Issue/PR automation and code review workflow

**Available Tools:**
- `create_pull_request` - Create pull requests
- `add_comment_to_pending_review` - Add review comments to PRs
- `add_issue_comment` - Add comments to issues/PRs
- `list_issues` - List issues with filtering
- `search_issues` - Search issues with complex queries
- `get_pull_request` - Get PR details
- `list_pull_requests` - List PRs with filtering

**Agent Assignment Workflow (Alternative to AssignCodingAgent):**

Since GitHub MCP does not have an `AssignCodingAgent` tool, use the following alternatives:

1. **PR Reviewers:**
   - Use `create_pull_request` with `reviewers` parameter to assign reviewers
   - Example: Create PR with specific reviewers for agent assignment

2. **Issue Labels:**
   - Use `add_issue_comment` to add labels indicating assigned agent
   - Example: Label with `agent:devin` or `agent:human`

3. **Assignees:**
   - Use GitHub native assignee field via issue/PR operations
   - Assign to team members or service accounts

4. **Automated Comments:**
   - Use `add_issue_comment` to notify agents via @mentions
   - Example: Comment with "@devin please review this PR"

**Usage Example:**
```javascript
// Create PR with reviewers
await mcp_call_tool({
  server_name: "github-mcp-server",
  tool_name: "create_pull_request",
  arguments: {
    owner: "feexsystems",
    repo: "naija-after-dark",
    title: "Implement dialogue branching system",
    body: "Implements full dialogue branching as per Megaplan Track A",
    head: "feature/dialogue-branching",
    base: "main",
    reviewers: ["devin-ai-bot"]
  }
})
```

### Supabase MCP Server

**Purpose:** Supabase project management and database operations

**Available Tools:**
- `list_projects` - List Supabase projects
- `get_project` - Get project details
- `search_projects` - Search projects by criteria
- Documentation search tools

**Usage Guidelines:**
- Use read-only operations by default
- Project creation/management may incur costs (user approval required)
- Use for schema validation and migration testing

**Usage Example:**
```javascript
// List projects
await mcp_call_tool({
  server_name: "supabase-mcp-server",
  tool_name: "list_projects",
  arguments: {}
})
```

### Playwright MCP Server

**Purpose:** Browser automation for UI testing

**Available Tools:**
- `navigate` - Navigate to URL
- `screenshot` - Take screenshot
- `click` - Click elements
- `fill` - Fill form fields
- `evaluate` - Execute JavaScript in browser

**Use Cases:**
- Test Supabase dashboard UI
- Test authentication flows
- Test Edge Function endpoints via web interface
- Automated visual regression testing

**Usage Example:**
```javascript
// Navigate and screenshot
await mcp_call_tool({
  server_name: "mcp-playwright",
  tool_name: "navigate",
  arguments: {
    url: "https://app.supabase.com/project/unzfqrfyejkyisalzkhc"
  }
})
```

### Unity MCP Server

**Purpose:** Unity Editor automation via JSON-RPC

**Configuration:**
- Endpoint: `http://localhost:17890/mcp/rpc`
- Plugin: `com.ivanmurzak.unity.mcp`
- Manifest: `game/unity/mcp-manifest.json`

**Requirements:**
- Unity Editor must be running
- MCP plugin must be loaded
- Port 17890 must be listening

**Startup Instructions:**
1. Open Unity Editor
2. Ensure `com.ivanmurzak.unity.mcp` package is installed
3. MCP server starts automatically on port 17890
4. Verify with: `curl http://localhost:17890/mcp/rpc`

**Usage:**
- Automated build operations
- Scene verification
- Asset import/export
- Test execution

## MCP Command Registry

### GitHub MCP

```json
{
  "github-mcp-server": {
    "purpose": "GitHub issue/PR automation",
    "tools": ["create_pull_request", "add_comment_to_pending_review", "list_issues"],
    "enabled": true,
    "configuration": {
      "owner": "feexsystems",
      "repo": "naija-after-dark"
    }
  }
}
```

### Supabase MCP

```json
{
  "supabase-mcp-server": {
    "purpose": "Supabase project management",
    "tools": ["list_projects", "get_project"],
    "enabled": true,
    "configuration": {
      "projectId": "unzfqrfyejkyisalzkhc"
    }
  }
}
```

### Playwright MCP

```json
{
  "mcp-playwright": {
    "purpose": "Browser automation testing",
    "tools": ["navigate", "screenshot", "click"],
    "enabled": true
  }
}
```

### Unity MCP

```json
{
  "unity-mcp": {
    "purpose": "Unity Editor automation",
    "tools": ["json_rpc"],
    "enabled": true,
    "configuration": {
      "endpoint": "http://localhost:17890/mcp/rpc"
    }
  }
}
```

## Troubleshooting

### Unity MCP Not Reachable

**Symptom:** `curl http://localhost:17890/mcp/rpc` fails

**Solutions:**
1. Ensure Unity Editor is running
2. Check that `com.ivanmurzak.unity.mcp` package is installed
3. Check Unity Console for MCP plugin errors
4. Restart Unity Editor

### GitHub MCP Authentication

**Symptom:** GitHub API returns 401 Unauthorized

**Solutions:**
1. Configure GitHub token in environment variables
2. Check token has required permissions (repo, issues, pull_requests)
3. Verify token is not expired

### Supabase MCP Cost Warning

**Symptom:** Operations that may incur costs

**Solutions:**
1. Use read-only operations by default
2. Get user approval before write operations
3. Use staging project for testing

## Best Practices

1. **Use Read-First:** Always attempt read operations before writes
2. **User Approval:** Get explicit approval for cost-incurring operations
3. **Error Handling:** Check MCP tool responses for errors
4. **Idempotency:** Design operations to be idempotent where possible
5. **Logging:** Log all MCP operations for debugging
6. **Fallback:** Have fallback procedures when MCP is unavailable

## Integration with Agent Skills

### task-analysis Skill

The task-analysis skill can be adapted to use MCP tools for:
- Automated issue creation for new tasks
- PR creation for completed features
- Automated testing via Playwright

### game-development Skill

The game-development skill can use Unity MCP for:
- Automated build operations
- Test execution
- Screenshot capture for verification

### auto-cr Skill

The auto-cr skill can use GitHub MCP for:
- Automated code review
- PR comment analysis
- Review comment suggestions
