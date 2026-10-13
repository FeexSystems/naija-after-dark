# NAAD First Night Gameplay Integration

## Overview

This document defines the complete First Night gameplay loop integration, building on the existing `FirstNightController.cs` implementation.

## Required Sequence

**Full Sequence**: Apartment → Phone → Tunde message → Destination selection → Travel → Suya Spot → NPC interaction → Validated purchase → Nightclub invitation → Nightclub → Beach event → Relationship/memory update → Return home → Complete Night → Night Summary

## Existing Implementation Status

### Completed Steps
✓ StartingNight - Start night command  
✓ Phone - Load Tunde invite  
✓ AwaitingPhoneChoice - Wait for player response  
✓ TravelSuya - Travel to Suya Spot  
✓ BuySuya - Purchase suya  
✓ TravelClub - Travel to Nightclub  
✓ BuyTicket - Purchase club ticket  
✓ CompletingNight - Complete night command  
✓ Summary - Display night summary  

### Missing Steps
✗ Apartment - Starting location setup  
✗ Destination selection - Choose where to go  
✗ NPC interaction at Suya Spot - Dialogue with suya vendor  
✗ Beach event - Beach activities  
✗ Relationship/memory update - Commit relationship changes  
✗ Return home - Travel back to apartment  

## Step-by-Step Integration

### Step 1: Apartment (Starting Location)

**Entry Conditions**:
- Player is authenticated
- Player has not started a night yet
- Player is at apartment location

**Authoritative Command**: None (initial state)  
**Read Service**: `IPlayerService.FetchCurrentPlayerAsync()`  
**Unity Interaction**: 
- Show apartment interior
- Display player avatar
- Show "Start Night" button
- Display current balance

**Server Errors**: 
- Authentication failure → Redirect to login
- Player data fetch failure → Retry with error message

**State Refresh**: 
- Load player profile
- Load current balance
- Set location to apartment

**Interrupted Session Recovery**:
- Check if night already started
- If in progress, resume at current step
- If completed, show summary

### Step 2: Phone (Check Messages)

**Entry Conditions**:
- Night has started
- Player is at apartment

**Authoritative Command**: `GameCommandType.StartNight`  
**Read Service**: `IPhoneService.ListMessagesJsonAsync()`  
**Unity Interaction**:
- Open phone UI
- Show message list
- Highlight Tunde's message
- Display message content

**Server Errors**:
- StartNight command failure → Show error, retry option
- Message fetch failure → Show error, retry option

**State Refresh**:
- Set night status to active
- Load messages
- Update notification badges

**Interrupted Session Recovery**:
- If night started but no message response, resume at phone step
- If message responded, skip to destination selection

### Step 3: Tunde Message (Respond to Invite)

**Entry Conditions**:
- Tunde's message is displayed
- Message has no chosen_action yet

**Authoritative Command**: `GameCommandType.PhoneReply`  
**Read Service**: None  
**Unity Interaction**:
- Show message from Tunde
- Display response options: GO, ASK_DETAILS, DECLINE
- Handle user selection

**Server Errors**:
- PhoneReply command failure → Show error, retry option
- Invalid action → Show error, re-display options

**State Refresh**:
- Update message with chosen_action
- Update relationship with Tunde based on response
- Unlock next step based on response

**Interrupted Session Recovery**:
- If message already has chosen_action, skip to destination selection
- If GO, unlock Suya Spot and Nightclub
- If ASK_DETAILS, show details then unlock destinations
- If DECLINE, end night early

### Step 4: Destination Selection

**Entry Conditions**:
- Player has responded to Tunde's message
- Response was GO or ASK_DETAILS

**Authoritative Command**: None (presentation only)  
**Read Service**: None  
**Unity Interaction**:
- Show destination options: Suya Spot, Nightclub, Beach
- Display estimated costs
- Show travel time
- Handle user selection

**Server Errors**: None (local selection)

**State Refresh**: None (selection stored locally until travel)

**Interrupted Session Recovery**:
- If destination selected but not traveled, re-display selection
- If traveled, skip to travel step

### Step 5: Travel

**Entry Conditions**:
- Destination selected
- Player has sufficient balance for travel

