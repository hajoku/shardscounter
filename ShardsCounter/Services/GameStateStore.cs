using System.Text.Json;
using Android.Content;
using ShardsCounter.Models;

namespace ShardsCounter.Services;

public sealed class GameStateStore
{
    private const string FileName = "game-state.json";
    private readonly string _filePath;
    private readonly GameService _gameService;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public GameStateStore(Context context, GameService gameService)
    {
        _filePath = Path.Combine(context.FilesDir?.AbsolutePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileName);
        _gameService = gameService;
    }

    public Models.GameState Load()
    {
        if (!File.Exists(_filePath))
        {
            return _gameService.CreateInitialState();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Models.GameState>(json, _jsonSerializerOptions) ?? _gameService.CreateInitialState();
        }
        catch
        {
            return _gameService.CreateInitialState();
        }
    }

    public void Save(Models.GameState state)
    {
        var json = JsonSerializer.Serialize(state, _jsonSerializerOptions);
        File.WriteAllText(_filePath, json);
    }
}
