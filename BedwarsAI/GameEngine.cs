namespace BedwarsAI;

public class GameEngine
{
    private GameState _gameState;
    private bool _isRunning = false;

    public GameEngine(
        List<BedIsland> bedIslands, 
        List<DiamondIsland> diamondIslands, 
        List<EmeraldIsland> emeraldIslands,
        List<Player> players)
    {
        _gameState = new GameState(bedIslands, diamondIslands, emeraldIslands, players);
    }

    public void Tick()
    {
        if (!_isRunning) return;
        
        _gameState.Tick();
        
        foreach (var player in _gameState.GetPlayers().Where(p => p.getIsAlive()))
        {
            player.Tick(_gameState);
        }
        
        if (_gameState.IsGameOver())
        {
            _isRunning = false;
            var winner = _gameState.GetWinner();
            if (winner != null)
            {
                Console.WriteLine($"Game over! Winner: {winner.getColor()}");
            }
            else
            {
                Console.WriteLine("Game over! No winner.");
            }
        }
    }
    
    public void StartGame()
    {
        _isRunning = true;
        Console.WriteLine("Game started!");
    }
    
    public void StopGame()
    {
        _isRunning = false;
        Console.WriteLine("Game stopped!");
    }
    
    public GameState GetGameState()
    {
        return _gameState;
    }
    
    public void RunMatch(int maxTicks = 10000, int tickDelayMs = 0)
    {
        StartGame();
        int tickCount = 0;

        while (_isRunning && tickCount < maxTicks)
        {
            Tick();
            tickCount++;

            if (tickDelayMs > 0)
                Thread.Sleep(tickDelayMs); // Simulate real-time ticking
        }

        if (!_isRunning)
            Console.WriteLine($"Match ended after {tickCount} ticks.");
        else
            Console.WriteLine("Max tick limit reached. Game forcibly stopped.");
    }

}