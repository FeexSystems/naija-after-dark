# NAAD Mobile UI Framework Architecture

## Overview

The NAAD mobile UI framework is a premium nightlife interface designed for portrait Android screens. It features dark surfaces, strong typography, readable status indicators, and accessible touch targets.

## Existing Infrastructure

### Service Contracts (Preserved)
- ✓ `IAuthService` - Authentication (sign up, sign in, sign out, session restore)
- ✓ `IPhoneService` - Phone features (messages, night summary, balance)
- ✓ `IPlayerService` - Player data (fetch current player)
- ✓ `ICommandService` - Game commands (travel, spend, buy, etc.)
- ✓ `IDialogueService` - NPC dialogue
- ✓ `IWorldStateService` - World state

### Existing UI
- ✓ `AuthController` - Authentication UI controller (preserved)

## UI Framework Principles

### 1. Service Layer Separation
- UI never calls backend services directly
- UI → Controller → Service → Backend
- Never fabricate backend responses
- Never show successful transaction unless authoritative command succeeds

### 2. State Management
- Each screen has explicit states: Loading, Empty, Failure, Retry, Success
- State transitions are handled by controllers
- UI reflects current state visually

### 3. Data Contracts
- Each screen connects to explicit data contract
- DTOs define data structure
- Services return typed data, not raw JSON

### 4. Mobile-First Design
- Portrait Android primary target
- Safe area support (notch, camera cutout)
- Varying aspect ratios supported
- Touch input optimized (44dp minimum touch targets)

## Screen Architecture

### Screen Structure
```
Screen (MonoBehaviour)
├── View (UI components)
├── Controller (Business logic)
├── ViewModel (Data binding)
└── Service (Backend connection)
```

### Screen Lifecycle
1. **Initialize**: Bind services, setup UI
2. **Load**: Fetch data from service
3. **Render**: Display data based on state
4. **Interact**: Handle user input
5. **Update**: Refresh data as needed
6. **Cleanup**: Unbind services, cleanup resources

## Required Screens

### Priority 1 (MVP)
1. **Bootstrap/Loading/Error** - Initial loading, error handling
2. **Main HUD** - In-game HUD overlay
3. **Phone Home** - Phone app launcher
4. **Ping Messaging** - Messaging interface
5. **Night Summary** - End-of-night summary

### Priority 2
6. **Character Creation** - New player setup
7. **Interaction Prompt** - In-game interaction prompts
8. **Bottom Navigation** - Home, Social, City, Life, Profile

### Priority 3
9. **NaijaGram Social Feed** - Social media feed
10. **NightPulse Events** - Event listings
11. **QuickCash Virtual Wallet** - Wallet/balance
12. **RideNow Transport** - Transportation
13. **MarketHub** - Marketplace
14. **MetroMap** - City map
15. **Music Controls** - Music player
16. **Relationship View** - Relationship status
17. **Mission/Event Notifications** - Notifications
18. **Settings and Accessibility** - Settings

## Screen Data Contracts

### Bootstrap Screen
**Service**: `IAuthService`, `IPlayerService`
**States**: Loading, Success, Error, Retry
**Data**: 
- Authentication status
- Player profile (if authenticated)

### Main HUD
**Service**: `IPlayerService`, `ICommandService`
**States**: Active, Hidden
**Data**:
- Player name
- Current location
- Balance
- Active interactions
- Time of day

### Phone Home
**Service**: `IPhoneService`
**States**: Loading, Success, Error
**Data**:
- App list (Ping, NaijaGram, NightPulse, QuickCash, RideNow, MarketHub, MetroMap, Music)
- Unread notification counts
- Battery status

### Ping Messaging
**Service**: `IPhoneService`
**States**: Loading, Empty, Success, Error, Retry
**Data**:
- Message list
- Contact list
- Unread count
- Typing indicators

### Night Summary
**Service**: `IPhoneService`, `ICommandService`
**States**: Loading, Success, Error
**Data**:
- `NightSummaryDto` (spent NGN, transactions, people met, new connection, trust with Tunde, best moment)

### Character Creation
**Service**: `IAuthService`, `IPlayerService`
**States**: Form, Submitting, Success, Error
**Data**:
- Display name
- Avatar selection
- Starting location

### NaijaGram Social Feed
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Empty, Success, Error, Refreshing
**Data**:
- Post list
- User profile
- Like/comment counts

### NightPulse Events
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Empty, Success, Error
**Data**:
- Event list
- Event details
- Attendance status

### QuickCash Virtual Wallet
**Service**: `IPhoneService`, `ICommandService`
**States**: Loading, Success, Error
**Data**:
- Balance
- Transaction history
- Send/receive options

### RideNow Transport
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Searching, Success, Error
**Data**:
- Available rides
- Current location
- Destination
- Price estimate

### MarketHub
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Empty, Success, Error
**Data**:
- Item listings
- Categories
- User inventory

### MetroMap
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Success, Error
**Data**:
- Map data
- Player location
- Points of interest
- Travel routes

### Music Controls
**Service**: `ICommandService` (to be implemented)
**States**: Playing, Paused, Error
**Data**:
- Current track
- Playlist
- Volume
- Playback controls

### Relationship View
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Empty, Success, Error
**Data**:
- Relationship list
- Relationship levels
- Interaction history

