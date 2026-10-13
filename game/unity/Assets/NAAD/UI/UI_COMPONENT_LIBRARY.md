# NAAD UI Component Library

## Design System

### Color Palette

#### Primary Colors
- **Electric Purple**: `#9D4EDD` - Primary actions, branding
- **Neon Pink**: `#FF006E` - Accent, highlights
- **Cyan**: `#00F5D4` - Success, positive feedback

#### Background Colors
- **Deep Black**: `#0A0A0A` - Main background
- **Dark Gray**: `#1A1A1A` - Surface, cards
- **Medium Gray**: `#2A2A2A` - Elevated surfaces
- **Light Gray**: `#3A3A3A` - Borders, dividers

#### Text Colors
- **White**: `#FFFFFF` - Primary text
- **Light Gray**: `#E0E0E0` - Secondary text
- **Medium Gray**: `#A0A0A0` - Tertiary text, captions
- **Disabled Gray**: `#606060` - Disabled text

#### Status Colors
- **Success**: `#00F5D4` (Cyan)
- **Warning**: `#FFB703` (Yellow)
- **Error**: `#FF006E` (Neon Pink)
- **Info**: `#3A86FF` (Blue)

### Typography

#### Font Family
- **Primary**: Inter or Roboto (system font fallback)
- **Secondary**: Poppins (for headings, if available)

#### Font Sizes
- **Headline Large**: 28sp, Bold
- **Headline**: 24sp, Bold
- **Title**: 20sp, SemiBold
- **Subtitle**: 18sp, SemiBold
- **Body**: 16sp, Regular
- **Caption**: 14sp, Regular
- **Small**: 12sp, Regular

#### Line Heights
- **Headline**: 1.2
- **Title**: 1.3
- **Body**: 1.5
- **Caption**: 1.4

#### Letter Spacing
- **Headline**: -0.5%
- **Title**: 0%
- **Body**: 0.5%
- **Caption**: 1%

### Spacing

#### Scale
- **XS**: 4dp
- **S**: 8dp
- **M**: 16dp
- **L**: 24dp
- **XL**: 32dp
- **XXL**: 48dp

#### Usage
- **Component padding**: M (16dp)
- **Section spacing**: L (24dp)
- **Screen padding**: M (16dp)
- **Element spacing**: S (8dp)

### Touch Targets

#### Minimum Sizes
- **Button**: 44dp x 44dp (minimum), 48dp x 48dp (recommended)
- **Icon button**: 44dp x 44dp
- **List item**: 48dp height
- **Checkbox/Radio**: 44dp x 44dp

#### Spacing Between Touch Targets
- **Minimum**: 8dp
- **Recommended**: 16dp

### Border Radius

#### Values
- **Small**: 4dp
- **Medium**: 8dp
- **Large**: 16dp
- **XLarge**: 24dp
- **Full**: 50% (circle)

#### Usage
- **Buttons**: Medium (8dp)
- **Cards**: Large (16dp)
- **Inputs**: Medium (8dp)
- **Modals**: XLarge (24dp)

### Shadows

#### Elevation Levels
- **Level 1**: 2dp offset, 4dp blur
- **Level 2**: 4dp offset, 8dp blur
- **Level 3**: 8dp offset, 16dp blur
- **Level 4**: 16dp offset, 32dp blur

#### Usage
- **Cards**: Level 1
- **Modals**: Level 3
- **Navigation**: Level 2
- **Buttons**: Level 1 (pressed)

### Opacity

#### Values
- **Disabled**: 0.38
- **Secondary**: 0.6
- **Primary**: 0.87
- **Full**: 1.0

#### Usage
- **Disabled text**: 0.38
- **Secondary text**: 0.6
- **Primary text**: 0.87
- **Icons**: 1.0

## Components

### Button

