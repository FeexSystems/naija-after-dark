# GAMEPLAY ENHANCEMENT PLAN

## Overview
This plan outlines the enhancement phases for the Naija After Dark project, building upon the foundational components completed in Phase 1.

---

## Phase 1: Foundations ✅ (Weeks 1-2)
**Status**: Complete

### Completed Components:
- **Dialogue System**: DialogueTest.cs - TalkToTundel async test script ready
- **UI Base**: DialogueBubble.cs - TextPro UI base script created
- **Model Validation**: ModelValidator.cs - 57 model import verifier scripted
- **Networking**: Supabase config finalized with URL and key
- **Editor Tools**: Index.ts - EdgF function template ready
- **Asset Pipeline**: Blender procedural asset system with 57 assets built

### Success Criterions:
- ✅ 57/57 model import validation passing (Blender build + round-trip QA)
- ✅ Supabase project configured
- ✅ Dialogue system base functional
- ✅ All 57 assets exported (GLB + FBX) with preview renders
- ✅ Asset verification script (verify_assets.py) passing

---

## Phase 2: Enhance (Weeks 3-5) ✅ **Complete**

### Overview
Phase 2 focuses on enriching the gameplay experience with advanced systems building on the Phase 1 foundations.

### Week 3: Advanced NPC Behavioral System ✅ **Complete**

#### Goals:
- Implement sophisticated AI-driven NPC behaviors
- Create dynamic conversation branching based on player context
- Integrate environmental awareness into NPC decision-making

#### Tasks:
1. **NPC State Machine**: Design and implement a comprehensive state machine for NPCs covering:
   - Idle/wander behaviors
   - Conversation initiation
   - Task-based interactions
   - Environmental awareness (time of day, player proximity, etc.)

2. **Behavioral Configurations**: Create data-driven behavior configurations using ScriptableObjects:
   - NPC personality profiles affecting conversation style
   - Relationship mapping between NPCs
   - Dynamic mood systems affecting dialogue options

3. **Tunde NPC Integration**: 
   - Integrate the validated 57 models into Tunde's character controller
   - Implement animation blending for conversation states
   - Create context-aware gesture systems

#### Deliverables:
- `Assets/NAAD/NPCs/TundeNPC.cs` - Advanced Tunde NPC controller
- `Assets/NAAD/NPCs/BehaviorStateMachine.cs` - State machine implementation
- `Assets/NAAD/NPCs/DialogueBranch.cs` - Conversation branching logic

---

### Week 4: Dialogue System Enhancement ✅ **Complete**

#### Goals:
- Enhance the existing dialogue system with branching conversations
- Add procedural dialogue generation capabilities
- Implement memory system for conversation context

#### Tasks:
1. **Branch Dialogue System**: 
   - Extend existing `DialogueBubble.cs` to support multiple choice responses
   - Implement dialogue trees with conditional logic
   - Create save/load state for conversation progress

2. **Context-Aware Dialogue**:
   - Integrate player reputation/relationship states into available options
   - Implement memory of past conversations
   - Add topic progression system

3. **Procedural Elements**:
   - Generate greeting/farewell sequences procedurally
   - Create filler dialogue for ambient NPCs
   - Implement random topic injection based on environment

#### Deliverables:
- `Assets/NAAD/Dialogue/EnhancedDialogue.cs` - Branch dialogue system
- `Assets/NAAD/Dialogue/DialogueMemory.cs` - Conversation memory system
- `Assets/NAAD/Dialogue/DialogueConditions.cs` - Conditional dialogue triggers

---

### Week 5: Environmental Enrichment & UI/UX Enhancements ✅ **Complete**

#### Goals:
- Populate the world with interactive prop systems
- Enhance UI/UX for dialogue and NPC interactions
- Create immersive environmental feedback systems

#### Completed:
1. **Interactive Prop System**:
   - InteractiveProp.cs with E-key interaction framework
   - Proximity triggers with configurable range
   - Context-sensitive interactions based on NPC state
   - Sound and visual feedback for interactions

2. **Environmental Storytelling**:
   - AmbientDialogueTrigger.cs for ambient dialogue
   - Triggered by player actions with cooldown system
   - NPC assignment and nearest NPC detection

3. **UI/UX Enhancements**:
   - Updated DialogueBubbleManager.cs with screen space positioning
   - Multi-screen support with bounds clamping
   - ConversationUI.cs with typing effect system
   - Conversation history scroll UI
   - Avatar portrait system for speaking NPCs
   - Emotion indicators and relationship meter

4. **Audio Integration**:
   - DialogueAudio.cs for context-appropriate sound effects
   - Ambient background music system with fade in/out
   - UI sound cues for interactions
   - Environmental audio support

#### Deliverables:
- ✅ `Assets/NAAD/Environment/InteractiveProp.cs` - Prop interaction system
- ✅ `Assets/NAAD/Environment/AmbientDialogueTrigger.cs` - Ambient dialogue triggers
- ✅ `Assets/NAAD/UI/ConversationUI.cs` - Enhanced conversation UI
- ✅ `Assets/NAAD/Audio/DialogueAudio.cs` - Dialogue audio system
- ✅ Updated `DialogueBubbleManager.cs` - Screen space positioning
- ✅ Updated `DialogueBubble.cs` - Screen space mode support

---

### Cross-Phase Dependencies

#### Requires Phase 1 Completion:
- ✅ 57/57 model validation passing
- ✅ Supabase networking backbone
- ✅ Basic dialogue UI framework

#### Feeds Into Phase 3:
- Advanced AI/LLM orchestration (Weeks 6-8)
- Multiplayer/Services integration (Weeks 9-10)
- Polish and optimization (Weeks 11-12)

### Success Metrics for Phase 2
- [x] Advanced NPC state machine operational with 5+ distinct behaviors per NPC
- [x] Branch dialogue system supports 3+ conditional pathways per conversation
- [x] Interactive prop system with E-key interactions and context sensitivity
- [x] Enhanced UI/UX with typing effect, history scroll, and avatar portraits
- [x] Dialogue audio system with ambient music and UI sound cues
- [x] Conversation memory system functional across session restarts

### Next Steps After Phase 2
Proceed to Phase 3: AI/LLM Orchestration Enhancement (Weeks 6-8), which will focus on:
- Integrating LLM-powered dialogue generation
- Multiplayer synchronization of NPC states
- Advanced environmental systems
- Performance optimization across all enhancements
