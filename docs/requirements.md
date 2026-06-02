# Requirements: ShardsCounter

## 1. Project Goal

ShardsCounter is an assistant app for the two-player card game "Shards of Infinity". The app is intended to lie on the table between both players during the game and display important game values so players do not have to remember or track them manually elsewhere.

## 2. Target Platform

- Platform: Android
- Technology: .NET Android
- Target framework: .NET 10
- Usage context: The device lies on the table between two players during a game

## 3. Primary Users

- Two players in a local game of "Shards of Infinity"

## 4. Current Scope

The exact feature scope will be clarified step by step.

### Already Known Desired Values

- HP / Health
- Mastery
- Player identity and identity color
- Crystals accumulated during the active player's turn
- Damage accumulated during the active player's turn

## 5. Requirements

### 5.1 Functional Requirements

#### 5.1.1 Player Identity

- At the beginning of the game, each player chooses one identity.
- There are five available identities.
- The available identities are:
  - Tetra: blue
  - Decima: yellow
  - Volos: green
  - Ko Syn Wu: purple
  - Rez: red
- Each identity has a corresponding color.
- Each player's selected identity and corresponding color must remain visible throughout the whole game.
- The identity area shows the identity name with the corresponding identity color as its background color.
- Identity names and colors are not editable.
- Player identities must be unique; both players cannot have the same identity.
- After starting a new game, both players choose their identity manually on their own side of the screen.
- Each player chooses their identity by clicking one of five identity buttons.
- The identity-selection screen uses landscape orientation and is rotated toward the players.
- If both players choose the same identity, the app shows an error and does not start the game.
- If both players choose different identities, the app starts the game by resetting all values to their starting values.
- For the first ever game after app installation, the starting player is chosen randomly.
- After the first game, the starting player alternates from the previous game's starting player.

#### 5.1.2 Persistent Player Values

- The app must track and display Health Points for both players throughout the whole game.
- Health Points start at 50.
- Health Points have a maximum value of 50.
- Health Points can be changed using +1, -1, +5, and -5 buttons.
- Health Points are clamped between 0 and 50.
- The app must track and display Mastery for both players throughout the whole game.
- Mastery starts at 0.
- Mastery has a maximum value of 30.
- Mastery can be changed using +1, -1, +5, and -5 buttons.
- Mastery is clamped between 0 and 30.

#### 5.1.3 Turn State

- The app must track whose turn it currently is.
- The active player must be visually highlighted.
- During a player's turn, the app must show the crystals accumulated by the active player during that turn.
- During a player's turn, the app must show the damage accumulated by the active player during that turn.
- Crystals start at 0 at the beginning of each turn.
- Damage starts at 0 at the beginning of each turn.
- Crystals can be changed using +1, -1, +5, and -5 buttons.
- Damage can be changed using +1, -1, +5, and -5 buttons.
- Crystals can never go below 0.
- Damage can never go below 0.
- When the next turn starts, the app must show that the other player is now the active player.

#### 5.1.4 End Turn Behavior

- When the active player ends their turn, the opponent's Health Points are reduced by the active player's accumulated damage.
- After ending the turn, crystals reset to 0.
- After ending the turn, damage resets to 0.
- The turn switches to the other player.
- If a player's Health Points are at or below 0, the game ends.
- Mastery can never go below 0.

#### 5.1.5 Game End Behavior

- When a player reaches 0 Health Points, the app shows a small popup.
- The popup text is: `Player <Name of Identity> won`.
- The winning player is the opponent of the player whose Health Points reached 0.
- The current game screen remains visible behind the popup.
- When the player closes the winner popup, the app moves to identity selection for the next game.
- When the game resets after a game end, both players choose their identity manually on their own side of the screen.
- When the game resets after a game end, the starting player is the player who did not start the previous game.
- If the players want to play another game, they can close the popup and continue with the reset game state.

#### 5.1.6 Manual New Game

- The app provides a `New Game` button.
- Pressing `New Game` requires confirmation before leaving the current game.
- After confirming `New Game`, both players choose their identity manually on their own side of the screen.
- Each player chooses from the five fixed identity options.
- The identity-selection screen is rotated toward the players in landscape mode.
- If both players choose the same identity, the app shows an error and does not start the game.
- If both players choose different identities, the app starts the game by resetting all values to their starting values.

### 5.2 Non-Functional Requirements

To be specified.

### 5.3 Interaction Concept

- Tapping `End Turn` does not require confirmation.
- Buttons do not provide haptic feedback for the initial implementation.
- Buttons do not provide sound feedback for the initial implementation.
- Long-press behavior is not supported; one tap performs exactly one change.
- There is no undo feature for accidental taps in the initial implementation.

### 5.4 Display / Layout

#### 5.4.1 Landscape Layout

- The initial layout target is landscape orientation.
- Portrait orientation is ignored for the initial implementation.
- The screen is organized as two player rows where each player row shows the identity as a narrow strip above side-by-side HP and Mastery controls:
  - Row 1: `[New Game][P1 Identity above P1 HP/P1 Mastery][Crystals][Damage]`
  - Row 2: `[New Game][P2 Identity above P2 HP/P2 Mastery][End Turn spanning below Crystals and Damage]`
- The `New Game` button is placed left of the player identity areas.
- The `New Game` button hit area is not rotated; only its visible text is statically rotated 90 degrees so it is upright when the phone is held upright. This does not require checking the current device orientation.
- `Crystals`, `Damage`, and `End Turn` are shared turn-control areas, not duplicated per player.
- `New Game` is a shared control area, not duplicated per player.
- The shared `New Game` area spans both player rows.
- Each player identity area spans the width of that player's HP and Mastery controls.
- The shared turn-controls area containing Crystals, Damage, and End Turn should be wider than the player stats area, approximately 66% turn controls to 34% player stats.
- The shared `Crystals` and `Damage` areas are side by side, and the shared `End Turn` button is below them from the active player's point of view.
- The shared `End Turn` button should be only tall enough to comfortably fit its text.
- The shared turn-controls group rotates toward the active player so `End Turn` stays below `Crystals` and `Damage` from the active player's point of view.
- Each player's own value areas are rotated toward that player.
- The app does not need to decide which physical side of the device belongs to Player 1 or Player 2; players can rotate the device on the table if needed.