**Authoritative Command**: `GameCommandType.Travel`  
**Read Service**: None  
**Unity Interaction**:
- Show travel animation
- Display loading screen
- Update location
- Show arrival notification

**Server Errors**:
- Travel command failure → Show error, retry option
- Insufficient balance → Show error, return to destination selection

**State Refresh**:
- Update player location
- Deduct travel cost (if any)
- Update scene to destination

**Interrupted Session Recovery**:
- If travel command succeeded but scene not loaded, load scene
- If at destination, skip to next step

### Step 6: Suya Spot (NPC Interaction)

**Entry Conditions**:
- Player is at Suya Spot location
- Player has not purchased suya yet

**Authoritative Command**: None (dialogue)  
**Read Service**: `IDialogueService` (if available)  
**Unity Interaction**:
- Show suya vendor NPC
- Display dialogue options
- Handle NPC interaction
- Show suya menu

**Server Errors**:
- Dialogue service unavailable → Use deterministic fallback dialogue
- Dialogue fetch failure → Use fallback dialogue

**State Refresh**:
- Load NPC dialogue
- Update relationship with vendor based on interaction

**Interrupted Session Recovery**:
- If suya purchased, skip to purchase step
- If at Suya Spot, re-display NPC

**Deterministic Fallback Dialogue**:
```
Vendor: "Welcome! Fresh suya ready. You want some?"
Options: ["Yes, please", "How much?", "Not today"]
```

### Step 7: Validated Purchase (Buy Suya)

**Entry Conditions**:
- Player is at Suya Spot
- Player has interacted with vendor
- Player has sufficient balance

**Authoritative Command**: `GameCommandType.Spend`  
**Read Service**: None  
**Unity Interaction**:
- Show suya menu
- Display prices
- Handle purchase
- Show success/failure feedback

**Server Errors**:
- Spend command failure → Show error, retry option
- Insufficient balance → Show error, return to menu
- Invalid SKU → Show error, refresh menu

**State Refresh**:
- Deduct cost from balance
- Add suya to inventory (if implemented)
- Update vendor relationship

**Interrupted Session Recovery**:
- If suya purchased, skip to Nightclub invitation
- If at Suya Spot, re-display menu

### Step 8: Nightclub Invitation

**Entry Conditions**:
- Player has purchased suya
- Player has not visited Nightclub yet

**Authoritative Command**: None (presentation)  
**Read Service**: `IPhoneService.ListMessagesJsonAsync()`  
**Unity Interaction**:
- Show Nightclub invitation message
- Display invitation details
- Show "Go to Nightclub" option

**Server Errors**:
- Message fetch failure → Show error, retry option
- No invitation found → Skip to Beach event

**State Refresh**:
- Load invitation message
- Update notification badges

**Interrupted Session Recovery**:
- If invitation accepted, skip to Nightclub travel
- If Nightclub visited, skip to Beach event

### Step 9: Nightclub (Enter and Activities)

**Entry Conditions**:
- Player has Nightclub invitation
- Player has sufficient balance for ticket

**Authoritative Command**: `GameCommandType.Travel` + `GameCommandType.Spend`  
**Read Service**: None  
**Unity Interaction**:
- Travel to Nightclub
- Show entrance queue
- Purchase ticket
- Enter Nightclub
- Show Nightclub interior
- Display activities (dance, socialize, drink)

**Server Errors**:
- Travel command failure → Show error, retry option
- Spend command failure → Show error, retry option
- Insufficient balance → Show error, return to Suya Spot

**State Refresh**:
- Update location to Nightclub
- Deduct ticket cost
- Update Nightclub visit status

**Interrupted Session Recovery**:
- If at Nightclub, re-display interior
- If ticket purchased, skip purchase step

### Step 10: Beach Event

**Entry Conditions**:
- Player has visited Nightclub
- Player has not visited Beach yet

**Authoritative Command**: `GameCommandType.Travel`  
**Read Service**: None  
**Unity Interaction**:
- Travel to Beach
- Show Beach scene
- Display beach activities
- Handle beach interactions

**Server Errors**:
- Travel command failure → Show error, retry option