#### Primary Button
- **Background**: Electric Purple (#9D4EDD)
- **Text**: White (#FFFFFF)
- **Height**: 48dp
- **Border Radius**: 8dp
- **States**: Normal, Pressed (darker), Disabled (opacity 0.38)

#### Secondary Button
- **Background**: Transparent
- **Border**: 2dp Electric Purple
- **Text**: Electric Purple
- **Height**: 48dp
- **Border Radius**: 8dp

#### Text Button
- **Background**: Transparent
- **Text**: Electric Purple
- **Height**: 44dp
- **States**: Normal, Pressed (opacity 0.6)

#### Icon Button
- **Size**: 44dp x 44dp
- **Icon**: Electric Purple
- **Background**: Transparent
- **States**: Normal, Pressed (darker background)

### Card

#### Standard Card
- **Background**: Dark Gray (#1A1A1A)
- **Border Radius**: 16dp
- **Padding**: 16dp
- **Shadow**: Level 1
- **Spacing**: 8dp between cards

#### Elevated Card
- **Background**: Dark Gray (#1A1A1A)
- **Border Radius**: 16dp
- **Padding**: 16dp
- **Shadow**: Level 2
- **Margin**: 8dp

#### Compact Card
- **Background**: Dark Gray (#1A1A1A)
- **Border Radius**: 8dp
- **Padding**: 12dp
- **Shadow**: Level 1

### List

#### Standard List Item
- **Height**: 56dp
- **Padding**: 16dp horizontal
- **Divider**: 1dp Medium Gray (#3A3A3A)
- **Touch Target**: Full width

#### Grouped List
- **Group Header**: 16dp padding, Medium Gray text
- **Item Height**: 56dp
- **Group Spacing**: 24dp
- **Rounded Corners**: 8dp on group

#### Compact List
- **Height**: 44dp
- **Padding**: 12dp horizontal
- **No Divider**: Optional

### Input

#### Text Input
- **Background**: Medium Gray (#2A2A2A)
- **Border**: 1dp Medium Gray (#3A3A3A)
- **Border Radius**: 8dp
- **Height**: 56dp
- **Padding**: 16dp horizontal
- **States**: Normal, Focused (Electric Purple border), Error (Neon Pink border)

#### Number Input
- Same as text input
- Keyboard: Numeric

#### Selection Input
- **Background**: Medium Gray (#2A2A2A)
- **Border**: 1dp Medium Gray (#3A3A3A)
- **Border Radius**: 8dp
- **Height**: 56dp
- **Chevron**: Right-aligned

### Status Indicator

#### Success
- **Color**: Cyan (#00F5D4)
- **Icon**: Checkmark
- **Background**: Transparent with colored icon

#### Warning
- **Color**: Yellow (#FFB703)
- **Icon**: Exclamation
- **Background**: Transparent with colored icon

#### Error
- **Color**: Neon Pink (#FF006E)
- **Icon**: X mark
- **Background**: Transparent with colored icon

#### Info
- **Color**: Blue (#3A86FF)
- **Icon**: Info circle
- **Background**: Transparent with colored icon

### Progress Bar

#### Linear Progress
- **Height**: 4dp
- **Background**: Medium Gray (#2A2A2A)
- **Fill**: Electric Purple (#9D4EDD)
- **Border Radius**: 2dp

#### Circular Progress
- **Size**: 48dp
- **Stroke Width**: 4dp
- **Color**: Electric Purple (#9D4EDD)

#### Indeterminate
- **Animation**: Moving gradient or pulsing
- **Color**: Electric Purple (#9D4EDD)

### Modal

#### Bottom Sheet
- **Background**: Dark Gray (#1A1A1A)
- **Border Radius**: 16dp top corners
- **Handle**: 4dp height, 32dp width, centered top
- **Padding**: 16dp
- **Shadow**: Level 3

#### Dialog
- **Background**: Dark Gray (#1A1A1A)
- **Border Radius**: 16dp
- **Padding**: 24dp
- **Shadow**: Level 3
- **Max Width**: 320dp

#### Full Screen Modal
- **Background**: Deep Black (#0A0A0A)
- **No Border Radius**
- **Full Screen**
- **Status Bar**: Transparent or matching

### Toast

#### Notification Toast
- **Background**: Dark Gray (#1A1A1A) with 0.9 opacity
- **Border Radius**: 8dp
- **Padding**: 12dp horizontal, 8dp vertical
- **Shadow**: Level 2
- **Duration**: 3 seconds
- **Position**: Bottom, above navigation

#### Error Toast
- **Background**: Neon Pink (#FF006E) with 0.9 opacity
- **Text**: White
- **Border Radius**: 8dp
- **Padding**: 12dp horizontal, 8dp vertical

#### Success Toast
- **Background**: Cyan (#00F5D4) with 0.9 opacity
- **Text**: Deep Black (#0A0A0A)
- **Border Radius**: 8dp
- **Padding**: 12dp horizontal, 8dp vertical

### Navigation Bar

#### Bottom Navigation
- **Height**: 56dp + safe area bottom
- **Background**: Dark Gray (#1A1A1A)
- **Border**: 1dp top, Medium Gray (#3A3A3A)
- **Item Size**: 56dp x 56dp
- **Icon Size**: 24dp
- **Active Color**: Electric Purple (#9D4EDD)
- **Inactive Color**: Medium Gray (#A0A0A0)
- **Badge**: 16dp diameter, Neon Pink (#FF006E)

#### Top Navigation
- **Height**: 56dp + safe area top
- **Background**: Deep Black (#0A0A0A)
- **Border**: 1dp bottom, Medium Gray (#3A3A3A)
- **Title**: 20sp, SemiBold, White
- **Back Button**: 44dp x 44dp, left aligned

### Avatar

#### Circular Avatar
- **Size**: 40dp (standard), 56dp (large)
- **Border Radius**: 50%
- **Background**: Medium Gray (#2A2A2A)
- **Initials**: White, 16sp
- **Border**: 2dp, optional

#### Group Avatar
- **Size**: 40dp
- **Overlap**: 20%
- **Background**: Different colors per person
- **Border**: 2dp, Dark Gray (#1A1A1A)

### Badge

#### Notification Badge
- **Size**: 16dp diameter
- **Background**: Neon Pink (#FF006E)
- **Text**: White, 10sp, Bold
- **Position**: Top-right of parent
- **Border**: 2dp, parent background

#### Status Badge
- **Height**: 20dp
- **Padding**: 4dp horizontal
- **Border Radius**: 10dp
- **Background**: Status color with 0.2 opacity
- **Text**: Status color, 10sp, SemiBold

### Divider

#### Standard Divider
- **Height**: 1dp
- **Color**: Medium Gray (#3A3A3A)
- **Margin**: 16dp horizontal

#### Section Divider
- **Height**: 8dp
- **Color**: Transparent
- **Margin**: 24dp vertical

### Icon

#### Standard Icon
- **Size**: 24dp
- **Color**: Light Gray (#E0E0E0)
- **Stroke**: 2dp

#### Small Icon
- **Size**: 16dp
- **Color**: Medium Gray (#A0A0A0)
- **Stroke**: 2dp

#### Large Icon
- **Size**: 32dp
- **Color**: White (#FFFFFF)
- **Stroke**: 2dp

## Layout Patterns

### Screen Layout

#### Standard Screen
```
┌─────────────────────────┐
│  Top Navigation (56dp) │
├─────────────────────────┤
│                         │
│  Content (flex)         │
│  - 16dp padding         │
│                         │
├─────────────────────────┤
│  Bottom Nav (56dp)      │
└─────────────────────────┘
```

#### Full Screen Content
```
┌─────────────────────────┐
│  Content (flex)         │
│  - 16dp padding         │
│  - No top/bottom nav    │
└─────────────────────────┘
```

#### Modal Content
```
┌─────────────────────────┐
│  Header (optional)      │
├─────────────────────────┤
│  Body (scrollable)      │
├─────────────────────────┤
│  Footer (actions)       │
└─────────────────────────┘
```

### List Layout

#### Standard List Item
```
┌─────────────────────────┐
│ [Icon][Title  ][Chevron]│
│ [   ][Subtitle][        ]│
└─────────────────────────┘
```

#### Card List Item
```
┌─────────────────────────┐
│ [Icon][Title  ][Badge  ]│
│ [   ][Subtitle][        ]│
│ [   ][Body              ]│
└─────────────────────────┘
```

### Form Layout

#### Vertical Form
```
┌─────────────────────────┐
│ Label                   │
│ [Input Field           ]│
│ Error message           │
│                         │
│ Label                   │
│ [Input Field           ]│
│ Error message           │
│                         │
│ [Submit Button        ]│
└─────────────────────────┘
```

## Animation

### Standard Durations
- **Fast**: 150ms
- **Medium**: 300ms
- **Slow**: 500ms

### Easing
- **Ease In**: Cubic
- **Ease Out**: Cubic
- **Ease In Out**: Cubic

### Transitions
- **Screen Push**: 300ms, ease in out
- **Modal Fade**: 200ms, ease out
- **Button Press**: 100ms, ease out
- **List Item**: 150ms staggered, ease out

### Feedback
- **Touch Ripple**: 300ms, ease out
- **Success Check**: 300ms, ease out
- **Error Shake**: 300ms, ease in out

## Responsive Guidelines

### Safe Areas
- **Top**: 24dp (status bar) + safe area
- **Bottom**: 16dp + safe area
- **Left**: 16dp + safe area
- **Right**: 16dp + safe area

### Aspect Ratios
- **9:16**: Standard portrait
- **9:18**: Tall portrait (scale content)
- **9:19**: Extra tall (scale content)

### Breakpoints
- **Small**: < 360dp width
- **Medium**: 360-411dp width
- **Large**: > 411dp width

## Accessibility

### Contrast Ratios
- **Minimum**: 4.5:1 (WCAG AA)
- **Recommended**: 7:1 (WCAG AAA)

### Font Scaling
- Support system font scaling
- Test at 1.5x and 2.0x scale
- Maintain layout integrity

### Screen Reader
- Semantic labels for all interactive elements
- Focus order logical
- State changes announced

### Color Blindness
- Don't rely on color alone
- Use icons and text labels
- Test with color blindness simulators

## Implementation Notes

### Unity UI Toolkit (Recommended)
- Use USS for styling
- Use UXML for structure
- Use data binding for view-model connection
- Use VisualElement for components

### uGUI Alternative
- Use Canvas Scaler for scaling
- Use RectTransform for layout
- Use UnityEvent for interaction
- Use Coroutines for async operations

### Component Reuse
- Create prefabs for common components
- Use scriptable objects for shared data
- Use asset bundles for theme resources
- Use addressables for dynamic loading
