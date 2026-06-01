namespace ShardsCounter.Models;

public sealed class GameState
{
    public int StateVersion { get; set; } = 1;

    public GamePhase Phase { get; set; } = GamePhase.IdentitySelection;

    public PlayerState Player1 { get; set; } = new();

    public PlayerState Player2 { get; set; } = new();

    public PlayerSlot ActivePlayer { get; set; } = PlayerSlot.Player1;

    public PlayerSlot CurrentStartingPlayer { get; set; } = PlayerSlot.Player1;

    public PlayerSlot? PreviousStartingPlayer { get; set; }

    public int CurrentTurnCrystals { get; set; }

    public int CurrentTurnDamage { get; set; }

    public string? PendingPlayer1IdentityName { get; set; }

    public string? PendingPlayer2IdentityName { get; set; }

    public bool HasCreatedFirstGame { get; set; }
}