**State Refresh**:
- Update location to Beach
- Update Beach visit status

**Interrupted Session Recovery**:
- If at Beach, re-display scene
- If Beach visited, skip to return home

### Step 11: Relationship/Memory Update

**Entry Conditions**:
- Player has completed all activities
- Night is ending

**Authoritative Command**: `GameCommandType.RelationshipDelta` + `GameCommandType.CommitMemory`  
**Read Service**: None  
**Unity Interaction**:
- Show relationship updates
- Display memory commit options
- Handle user confirmation

**Server Errors**:
- RelationshipDelta command failure → Show error, retry option
- CommitMemory command failure → Show error, retry option

**State Refresh**:
- Update relationship values
- Commit memory to player profile

**Interrupted Session Recovery**:
- If relationships updated, skip to return home
- If memory committed, skip to return home

### Step 12: Return Home

**Entry Conditions**:
- All activities completed
- Relationships updated
- Memory committed

**Authoritative Command**: `GameCommandType.Travel`  
**Read Service**: None  
**Unity Interaction**:
- Show return home option
- Travel to apartment
- Show apartment interior

**Server Errors**:
- Travel command failure → Show error, retry option

**State Refresh**:
- Update location to apartment
- Set night status to ending

**Interrupted Session Recovery**:
- If at apartment, skip to complete night

### Step 13: Complete Night

**Entry Conditions**:
- Player is at apartment
- Night status is ending

**Authoritative Command**: `GameCommandType.CompleteNight`  
**Read Service**: None  
**Unity Interaction**:
- Show complete night confirmation
- Display night summary
- Handle user confirmation

**Server Errors**:
- CompleteNight command failure → Show error, retry option

**State Refresh**:
- Set night status to completed
- Generate night summary
- Update player profile

**Interrupted Session Recovery**:
- If night completed, show summary directly

### Step 14: Night Summary

**Entry Conditions**:
- Night completed successfully

**Authoritative Command**: None (read only)  
**Read Service**: `IPhoneService.CompleteNightAsync()`  
**Unity Interaction**:
- Display night summary screen
- Show statistics (spent, transactions, people met)
- Display relationship updates
- Show best moment
- Display continue option

**Server Errors**:
- Summary fetch failure → Show error, retry option

**State Refresh**: None (summary is final)

**Interrupted Session Recovery**:
- If summary already viewed, show directly

## Implementation Plan

### Phase 1: Extend FirstNightController
1. Add missing steps to FirstNightStep enum
2. Implement missing step methods
3. Add destination selection logic
4. Add NPC interaction logic
5. Add Beach event logic
6. Add relationship/memory update logic
7. Add return home logic

### Phase 2: Unity Interactions
1. Create apartment scene
2. Create phone UI integration
3. Create destination selection UI
4. Create NPC interaction UI
5. Create Beach scene
6. Create relationship update UI
7. Create return home animation

### Phase 3: Testing
1. Create automated tests for each transition
2. Create smoke test for full sequence
3. Test interrupted session recovery
4. Test error handling
5. Test command failures

## Data Persistence

### Backend Contracts
- All mutations go through ICommandService
- No duplicate tables or parallel state stores
- No client-authoritative money
- Progress persisted through existing backend contracts

### Session Recovery
- Check night status on app start
- Resume at appropriate step
- Preserve incomplete transactions
- Handle network interruptions

## Error Handling

### Server Errors
- Show user-friendly error message
- Provide retry option
- Log error for debugging
- Preserve current state

### Rejected Commands
- Show rejection reason
- Provide alternative options
- Update UI to reflect rejection
- Allow retry with different parameters

## Testing Requirements

### Automated Tests
- Test each step transition
- Test command success
- Test command failure
- Test error handling
- Test session recovery

### Smoke Test
- Run full sequence automatically
- Report first failed step
- Provide evidence of failure
- Log all steps and results

## Next Steps

1. Extend FirstNightStep enum with missing steps
2. Implement missing step methods in FirstNightController
3. Create Unity UI for missing steps
4. Implement NPC interaction with dialogue service
5. Implement relationship/memory update
6. Create automated tests
7. Create smoke test
8. Run full sequence verification
