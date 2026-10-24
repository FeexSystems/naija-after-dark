# Dialogue System Future Enhancements Implementation Plan

This document provides a detailed implementation plan for the dialogue system future enhancements listed in the Implementation Walkthrough.

## Table of Contents

1. [Enhancement Overview](#enhancement-overview)
2. [Enhancement 1: Visual Node Graph Editor](#enhancement-1-visual-node-graph-editor)
3. [Enhancement 2: Dialogue Editor Window](#enhancement-2-dialogue-editor-window)
4. [Enhancement 3: Dialogue Preview in Editor Mode](#enhancement-3-dialogue-preview-in-editor-mode)
5. [Enhancement 4: Backend Test Integration](#enhancement-4-backend-test-integration)
6. [Enhancement 5: Automated Dialogue Testing in CI/CD](#enhancement-5-automated-dialogue-testing-in-cicd)
7. [Implementation Phases](#implementation-phases)
8. [Estimated Timeline](#estimated-timeline)

---

## Enhancement Overview

### Current State

The dialogue system has:
- **Data structures:** DialogueData.cs with nodes, responses, conditions
- **ScriptableObject authoring:** DialogueGraph.cs for basic Unity Editor creation
- **Runtime system:** DialogueManager, DialogueBubbleManager, DialogueMemory
- **Testing:** DialogueTestRunner for Editor-only unit tests

### Enhancement Goals

1. **Visual Node Graph Editor** - Provide XGraph-like visual editing for dialogue trees
2. **Dialogue Editor Window** - Custom editor window for easier dialogue authoring
3. **Dialogue Preview in Editor Mode** - Play dialogue without entering Play Mode
4. **Backend Test Integration** - Validate dialogue commands against Edge Function
5. **Automated Dialogue Testing in CI/CD** - Run dialogue tests in GitHub Actions

### Architecture Constraints

- **Server-authoritative:** All authoritative mutations must go through Supabase Edge Function
- **Unity as presentation layer:** Dialogue branching logic is local, state changes are server-validated
- **No secrets in Unity:** Service-role keys and Gemini credentials stay server-side
- **Test safety:** Backend tests should use staging project, not production

---

## Enhancement 1: Visual Node Graph Editor

### Objective

Create a visual node graph editor for DialogueGraph using Unity's Graph Editor framework (similar to Shader Graph, Visual Scripting, or XGraph).

### Technical Approach

#### Option A: Unity Graph Editor Framework (Recommended)

Unity 6 provides a built-in Graph Editor framework for creating node-based editors.

**Pros:**
- Native Unity integration
- Built-in node, edge, and graph management
- Consistent with Unity's visual scripting tools
- Good performance for large graphs

**Cons:**
- Learning curve for Graph Editor API
- Requires Unity 6 (already using 6000.6.5f1)
- Limited documentation in some areas

#### Option B: Custom Editor Window with Node Framework

Use a third-party node framework (e.g., NodeGraphProcessor, xNode) or build custom.

**Pros:**
- More control over UI/UX
- Can use existing frameworks
- Not tied to Unity's Graph Editor API

**Cons:**
- Additional dependencies
- More maintenance burden
- May not integrate as well with Unity workflows

**Recommendation:** Use Unity Graph Editor Framework (Option A).

### Implementation Steps

#### Phase 1: Setup Graph Editor Infrastructure

**Files to create:**
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueGraphAsset.cs` - Graph asset
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueGraphView.cs` - Graph view
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueGraphWindow.cs` - Graph window
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueNodeView.cs` - Node view
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueEdgeView.cs` - Edge view

**Dependencies:**
- Unity Graph Editor API (Unity 6)
- `UnityEngine.GraphTools Foundation` package

**Tasks:**
1. Create DialogueGraphAsset extending GraphAsset
2. Create DialogueGraphWindow extending GraphWindow
3. Register graph in Unity's Graph system
4. Create basic node and edge views
5. Implement Save/Load to DialogueGraph ScriptableObject

**Estimated Time:** 2-3 days

#### Phase 2: Implement Node Types

**Node types to implement:**
- **DialogueNode** - Main dialogue node with speaker, text, emotion
- **ResponseNode** - Player response option
- **ConditionNode** - Condition evaluation (reputation, seen, relationship)
- **EffectNode** - State change effect (reputation += 10)
- **StartNode** - Dialogue entry point
- **EndNode** - Dialogue exit point

**Tasks:**
1. Create node model classes for each type
2. Create node view classes with UI
3. Implement data serialization to DialogueGraph
4. Add node search/palette
5. Add node validation (required fields)

**Estimated Time:** 3-4 days

#### Phase 3: Implement Connections and Logic

**Tasks:**
1. Implement node-to-node connections (edges)
2. Add connection validation (Response → Dialogue, Condition → Branch)
3. Implement graph traversal visualization
4. Add condition visual indicators
5. Add preview of dialogue flow

**Estimated Time:** 2-3 days

#### Phase 4: Polish and Features

**Tasks:**
1. Add node grouping/folders
2. Add mini-map for large graphs
3. Add search/filter nodes
4. Add undo/redo support
5. Add graph templates (basic greeting, quest dialogue, etc.)
6. Add keyboard shortcuts

**Estimated Time:** 2-3 days

### Acceptance Criteria

- [ ] Graph editor opens from DialogueGraph ScriptableObject
- [ ] Can create, delete, and connect nodes
- [ ] Can edit node properties in Inspector
- [ ] Graph serializes to DialogueGraph correctly
- [ ] Can export/import to JSON
- [ ] Undo/redo works
- [ ] Performance acceptable for 100+ node graphs

### Risks and Mitigations

**Risk:** Unity Graph Editor API complexity
**Mitigation:** Start with simple node types, expand gradually

**Risk:** Performance with large graphs
**Mitigation:** Implement virtualization for node views, use async loading

**Risk:** Breaking existing DialogueGraph ScriptableObjects
**Mitigation:** Maintain backward compatibility, provide migration tool

---

## Enhancement 2: Dialogue Editor Window

### Objective

Create a custom Editor Window for easier dialogue authoring, providing a form-based alternative to the node graph editor.

### Technical Approach

Create a standalone Editor Window that provides:
- Form-based dialogue editing
- NPC and dialogue list management
- Quick preview of dialogue text
- Search and filter capabilities
- Bulk operations (rename, delete, export)

### Implementation Steps

#### Phase 1: Create Editor Window ✅ COMPLETED

**Files created:**
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueEditorWindow.cs` - Main editor window

**Tasks completed:**
1. Created Editor Window with menu item (Window > NAAD > Dialogue Editor)
2. Implemented two-panel layout (list + detail)
3. Load all DialogueGraph assets from project
4. Display dialogue list with basic info (ID, name, node count)
5. Implemented selection handling
6. Added search bar for dialogue list
7. Added filter by NPC ID

**Estimated Time:** 1-2 days
**Actual Time:** 1 day

**Commit:** `cbc209f` - feat: implement Dialogue Editor Window (Enhancement 2, Phase 1)

#### Phase 2: Implement Detail View ✅ COMPLETED

**Files created:**
- `game/unity/Assets/NAAD/Dialogue/Editor/DialogueDetailPanel.cs` - Dialogue detail panel

**Tasks completed:**
1. Display selected dialogue properties (ID, name, description)
2. Display node list with editing capability
3. Add create/delete node buttons
4. Implement node property editing (speaker, text, emotion)
5. Add response editing with conditions
6. Integrated DetailPanel into DialogueEditorWindow

**Estimated Time:** 2-3 days
**Actual Time:** 1 day

**Commit:** `8e47260` - feat: implement Dialogue Detail Panel (Enhancement 2, Phase 2)

#### Phase 3: Add Search and Filter ✅ COMPLETED

**Tasks completed:**
1. Added search bar for dialogue list (in Phase 1)
2. Added filter by NPC ID (in Phase 1)
3. Filter options saved

**Estimated Time:** 1 day
**Actual Time:** Included in Phase 1

#### Phase 4: Add Bulk Operations ✅ COMPLETED

**Tasks completed:**
1. Added bulk export to JSON
2. Added bulk import from JSON
3. Added bulk export/import buttons to toolbar

**Estimated Time:** 1-2 days
**Actual Time:** 0.5 day

**Commit:** `c29ff73` - feat: add bulk operations to Dialogue Editor (Enhancement 2, Phase 4)

#### Phase 5: Polish ⏳ OPTIONAL

**Tasks pending:**
1. Add keyboard shortcuts
2. Add undo/redo
3. Add dialogue statistics (word count, estimated duration)
4. Add validation (required fields, orphaned nodes)
5. Add help documentation

**Estimated Time:** 1-2 days

### Acceptance Criteria

- [x] Editor window opens from menu
- [x] Can create, edit, delete dialogues
- [x] Can search and filter dialogues
- [x] Can perform bulk operations (export/import)
- [x] Changes persist to ScriptableObjects
- [ ] Undo/redo works (Phase 5)
- [ ] Validation detects errors (Phase 5)

### Risks and Mitigations

**Risk:** Duplicate functionality with node graph editor
**Mitigation:** Position as complementary tool - node graph for structure, editor window for content editing

**Risk:** Performance with many dialogues
**Mitigation:** Implement lazy loading, pagination

---

## Enhancement 3: Dialogue Preview in Editor Mode

### Objective

Enable dialogue preview and testing without entering Play Mode, using Play Mode simulation or Editor-only preview system.

### Technical Approach

#### Option A: Play Mode Simulation (Recommended)

Use Unity's Play Mode simulation in Editor to run dialogue logic without full game state.

**Pros:**
- Can test actual runtime behavior
- Uses existing DialogueManager and DialogueBubbleManager
- Minimal code changes

**Cons:**
- Requires entering "simulated" Play Mode
- May still need some game state initialization

#### Option B: Editor-Only Preview System

Create a separate preview system that runs outside Play Mode.

**Pros:**
- No Play Mode overhead
- Faster iteration
- Can run in background

**Cons:**
- Requires duplicate preview logic
- May not match runtime behavior exactly
- More maintenance burden

**Recommendation:** Use Play Mode Simulation (Option A) with Editor-only mode for quick text preview.

### Implementation Steps

#### Phase 1: Editor-Only Text Preview

**Files to create:**
- `game/unity/Assets/NAAD/Dialogue/Editor/DialoguePreviewWindow.cs` - Preview window

**Tasks:**
1. Create preview window (Window > NAAD > Dialogue Preview)
2. Add text rendering field with styling
3. Load selected DialogueGraph
4. Display starting node
5. Add response buttons
6. Implement basic flow (select response → show next node)

**Estimated Time:** 1-2 days

#### Phase 2: Play Mode Simulation

**Files to modify:**
- `game/unity/Assets/NAAD/Dialogue/Editor/DialoguePreviewWindow.cs`
- `game/unity/Assets/NAAD/Gameplay/DialogueManager.cs` (add Editor-only mode)

**Tasks:**
1. Add Enter/Exit Play Mode Simulation buttons
2. Initialize minimal game state for dialogue
3. Create DialogueManager instance in simulation
4. Create DialogueBubbleManager instance in simulation
5. Run dialogue flow with full UI
6. Add ability to simulate NPC state (reputation, relationships)

**Estimated Time:** 2-3 days

#### Phase 3: Condition Testing

**Tasks:**
1. Add reputation slider to preview window
2. Add relationship sliders for factions
3. Add "seen dialogues" toggle list
4. Real-time condition evaluation
5. Show which responses are available/hidden based on conditions

**Estimated Time:** 1-2 days

#### Phase 4: Polish

**Tasks:**
1. Add preview history (recent dialogues)
2. Add bookmark dialogues for quick access
3. Add export preview to text file
4. Add screenshot of dialogue flow
5. Add keyboard shortcuts

**Estimated Time:** 1 day

### Acceptance Criteria

- [ ] Can preview dialogue without entering Play Mode
- [ ] Can navigate dialogue tree
- [ ] Can test conditions (reputation, relationships)
- [ ] Preview matches runtime behavior
- [ ] Fast enough for iterative editing

### Risks and Mitigations

**Risk:** Preview doesn't match runtime behavior
**Mitigation:** Use same DialogueManager code, add validation tests

**Risk:** Simulation requires too much game state
**Mitigation:** Start with minimal state, expand as needed

---

## Enhancement 4: Backend Test Integration

### Objective

Integrate dialogue system tests with Supabase Edge Function validation to ensure dialogue commands work correctly with the backend.

### Technical Approach

Create integration tests that:
- Send dialogue commands to Edge Function
- Validate Edge Function responses
- Check state changes in PostgreSQL
- Test error handling (rejected commands)

### Implementation Steps

#### Phase 1: Edge Function Test Client

**Files to create:**
- `backend/ai-orchestrator/tests/dialogue-integration.test.ts` - Integration tests
- `backend/ai-orchestrator/src/test-client.ts` - Test helper client

**Tasks:**
1. Create test client for Edge Function
2. Add mock/staging environment configuration
3. Implement dialogue command sending
4. Implement response validation
5. Add test utility functions

**Estimated Time:** 1-2 days

#### Phase 2: Dialogue Command Tests

**Test cases to implement:**
1. Start dialogue command
2. Select response command
3. Reputation change command
4. Relationship change command
5. Invalid response command (reject)
6. State query command

**Tasks:**
1. Write test for each command type
2. Add assertions for expected responses
3. Add assertions for state changes
4. Add error case tests
5. Add cleanup (reset state after tests)

**Estimated Time:** 2-3 days

#### Phase 3: Unity-Backend Integration Tests

**Files to create:**
- `game/unity/Assets/NAAD/Editor/DialogueBackendTestRunner.cs` - Unity editor test runner

**Tasks:**
1. Create Unity Editor test runner
2. Add menu item (NAAD > Tests > Run Backend Dialogue Tests)
3. Call backend tests from Unity
4. Display results in Unity console
5. Generate test report

**Estimated Time:** 1-2 days

#### Phase 4: Staging Environment Setup

**Tasks:**
1. Create Supabase staging project (if not exists)
2. Deploy Edge Functions to staging
3. Configure test credentials
4. Add staging project to CI/CD
5. Document staging environment setup

**Estimated Time:** 1-2 days (may require manual Supabase setup)

### Acceptance Criteria

- [ ] Edge Function integration tests pass
- [ ] Tests validate state changes in PostgreSQL
- [ ] Tests handle error cases (rejected commands)
- [ ] Unity can run backend tests
- [ ] Tests use staging environment (not production)
- [ ] Tests are idempotent (can run multiple times)

### Risks and Mitigations

**Risk:** Staging environment not available
**Mitigation:** Create local Supabase instance or use Docker

**Risk:** Tests mutate production data
**Mitigation:** Use staging environment exclusively, add safeguards

**Risk:** Tests are flaky due to network latency
**Mitigation:** Add retry logic, timeout handling

---

## Enhancement 5: Automated Dialogue Testing in CI/CD

### Objective

Add automated dialogue testing to GitHub Actions workflow to ensure dialogue system integrity on every commit.

### Technical Approach

Extend existing CI/CD workflow to include:
- Dialogue data structure validation
- Dialogue graph validation (no orphaned nodes)
- Backend integration tests (if staging available)
- Dialogue test runner execution

### Implementation Steps

#### Phase 1: Dialogue Data Validation

**Files to create:**
- `game/unity/Assets/NAAD/Editor/DialogueDataValidator.cs` - Data validation

**Tasks:**
1. Create validation script for DialogueGraph assets
2. Check for orphaned nodes (no incoming/outgoing edges)
3. Check for missing required fields
4. Check for invalid node IDs
5. Check for invalid response conditions
6. Generate validation report

**Estimated Time:** 1-2 days

#### Phase 2: Unity CLI Test Command

**Files to modify:**
- `game/unity/Assets/NAAD/Editor/DialogueTestRunner.cs`
- `game/unity/scripts/test.bat`

**Tasks:**
1. Add batch mode support to DialogueTestRunner
2. Add exit code for test failure
3. Add test.bat option for dialogue tests
4. Add Unity CLI command for dialogue tests
5. Generate JUnit XML output for CI/CD

**Estimated Time:** 1 day

#### Phase 3: CI/CD Integration

**Files to modify:**
- `.github/workflows/naad-unity.yml`

**Tasks:**
1. Add "verify-dialogue" job
2. Add dialogue data validation step
3. Add dialogue test runner step
4. Add backend integration test step (if staging available)
5. Upload test results as artifacts
6. Add test result summary to PR

**Estimated Time:** 1 day

#### Phase 4: Test Reporting

**Tasks:**
1. Generate HTML test report
2. Add test coverage metrics
3. Add test trend (pass/fail over time)
4. Add Slack/Discord notification on failure
5. Add test result badge to README

**Estimated Time:** 1-2 days

### Acceptance Criteria

- [ ] Dialogue validation runs in CI/CD
- [ ] Dialogue tests run in CI/CD
- [ ] Backend integration tests run in CI/CD (if staging available)
- [ ] Test results uploaded as artifacts
- [ ] Failed tests fail the workflow
- [ ] Test results visible in PR

### Risks and Mitigations

**Risk:** CI/CD execution time increases significantly
**Mitigation:** Run tests in parallel, cache results

**Risk:** Backend tests flaky in CI/CD
**Mitigation:** Add retry logic, use staging with reliable infrastructure

**Risk:** Tests require staging environment
**Mitigation:** Make backend tests optional, skip if staging unavailable

---

## Implementation Phases

### Phase 1: Editor Experience (2-3 weeks)

**Focus:** Enhancements 1, 2, 3 (Visual Node Graph, Editor Window, Preview)

**Priority:** High - Improves developer productivity immediately

**Dependencies:**
- None (independent of backend)

**Milestones:**
- Week 1: Visual Node Graph Editor basic implementation
- Week 2: Dialogue Editor Window implementation
- Week 3: Dialogue Preview and polish

### Phase 2: Backend Integration (1-2 weeks)

**Focus:** Enhancement 4 (Backend Test Integration)

**Priority:** Medium - Important for server-authoritative validation

**Dependencies:**
- Staging Supabase project setup

**Milestones:**
- Week 1: Edge Function test client and tests
- Week 2: Unity-Backend integration and staging setup

### Phase 3: CI/CD Automation (1 week)

**Focus:** Enhancement 5 (Automated Testing in CI/CD)

**Priority:** Medium - Important for continuous quality

**Dependencies:**
- Phase 1 (dialogue validation)
- Phase 2 (backend tests, optional)

**Milestones:**
- Week 1: CI/CD integration and test reporting

---

## Estimated Timeline

### Conservative Estimate (5-6 weeks)

- **Phase 1:** 3 weeks (Visual Node Graph, Editor Window, Preview)
- **Phase 2:** 2 weeks (Backend Integration)
- **Phase 3:** 1 week (CI/CD Automation)

**Total:** 6 weeks

### Optimistic Estimate (4-5 weeks)

- **Phase 1:** 2 weeks (Visual Node Graph, Editor Window, Preview)
- **Phase 2:** 1.5 weeks (Backend Integration)
- **Phase 3:** 1 week (CI/CD Automation)

**Total:** 4.5 weeks

### Factors Affecting Timeline

- **Unity Graph Editor API learning curve:** May add 1-2 days
- **Staging environment setup:** May add 2-3 days (manual process)
- **Backend test complexity:** May add 1-2 days
- **CI/CD debugging:** May add 1 day

---

## Next Steps

### Immediate Actions

1. **Review and approve this plan** - Confirm priorities and approach
2. **Set up staging Supabase project** - Required for Phase 2
3. **Create tracking board** - GitHub Projects or similar for task tracking
4. **Create feature branches** - One branch per enhancement or phase

### Recommended Starting Point

**Start with Enhancement 2 (Dialogue Editor Window)**

**Rationale:**
- Lower complexity than Visual Node Graph
- Immediate value for content creators
- Independent of other enhancements
- Can be delivered quickly (1-2 weeks)

**Then proceed with:**
- Enhancement 3 (Dialogue Preview) - builds on Editor Window
- Enhancement 1 (Visual Node Graph) - more complex, higher impact
- Enhancement 4 (Backend Integration) - requires staging setup
- Enhancement 5 (CI/CD Automation) - final polish

---

## Additional Resources

- **Unity Graph Editor Documentation:** https://docs.unity3d.com/6000.0/Documentation/Manual/GraphToolsFoundation-Overview.html
- **Unity Editor Window Documentation:** https://docs.unity3d.com/Manual/ExtendingTheEditor.html
- **Supabase Edge Functions:** https://supabase.com/docs/guides/functions
- **GitHub Actions for Unity:** https://github.com/game-ci/documentation
- **Existing Dialogue System:** `docs/IMPLEMENTATION_WALKTHROUGH.md`
