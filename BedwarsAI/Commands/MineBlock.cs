using BedwarsAI.Items;
using static System.Math;

namespace BedwarsAI.Commands;

public class MineBlockCommand : ICommand
{
    public readonly BedIsland _targetIsland;
    private readonly Player _player;
    private bool _isBedDestroyed = false;

    public MineBlockCommand(BedIsland targetIsland, Player player)
    {
        _targetIsland = targetIsland;
        _player = player;
    }

    public int Duration
    {
        get
        {
            if (_targetIsland.HasDefense())
            {
                Block topBlock = _targetIsland.PeekTopLayer();
                return (int)Ceiling((double)topBlock.Strength / _player.Pickaxe.Power);
            }
            return 1;
        }
    }

    public void Execute(Player player)
    {
        if (_targetIsland.HasDefense())
        {
            Console.WriteLine($"IMPORTANT: {player.getColor()} is breaking a layer of {_targetIsland.GetColor()}'s bed defense!");
            _targetIsland.DestroyLayer();
        }
        else
        {
            bool wasBedAlive = _targetIsland.IsBedAlive();
            _targetIsland.DestroyBed();
        
            if (wasBedAlive)
            {
                player.RecordBedDestroyed(_targetIsland);
                _isBedDestroyed = true;
            }
        
            Console.WriteLine($"CRITICAL: {player.getColor()} is DESTROYING {_targetIsland.GetColor()}'s bed!");
        }
    }
    
    public void OnTick(Player player)
    {
        Console.WriteLine($"{player.getColor()} is mining {_targetIsland.GetColor()}'s bed ({Duration - 1} ticks remaining)");
    }
}