using BedwarsAI.Items;

namespace BedwarsAI;

public enum Color
{
    Red,
    Blue,
    Green,
    Yellow,
    Aqua,
    White,
    Pink,
    Gray
}

public class BedIsland : AIsland
{
    private Color Color;
    private bool HasBed = true;
    public Stack<Block> DefenseLayers = new Stack<Block>();
    private Generator _ironGenerator;
    private Generator _goldGenerator;
    
    public BedIsland(Color color, int ironSpeed = 10, Dictionary<AIsland, Connection> neighbors = null) : base(neighbors ?? new Dictionary<AIsland, Connection>())
    {
        this.Color = color;
        _ironGenerator = new Generator(new Iron(0), ironSpeed);
        _goldGenerator = new Generator(new Gold(0), ironSpeed * 6); // Gold 6x slower than iron
    }

    public BedIsland(Color color, int ironSpeed = 10, int goldSpeed = 60, Dictionary<AIsland, Connection> neighbors = null) : base(neighbors ?? new Dictionary<AIsland, Connection>())
    {
        this.Color = color;
        _ironGenerator = new Generator(new Iron(0), ironSpeed);
        _goldGenerator = new Generator(new Gold(0), goldSpeed);
    }
    
    public void Tick()
    {
        _ironGenerator.Tick();
        _goldGenerator.Tick();
    }
    
    public Generator GetIronGenerator() => _ironGenerator;
    public Generator GetGoldGenerator() => _goldGenerator;
    
    public void DestroyBed()
    {
        HasBed = false;
        Console.WriteLine($"{Color} bed was destroyed");
    }
    
    public bool IsBedAlive()
    {
        return HasBed;
    }
    
    public Color GetColor()
    {
        return Color;
    }

    public void DestroyLayer()
    {
        if (DefenseLayers.Count > 0)
        {
            DefenseLayers.Pop();
        }
        else
        {
            DestroyBed();
        }
    }

    public void AddLayer(Block block)
    {
        DefenseLayers.Push(block);
        
    }

    public Block PeekTopLayer()
    {
        return DefenseLayers.Peek();
    }

    public bool HasDefense()
    {
        return DefenseLayers.Count > 0;
    }
}