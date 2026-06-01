# Implementation Prompt: ShardsCounter

You are implementing **ShardsCounter**, an Android-only .NET Android app for assisting two players during a local game of **Shards of Infinity**.

Before implementing, read and follow:

- `docs/requirements.md`
- `docs/architecture.md`
- `.github/copilot-instructions.md`

## Project Context

- Workspace root: `C:\TechemTools\Learning\ShardsCounter\ShardsCounter`
- Project: `ShardsCounter\ShardsCounter.csproj`
- Target framework: `net10.0-android`
- IDE: Visual Studio Professional 2026
- App type: native Android app using .NET Android
- UI approach: native Android XML layouts with `MainActivity` code-behind
- Do **not** use MAUI or cross-platform frameworks
- Do **not** introduce automated tests for the first implementation unless explicitly requested later

## High-Level Goal

Build a single-screen tabletop assistant app for two players. The device lies in landscape orientation between the players. The app tracks player identities, Health Points, Mastery, current-turn crystals, current-turn damage, active player, game reset, and game-end behavior.

## Core Requirements Summary

### Platform and Layout

- Android only.
- Native Android XML layouts.
- Landscape orientation only for the initial implementation.
- Portrait can be ignored.
- Keep the screen awake during a game.
- The UI should be suitable for a device lying between two players.
- Player-specific areas should be rotated toward their respective player.
- Shared turn controls should rotate toward the active player.

### Identities

There are five fixed identities:

- `Tetra`: blue
- `Decima`: yellow
- `Volos`: green
- `Ko Syn Wu`: purple
- `Rez`: red

Rules:

- Identity names and colors are fixed and not editable.
- Each player chooses an identity manually.
- Both players choose on their own side of the screen.
- The identity selection screen is landscape and rotated toward the players.
- Each player has five identity buttons.
- Both players cannot choose the same identity.
- If both choose the same identity, show an error and do not start the game.
- If they choose different identities, start the game and reset numeric values to their start values.
- The identity area in the game screen shows the identity name with its corresponding background color.

### Player Values

Each player has:

- Health Points
- Mastery

Health Points:

- Start: `50`
- Minimum: `0`
- Maximum: `50`
- Controls: `+1`, `-1`, `+5`, `-5`
- Clamp between `0` and `50`

Mastery:

- Start: `0`
- Minimum: `0`
- Maximum: `30`
- Controls: `+1`, `-1`, `+5`, `-5`
- Clamp between `0` and `30`

### Turn Values

Current turn values:

- Crystals
- Damage

Rules:

- Both start at `0` at the beginning of each turn.
- Both have controls: `+1`, `-1`, `+5`, `-5`.
- Neither can go below `0`.
- They belong to the currently active player’s turn, not permanently to either player.

### Starting Player

- For the first ever game after app installation, choose the starting player randomly.
- For all later games, alternate the starting player from the previous game's starting player.
- The active player should be visually highlighted.

### End Turn

When pressing `End Turn`:

1. Apply the active player's accumulated damage to the opponent's HP.
2. Clamp opponent HP to minimum `0`.
3. If opponent HP reaches `0`, the game ends.
4. If the game does not end:
   - reset crystals to `0`
   - reset damage to `0`
   - switch active player
5. `End Turn` does not require confirmation.

### Game End

When a player reaches `0` HP:

- Show a small popup saying: `Player <Name of Identity> won`
- The winner is the opponent of the player whose HP reached `0`.
- Keep the current game screen visible behind the popup.
- Only after the popup is closed, switch to identity selection for the next game.
- The next game uses the same identity-selection flow as manual `New Game`.
- The next game's starting player alternates from the previous game's starting player.

### New Game

- Provide a shared `New Game` button.
- It is placed left of the player identity areas.
- It spans both player rows.
- Pressing `New Game` requires confirmation.
- If cancelled, keep the current game unchanged.
- If confirmed, switch to identity selection.
- After valid identity selection, reset all values and start the next game.
- Manual `New Game` uses the same identity-selection flow as game-end reset.
- Starting player alternates from the previous game's starting player.

