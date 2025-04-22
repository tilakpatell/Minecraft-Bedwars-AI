namespace BedwarsAI;

public class GameState
{
    private int _currentTick;
    private readonly List<BedIsland> _bedIslands;
    private readonly List<DiamondIsland> _diamondIslands;
    private readonly List<EmeraldIsland> _emeraldIslands;
    private readonly List<Player> _players;
    private const int BED_BREAK_TIME = 250000;
    private bool _allBedsDestroyed = false;

    public GameState(
        List<BedIsland> bedIslands,
        List<DiamondIsland> diamondIslands,
        List<EmeraldIsland> emeraldIslands,
        List<Player> players)
    {
        _currentTick = 0;
        _bedIslands = bedIslands;
        _diamondIslands = diamondIslands;
        _emeraldIslands = emeraldIslands;
        _players = players;
    }

    public int GetCurrentTick() => _currentTick;

    public void Tick()
    {
        _currentTick++;

        if (_currentTick == BED_BREAK_TIME)
        {
            BreakAllBeds();
            _allBedsDestroyed = true;
        }

        foreach (var island in _bedIslands)
        {
            island.Tick();
        }

        foreach (var island in _diamondIslands)
        {
            island.GetGenerator().Tick();
        }

        foreach (var island in _emeraldIslands)
        {
            island.GetGenerator().Tick();
        }

        foreach (var player in _players)
        {
            if (!player.getIsAlive() && player.HasBed())
            {
                player.setIsAlive(true);
                player.PlayerHealth = 20;
                player.CurrentIsland = player.HomeIsland;
                player.Inventory.Clear();
                Console.WriteLine($"{player.getColor()} respawned at their home island.");
            }
        }

        foreach (var player in _players.Where(p => p.getIsAlive()))
        {
            var island = player.CurrentIsland;

            switch (island)
            {
                case BedIsland bedIsland when bedIsland == player.HomeIsland:
                    var iron = bedIsland.GetIronGenerator().Collect();
                    var gold = bedIsland.GetGoldGenerator().Collect();
                    if (iron != null) player.Inventory.AddMoney(iron);
                    if (gold != null) player.Inventory.AddMoney(gold);
                    break;

                case DiamondIsland diamondIsland:
                    var diamond = diamondIsland.GetGenerator().Collect();
                    if (diamond != null) player.Inventory.AddMoney(diamond);
                    break;

                case EmeraldIsland emeraldIsland:
                    var emerald = emeraldIsland.GetGenerator().Collect();
                    if (emerald != null) player.Inventory.AddMoney(emerald);
                    break;
            }
        }
    }

    private void BreakAllBeds()
    {
        Console.WriteLine("\n**** TIME LIMIT REACHED - ALL BEDS WILL BREAK ****");
        foreach (var island in _bedIslands)
        {
            if (island.IsBedAlive())
            {
                island.DestroyBed();
            }
        }

        Console.WriteLine("**** ALL BEDS HAVE BEEN DESTROYED - LAST PLAYER STANDING WINS ****\n");
    }

    public List<Player> GetPlayers() => _players;
    public List<BedIsland> GetBedIslands() => _bedIslands;
    public List<DiamondIsland> GetDiamondIslands() => _diamondIslands;
    public List<EmeraldIsland> GetEmeraldIslands() => _emeraldIslands;

    public bool IsGameOver()
    {
        var bedsAlive = _bedIslands.Count(i => i.IsBedAlive());
        var playersAlive = _players.Count(p => p.getIsAlive());

        if (_currentTick % 100 == 0)
        {
            Console.WriteLine($"\nTick {_currentTick} Status:");
            Console.WriteLine($"Beds alive: {bedsAlive}");
            Console.WriteLine($"Players alive: {playersAlive}");

            foreach (var island in _bedIslands)
            {
                Console.WriteLine($"{island.GetColor()} bed: {(island.IsBedAlive() ? "Alive" : "DESTROYED")}");
            }

            foreach (var player in _players)
            {
                Console.WriteLine(
                    $"{player.getColor()} player: {(player.getIsAlive() ? "Alive" : "DEAD")}, Has bed: {player.HasBed()}");
            }
        }

        bool gameOver;

        if (_allBedsDestroyed)
        {
            gameOver = playersAlive <= 1;
        }
        else
        {
            gameOver = (bedsAlive <= 1 && _currentTick > 500) || playersAlive <= 1;
        }

        if (gameOver)
        {
            Console.WriteLine("\n**** GAME OVER ****");
            Console.WriteLine($"Beds alive: {bedsAlive}");
            Console.WriteLine($"Players alive: {playersAlive}");
        }

        return gameOver;
    }

    public Player GetWinner()
    {
        return IsGameOver() ? _players.FirstOrDefault(p => p.getIsAlive()) : null;
    }
}