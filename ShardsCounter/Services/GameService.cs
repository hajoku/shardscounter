using ShardsCounter.Models;
using GameState = ShardsCounter.Models.GameState;

namespace ShardsCounter.Services;

public sealed class GameService
{
    public const int StartingHealthPoints = 50;
    public const int MinimumHealthPoints = 0;
    public const int MaximumHealthPoints = 50;
    public const int StartingMastery = 0;
    public const int MinimumMastery = 0;
    public const int MaximumMastery = 30;

    public static readonly IReadOnlyList<GameIdentity> Identities =
    [
        new() { Name = "Tetra", Color = "#1E88E5" },
        new() { Name = "Decima", Color = "#FDD835" },
        new() { Name = "Volos", Color = "#43A047" },
        new() { Name = "Ko Syn Wu", Color = "#8E24AA" },
        new() { Name = "Rez", Color = "#E53935" }
    ];

    public GameState CreateInitialState() => new()
    {
        Phase = GamePhase.IdentitySelection,
        Player1 = CreatePlayerState(),
        Player2 = CreatePlayerState()
    };

    public void SelectIdentity(GameState state, PlayerSlot slot, string identityName)
    {
        if (!IdentityExists(identityName))
        {
            return;
        }

        if (slot == PlayerSlot.Player1)
        {
            state.PendingPlayer1IdentityName = identityName;
        }
        else
        {
            state.PendingPlayer2IdentityName = identityName;
        }
    }

    public bool TryStartGame(GameState state, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(state.PendingPlayer1IdentityName) || string.IsNullOrWhiteSpace(state.PendingPlayer2IdentityName))
        {
            return false;
        }

        if (state.PendingPlayer1IdentityName == state.PendingPlayer2IdentityName)
        {
            errorMessage = "Both players cannot choose the same identity.";
            return false;
        }

        var startingPlayer = GetNextStartingPlayer(state);

        state.Player1 = CreatePlayerState(state.PendingPlayer1IdentityName);
        state.Player2 = CreatePlayerState(state.PendingPlayer2IdentityName);
        state.CurrentStartingPlayer = startingPlayer;
        state.PreviousStartingPlayer = startingPlayer;
        state.ActivePlayer = startingPlayer;
        state.CurrentTurnCrystals = 0;
        state.CurrentTurnDamage = 0;
        state.Phase = GamePhase.InGame;
        state.HasCreatedFirstGame = true;

        return true;
    }

    public void MoveToIdentitySelection(GameState state)
    {
        state.Phase = GamePhase.IdentitySelection;
        state.PendingPlayer1IdentityName = null;
        state.PendingPlayer2IdentityName = null;
    }

    public void AdjustHealth(GameState state, PlayerSlot slot, int delta)
    {
        var player = GetPlayer(state, slot);
        player.HealthPoints = Clamp(player.HealthPoints + delta, MinimumHealthPoints, MaximumHealthPoints);
    }

    public void AdjustMastery(GameState state, PlayerSlot slot, int delta)
    {
        var player = GetPlayer(state, slot);
        player.Mastery = Clamp(player.Mastery + delta, MinimumMastery, MaximumMastery);
    }

    public void AdjustCrystals(GameState state, int delta)
    {
        state.CurrentTurnCrystals = Math.Max(0, state.CurrentTurnCrystals + delta);
    }

    public void AdjustDamage(GameState state, int delta)
    {
        state.CurrentTurnDamage = Math.Max(0, state.CurrentTurnDamage + delta);
    }

    public PlayerSlot? EndTurn(GameState state)
    {
        var opponentSlot = GetOpponent(state.ActivePlayer);
        var opponent = GetPlayer(state, opponentSlot);
        opponent.HealthPoints = Math.Max(MinimumHealthPoints, opponent.HealthPoints - state.CurrentTurnDamage);

        if (opponent.HealthPoints == MinimumHealthPoints)
        {
            return state.ActivePlayer;
        }

        state.CurrentTurnCrystals = 0;
        state.CurrentTurnDamage = 0;
        state.ActivePlayer = opponentSlot;
        return null;
    }

    public GameIdentity? GetIdentity(string? identityName)
    {
        return Identities.FirstOrDefault(identity => identity.Name == identityName);
    }

    public PlayerState GetPlayer(GameState state, PlayerSlot slot)
    {
        return slot == PlayerSlot.Player1 ? state.Player1 : state.Player2;
    }

    public PlayerSlot GetOpponent(PlayerSlot slot)
    {
        return slot == PlayerSlot.Player1 ? PlayerSlot.Player2 : PlayerSlot.Player1;
    }

    private static PlayerState CreatePlayerState(string? identityName = null) => new()
    {
        IdentityName = identityName,
        HealthPoints = StartingHealthPoints,
        Mastery = StartingMastery
    };

    private static PlayerSlot GetNextStartingPlayer(GameState state)
    {
        if (!state.HasCreatedFirstGame || state.PreviousStartingPlayer is null)
        {
            return Random.Shared.Next(2) == 0 ? PlayerSlot.Player1 : PlayerSlot.Player2;
        }

        return state.PreviousStartingPlayer == PlayerSlot.Player1 ? PlayerSlot.Player2 : PlayerSlot.Player1;
    }

    private static bool IdentityExists(string identityName)
    {
        return Identities.Any(identity => identity.Name == identityName);
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, value));
    }
}