### Mission/Event Notifications
**Service**: `ICommandService` (to be implemented)
**States**: Loading, Empty, Success, Error
**Data**:
- Notification list
- Mission progress
- Event reminders

### Settings and Accessibility
**Service**: `IAuthService`, `IPlayerService`
**States**: Loading, Success, Error
**Data**:
- Account settings
- Display settings
- Audio settings
- Accessibility options

## Bottom Navigation

### Navigation Items
1. **Home** - Main hub, quick actions
2. **Social** - Ping, NaijaGram, relationships
3. **City** - MetroMap, RideNow, events
4. **Life** - QuickCash, MarketHub, music
5. **Profile** - Character stats, settings

### Navigation Behavior
- Persistent across app
- Active state indication
- Badge notifications for unread items
- Smooth transitions between tabs

## UI Component Library

### Design System
- **Theme**: Dark mode (premium nightlife aesthetic)
- **Primary Color**: Electric Purple (#9D4EDD)
- **Secondary Color**: Neon Pink (#FF006E)
- **Accent Color**: Cyan (#00F5D4)
- **Background**: Deep Black (#0A0A0A)
- **Surface**: Dark Gray (#1A1A1A)
- **Text**: White (#FFFFFF), Light Gray (#E0E0E0)

### Typography
- **Headline**: 24sp, Bold
- **Title**: 20sp, SemiBold
- **Body**: 16sp, Regular
- **Caption**: 14sp, Regular
- **Button**: 16sp, SemiBold

### Touch Targets
- **Minimum**: 44dp x 44dp
- **Recommended**: 48dp x 48dp
- **Spacing**: 8dp between elements

### Components
- **Button**: Primary, Secondary, Text
- **Card**: Elevated, flat
- **List**: Standard, grouped
- **Input**: Text, number, selection
- **Status Indicator**: Success, Warning, Error, Info
- **Progress Bar**: Linear, circular
- **Modal**: Bottom sheet, dialog
- **Toast**: Notification, error message

## State Management Pattern

### Screen State Enum
```csharp
public enum ScreenState
{
    Loading,
    Empty,
    Success,
    Error,
    Retry
}
```

### State Transition
1. Screen starts in Loading state
2. Fetch data from service
3. Transition to Success (data available) or Empty (no data) or Error (failure)
4. User can trigger Retry from Error state
5. State changes trigger UI updates

### Error Handling
- Network errors: Show retry option
- Validation errors: Show inline error
- Authentication errors: Redirect to login
- Unknown errors: Show generic error with retry

## Responsive Design

### Safe Areas
- Support for notches and camera cutouts
- Padding for status bar (24dp top)
- Padding for navigation bar (48dp bottom)
- Horizontal padding (16dp sides)

 Aspect Ratios
- **Primary**: 9:16 (portrait Android)
- **Supported**: 9:18, 9:19
- **Fallback**: Scale content for taller screens

### Touch Input
- Support for tap, long press, swipe
- Visual feedback on touch
- Haptic feedback where appropriate

## Implementation Priority

### Phase 1: Core Screens (MVP)
1. Bootstrap/Loading/Error screen
2. Main HUD
3. Phone Home
4. Ping Messaging
5. Night Summary
6. Bottom Navigation

### Phase 2: Additional Screens
7. Character Creation
8. Interaction Prompt
9. NaijaGram
10. NightPulse
11. QuickCash
12. RideNow
13. MarketHub
14. MetroMap

### Phase 3: Polish
15. Music Controls
16. Relationship View
17. Notifications
18. Settings
19. Accessibility features
20. Performance optimization

## Technical Stack

### Unity UI Framework
- **UI Toolkit** (recommended) or uGUI
- **UIDocument** for screen structure
- **USS** for styling
- **Data Binding** for view-model connection

### Alternative: uGUI
- **Canvas** with Canvas Scaler
- **RectTransform** for layout
- **UnityEvent** for interaction
- **Coroutines** for async operations

## Service Integration

### Controller Pattern
Each screen has a controller that:
1. Binds to required services
2. Fetches data asynchronously
3. Handles state transitions
4. Updates UI based on state
5. Handles user interactions

### Example Controller
```csharp
public class PhoneHomeController : MonoBehaviour
{
    private IPhoneService _phoneService;
    private ScreenState _state = ScreenState.Loading;
    
    private void Awake()
    {
        _phoneService = NAADApplicationRoot.Instance.Phone;
    }
    
    private async void Start()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        var balance = await _phoneService.GetBalanceAsync();
        // Update UI based on result
    }
}
```

## Testing Strategy

### Unit Tests
- Test controller logic
- Test state transitions
- Test service integration (with mocks)

### UI Tests
- Test touch target sizes
- Test safe area handling
- Test aspect ratio adaptation
- Test navigation flow

### Integration Tests
- Test screen loading
- Test data refresh
- Test error handling
- Test service communication

## Performance Considerations

### Optimization
- Object pooling for list items
- Lazy loading for large lists
- Image compression and caching
- Minimal redraws
- Efficient state updates

### Memory Management
- Unbind services on destroy
- Dispose of textures
- Clear event subscriptions
- Avoid memory leaks in coroutines

## Accessibility

### Features
- Screen reader support
- High contrast mode
- Text scaling
- Color adjustment
- Reduced motion

### Guidelines
- Semantic labels for UI elements
- Focus management
- Keyboard navigation support
- Clear error messages
- Sufficient color contrast (4.5:1 minimum)
