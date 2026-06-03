# Architecture: ShardsCounter

## 1. Purpose

This document describes the planned architecture for ShardsCounter, a .NET Android assistant app for tracking values during a two-player game of "Shards of Infinity".

The architecture is intentionally simple for the first implementation, while keeping game logic separate from Android UI code so the rules can be tested and evolved independently.

## 2. Architectural Goals

- Keep game rules independent from Android views and activities.
- Keep the first implementation small and easy to understand.
- Make all state changes explicit and centralized.
- Persist and restore the current game state reliably.
- Support a landscape, two-player tabletop layout.
- Avoid unnecessary dependencies for the initial version.

## 3. Technology Choices

- Platform: Android
- Framework: .NET Android targeting `net10.0-android`
- UI approach: native Android views defined in XML layouts and controlled from `MainActivity`
- Cross-platform UI frameworks such as .NET MAUI are intentionally not used.
- Compose-style/declarative UI abstractions are intentionally not used.
- Persistence: local JSON file stored in app-private storage
- Serialization: `System.Text.Json`
- Dependency injection: none for the first implementation

## 4. Project Structure

Planned source structure:

```plaintext
ShardsCounter/
  MainActivity.cs
  Models/
    GameIdentity.cs
    PlayerSlot.cs
    PlayerState.cs
    GameState.cs
    GamePhase.cs
  Services/
    GameService.cs
    GameStateStore.cs
  Resources/
    layout/
      activity_main.xml
    values/
      strings.xml
```

### 4.1 `MainActivity`

Responsibilities:

- Own the Android screen lifecycle.
- Load persisted game state on startup.
- Keep the screen awake while the activity is visible.
- Render the current state into Android views.
- Wire button clicks to `GameService` methods.
- Show confirmation dialogs, validation errors, and winner popup.
- Save state after each relevant state change and during lifecycle transitions.

`MainActivity` should not contain game-rule calculations directly.

### 4.2 Models

Models represent serializable app state and fixed game concepts.

#### `GameIdentity`

Represents one fixed identity:

- `Name`
- `Color`

The available identities are fixed:

- Tetra: blue
- Decima: yellow
- Volos: green
- Ko Syn Wu: purple
- Rez: red

#### `PlayerSlot`

Represents the physical/logical player slot:

- `Player1`
- `Player2`

#### `PlayerState`

Represents per-player state:

- Selected identity
- Health Points
- Mastery

#### `GamePhase`

Represents whether the app is currently selecting identities or actively tracking a game:

- `IdentitySelection`
- `InGame`

#### `GameState`

Represents the complete persisted state:

- Current phase
- Player 1 state
- Player 2 state
- Active player
- Starting player of the current game
- Starting player of the previous game, if known
- Current turn crystals
- Current turn damage
- Pending identity selection for Player 1
- Pending identity selection for Player 2
- Flag indicating whether the app has already created its first game

## 5. Game Logic Service

`GameService` contains all game-state transition logic.

Planned responsibilities:

- Create the initial state.
- Select identities for players.
- Validate that selected identities are unique.
- Start a game from valid identity selections.
- Reset numeric values to their starting values.
- Determine the first starting player randomly for the first ever game.
- Alternate the starting player for later games.
- Adjust Health Points with clamping between 0 and 50.
- Adjust Mastery with clamping between 0 and 30.
- Adjust crystals and damage with a minimum of 0.
- End the active player's turn.
- Apply accumulated damage to the opponent on turn end.
- Detect when a player reaches 0 Health Points.
- Return enough information for the UI to show the winner popup.
- Move back to identity selection after game end or confirmed manual `New Game`.

The service should avoid Android-specific APIs.

## 6. Persistence

Persistence is handled by `GameStateStore`.

### 6.1 Storage Format

- JSON
- Stored in app-private storage
- File name: `game-state.json`

### 6.2 Persistence Behavior

- Load state when the app starts.
- If no persisted state exists, create a new state in `IdentitySelection` phase.
- Save state after every state-changing action.
- Save state during lifecycle events such as pause/stop.
- Restore the last state when the app is reopened after being closed or backgrounded.

### 6.3 Versioning

For the first implementation, persistence versioning can be minimal. `GameState` should include a simple `StateVersion` field so future migrations are possible.

## 7. UI Architecture

The first implementation uses a single activity with two visual modes:

1. Identity selection mode
2. In-game mode

Both modes are rendered by showing/hiding sections in `activity_main.xml`.

### 7.1 Identity Selection Mode

Purpose:

- Let both players choose one identity on their own side of the screen.

Layout:

- Landscape orientation.
- Two player-facing selection areas.
- Each side shows five identity buttons.
- Each identity button uses the identity's background color and name.
- One player's side is rotated 180 degrees so both players can read their own controls.

Behavior:

- Each player taps one identity button.
- If both players choose the same identity, show an error and remain in identity selection.
- If both players choose different identities, start the game and reset all numeric values.

### 7.2 In-Game Mode

Landscape layout:

```plaintext
[New Game][P1 Identity above P1 HP/P1 Mastery][Crystals][Damage]
[New Game][P2 Identity above P2 HP/P2 Mastery][End Turn below Crystals/Damage]
```

Shared areas:

- `New Game` spans both rows.
- `New Game` uses an unrotated button hit area with an overlaid text label that is statically rotated 90 degrees so it is upright when the phone is held upright; the app does not check the current device orientation for this.
- `Crystals` and `Damage` are side by side in a shared turn-control area with equal width.
- `Buy Mastery` and `End Turn` are below `Crystals` and `Damage` in the shared turn-control area from the active player's point of view.
- The bottom button row is only tall enough to comfortably fit its text.
- `Buy Mastery` is enabled once per turn when current-turn Crystals is at least 1 and the active player's Mastery is below 30. It reduces Crystals by 1 and increases active-player Mastery by 1.
- `End Turn` opens a damage resolution popup with Shield and Champion value controls plus a tappable Damage area when current-turn Damage is greater than 0. If Damage is 0, the popup is skipped and the turn advances immediately. Shield and Champion use their image backgrounds and the same number size as the popup Damage value. Tapping the popup Damage area applies original damage minus shield minus champion and advances the turn.
- In each player row, the identity area is a narrow strip above the side-by-side HP and Mastery controls.
- The turn-controls area should take roughly 66% of the width available to stats/turn controls, with the player stats area taking roughly 34%.

Rotation:

- Player 1 value areas are rotated toward Player 1.
- Player 2 value areas are rotated toward Player 2.
- The shared turn-controls group containing `Crystals`, `Damage`, `Buy Mastery`, and `End Turn` rotates toward the active player so the bottom button row stays below the value controls from that player's point of view.
- `New Game` does not need active-player rotation unless this becomes desirable later.

### 7.3 Reusable Value Control

Health Points, Mastery, Crystals, and Damage use the same visual pattern:

```plaintext
-5            +5
     Value
-1            +1
```

Value controls omit separate title rows and unit suffixes. The center value shows only the number over an enlarged PNG image representing the value type: HP, Mastery, Crystals, or Damage. The four corner tap targets are transparent and each occupy one quarter of the area.

Behavior:

- One tap changes the value once.
- No long-press repeat.
- No haptic feedback.
- No sound feedback.
- Current value is visually larger than the adjustment buttons.

## 8. Dialogs and User Feedback

### 8.1 New Game Confirmation

When `New Game` is tapped:

1. Show a confirmation dialog.
2. If cancelled, keep the current game unchanged.
3. If confirmed, switch to identity selection.
4. After valid identity selection, reset values and start the next game.

### 8.2 Duplicate Identity Error

If both players select the same identity:

- Show an error message.
- Do not start the game.
- Keep the current selections visible so players can change them.

### 8.3 Winner Popup

When a player reaches 0 Health Points:

1. Determine the winner as the opponent of the player at 0 Health Points.
2. Show popup text: `Player <Name of Identity> won`.
3. Keep the current game screen visible behind the popup.
4. After the popup is closed, move to identity selection for the next game.
5. After valid identity selection, reset values and start the next game.

## 9. Constants and Rules

Use named constants instead of repeated magic numbers:

- Starting Health Points: `50`
- Minimum Health Points: `0`
- Maximum Health Points: `50`
- Starting Mastery: `0`
- Minimum Mastery: `0`
- Maximum Mastery: `30`
- Starting crystals per turn: `0`
- Starting damage per turn: `0`
- Counter adjustment steps: `-5`, `-1`, `+1`, `+5`

## 10. Testing Strategy

Game rules should be testable without Android UI.

Automated tests are intentionally postponed for the first implementation. The architecture should still keep game logic separate so tests can be added later without major restructuring.

Recommended future test coverage:

- Health Points clamping.
- Mastery clamping.
- Crystals and damage never going below 0.
- End turn applies damage to the opponent.
- End turn resets crystals and damage.
- Turn switches after ending a turn.
- Winner is detected when a player reaches 0 Health Points.
- Duplicate identity selection is rejected.
- First ever starting player can be random.
- Later starting players alternate.
- State can be serialized and deserialized.

A separate test project can be added later if desired.

## 11. Implementation Order

Suggested implementation sequence:

1. Add model classes.
2. Add `GameService` with pure game logic.
3. Add `GameStateStore` persistence.
4. Update Android manifest/activity settings for landscape and keep-awake behavior.
5. Build identity-selection UI.
6. Build in-game layout.
7. Wire buttons to `GameService`.
8. Add dialogs and winner flow.
9. Validate build and manually test core flows.
10. Add automated tests later.

## 12. Open Architecture Questions

No open architecture questions at this time.

## 13. Architecture Decisions

- Automated tests are postponed until after the first implementation.
- The winner popup keeps the current game screen visible behind it; identity selection is shown only after closing the popup.
- `New Game` and game-end reset use the same identity-selection flow, including starting-player alternation.
- The app is Android-only.
- The UI is implemented with native Android XML layouts and `MainActivity` code-behind.
- .NET MAUI is not used because cross-platform support is not planned.