### Main Game Layout

Landscape layout:

```plaintext
[New Game][P1 Identity][P1 HP][P1 Mastery][Crystals][Damage][End Turn]
[New Game][P2 Identity][P2 HP][P2 Mastery][Crystals][Damage][End Turn]
```

Shared areas:

- `New Game` spans both rows.
- `Crystals` spans both rows.
- `Damage` spans both rows.
- `End Turn` spans both rows.

Rotation:

- Player 1 value areas rotate toward Player 1.
- Player 2 value areas rotate toward Player 2.
- Shared `Crystals`, `Damage`, and `End Turn` rotate toward the active player.
- `New Game` does not need active-player rotation.

### Value Control Layout

Health Points, Mastery, Crystals, and Damage use this internal layout:

```plaintext
-1            +1
     Value
-5            +5
```

- The current value is visually larger than the buttons.
- One tap performs one change.
- No long-press behavior.
- No haptic feedback.
- No sound feedback.
- No undo.

### Persistence

- Persist the full current game state locally.
- Use app-private storage.
- JSON persistence is preferred.
- Suggested file name: `game-state.json`.
- Use `System.Text.Json`.
- Load persisted state at startup.
- If no saved state exists, start in identity-selection mode.
- Save after every state-changing action.
- Save again during lifecycle events such as `OnPause`.
- Include a simple state version field for future migration.

## Architecture to Implement

Follow `docs/architecture.md`.

Recommended structure:

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

### MainActivity Responsibilities

- Own Android lifecycle.
- Lock/handle landscape behavior according to project conventions.
- Keep screen awake while visible.
- Load persisted game state.
- Render identity-selection mode and in-game mode.
- Wire button clicks to `GameService`.
- Show dialogs/errors/popups.
- Save state after changes and on lifecycle transitions.

Avoid putting game-rule calculations directly in `MainActivity`.

### GameService Responsibilities

Keep this Android-independent.

Implement logic for:

- initial state creation
- identity selection
- duplicate identity validation
- game start/reset
- first random starting player
- later alternating starting player
- HP/Mastery/crystals/damage adjustments
- clamping rules
- end-turn behavior
- winner detection
- transition to identity selection after game end/new game

### GameStateStore Responsibilities

- Save/load `GameState` as JSON.
- Use app-private storage path from Android context, but keep persistence logic isolated.
- If loading fails or no file exists, return a fresh initial state.

## Implementation Constraints

- Make minimal, focused changes.
- Follow existing project style.
- Keep game rules testable later by keeping them out of Android UI code.
- Do not add external packages unless clearly necessary.
- Do not introduce MAUI.
- Do not introduce cross-platform abstractions.
- Do not add automated tests yet.
- Validate with a build after implementation.

## Suggested Implementation Order

1. Add model classes.
2. Add `GameService`.
3. Add `GameStateStore`.
4. Update manifest/activity for landscape and keep-awake behavior.
5. Implement `activity_main.xml` with identity-selection and in-game sections.
6. Update `MainActivity` to load/render/save state and wire interactions.
7. Add confirmation, duplicate identity error, and winner popup flows.
8. Build the solution and fix compile errors.

## Manual Validation Checklist

After implementation, manually verify:

- First launch shows identity selection.
- Identity buttons show correct names/colors.
- Duplicate identities show an error.
- Different identities start a game.
- HP starts at 50 and clamps 0..50.
- Mastery starts at 0 and clamps 0..30.
- Crystals and damage start at 0 and cannot go negative.
- `End Turn` applies damage to opponent HP.
- `End Turn` resets crystals/damage and switches active player if no winner.
- Active player is highlighted.
- Winner popup appears at 0 HP with correct identity name.
- Closing winner popup switches to identity selection.
- `New Game` asks for confirmation.
- Cancelled `New Game` leaves state unchanged.
- Confirmed `New Game` switches to identity selection.
- State is restored after app close/background/reopen.
- Screen stays awake during use.
- Landscape layout is usable on a table between two players.