#### 5.4.2 Value Control Layout

- The Health Points, Mastery, Crystals, and Damage areas use the same internal control layout.
- Each value area contains four adjustment buttons and the current value.
- The layout is:
  - Top left: `-1`
  - Top right: `+1`
  - Center: current value
  - Bottom left: `-5`
  - Bottom right: `+5`
- The current value is visually larger than the adjustment buttons.
- The center value shows only the number, without headers or units.
- The center value is displayed over a PNG image representing the value type: HP, Mastery, Crystals, or Damage.

### 5.5 Persistence / Game State

- The current game state must be persisted.
- If the app is closed or backgrounded, the current game state should be restored when the app is reopened.
- The app should keep the screen awake during a game.

### 5.6 Rules / Game Automation

To be specified.

## 6. Open Questions

1. Should values only be changed manually, or should the app understand additional game rules and automate more actions?
2. Should there be different modes, for example base game, expansions, or custom rules?
3. Are there requirements for design, colors, icons, or visual similarity to the board game?
4. Should the app support exactly two players only, or be designed for later extensibility?

## 7. Decisions

- The requirements document is written in English.
- English game terminology is used, for example "Mastery" instead of translated terms.
- The app tracks two players.
- Health Points are tracked for both players, start at 50, and have a maximum value of 50.
- Health Points can be changed with +1, -1, +5, and -5 buttons and are clamped between 0 and 50.
- Mastery is tracked for both players, starts at 0, and has a maximum value of 30.
- Mastery can be changed with +1, -1, +5, and -5 buttons and is clamped between 0 and 30.
- The active player is highlighted during their turn.
- The active player's accumulated crystals and damage are displayed during their turn.
- The available identities are Tetra, Decima, Volos, Ko Syn Wu, and Rez.
- Identity colors are fixed: Tetra is blue, Decima is yellow, Volos is green, Ko Syn Wu is purple, and Rez is red.
- Identity names and colors are not editable.
- The identity area displays the identity name with the identity color as its background color.
- Player identities must be unique.
- Both players choose their identities manually on their own side of the screen when starting a new game.
- The identity-selection screen is rotated toward the players in landscape mode.
- If both players choose the same identity, the app shows an error and does not start the game.
- The first ever starting player is chosen randomly.
- After the first game, the starting player alternates from the previous game's starting player.
- Crystals and damage start at 0 at the beginning of each turn.
- Crystals and damage can be changed with +1, -1, +5, and -5 buttons.
- Crystals and damage can never go below 0.
- Ending a turn applies the active player's accumulated damage to the opponent's Health Points, resets crystals and damage to 0, and switches the active player.
- A player loses when their Health Points are at or below 0.
- When a player reaches 0 Health Points, the app shows a popup saying `Player <Name of Identity> won`.
- When the winner popup is closed, the app moves to identity selection for the next game.
- When a game resets after game end, both players choose identities manually again.
- The next game's starting player is the player who did not start the previous game.
- After manually starting a `New Game`, the starting player alternates from the previous game's starting player.
- The app provides a `New Game` button that requires confirmation before leaving the current game and opening identity selection.
- The current game state is restored after closing, backgrounding, or reopening the app.
- The app keeps the screen awake during a game.
- Mastery cannot go below 0.
- The initial UI target is landscape orientation.
- Portrait orientation is ignored for the initial implementation.
- Player value areas are rotated toward their respective player.
- Shared New Game, Crystals, Damage, and End Turn areas span both player rows; Crystals, Damage, and End Turn rotate toward the active player.
- Health Points, Mastery, Crystals, and Damage use a shared value-control layout with -1, +1, -5, and +5 buttons around a larger central value.
- End Turn does not require confirmation.
- Buttons have no haptic feedback, no sound feedback, no long-press behavior, and no undo support in the initial implementation.
- The app does not need to assign Player 1 or Player 2 to a fixed physical side of the device.

## 8. Change Log

- Initial version created.
- Translated requirements document to English.
- Added initial player identity, persistent value, and turn-state requirements.
- Added identity uniqueness, random initial identity assignment, turn reset behavior, end-turn damage application, and game-end condition.
- Corrected Health Points start value to 50, added random first player selection, and added +1/-1/+5/-5 controls for crystals and damage.
- Added +1/-1/+5/-5 controls for Health Points and Mastery, plus clamping rules for Health Points, Mastery, crystals, and damage.
- Added game-end popup and reset behavior.
- Clarified reset behavior: identities are randomized again, and the next starting player alternates from the previous game's starting player.
- Added landscape layout requirements, player-facing rotation, shared active turn controls, and value-control layout.
- Added interaction decisions: no End Turn confirmation, no feedback, no long-press behavior, no undo, portrait ignored, and no fixed physical side assignment.
- Added persistence requirements, keep-screen-awake behavior, and a confirmed New Game control placed left of the identity areas.
- Added final identity names, fixed identity colors, and identity display requirements.
- Replaced random identity assignment with manual per-player identity selection, including duplicate-identity validation.
- Clarified that identity selection is rotated toward players and manual New Game alternates the starting player.
- Clarified that the first ever starting player is random, while later games alternate the starting player.
- Clarified that after game end, the current game remains visible behind the winner popup and identity selection is shown only after closing the popup.
