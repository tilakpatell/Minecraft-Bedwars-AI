using BedwarsAI.Items;

namespace BedwarsAI;

public class Generator
{
    private readonly int _spawnRate;
    private readonly int _maxAccumulated;
    private int _accumulated;
    private int _ticksSinceLastSpawn;
    private readonly Money _resourceType;
    private readonly string _resourceName;

    public Generator(Money resourceType, int spawnRate, int maxAccumulated = 8)
    {
        _resourceType = resourceType;
        _spawnRate = spawnRate;
        _maxAccumulated = maxAccumulated;
        _accumulated = 0;
        _ticksSinceLastSpawn = 0;
        
        // Set resource name for logging
        if (resourceType is Iron) _resourceName = "Iron";
        else if (resourceType is Gold) _resourceName = "Gold";
        else if (resourceType is Diamond) _resourceName = "Diamond";
        else if (resourceType is Emerald) _resourceName = "Emerald";
        else _resourceName = "Unknown";
    }

    public void Tick()
    {
        _ticksSinceLastSpawn++;
        
        if (_ticksSinceLastSpawn >= _spawnRate && _accumulated < _maxAccumulated)
        {
            _accumulated++;
            _ticksSinceLastSpawn = 0;
            // Console.WriteLine($"Generated 1 {_resourceName}. Total: {_accumulated}");
        }
    }

    public Money Collect()
    {
        if (_accumulated <= 0)
        {
 
            return _resourceType switch
            {
                Iron => new Iron(0),
                Gold => new Gold(0),
                Diamond => new Diamond(0),
                Emerald => new Emerald(0),
                _ => throw new NotSupportedException("Unknown resource type")
            };
        }

        var amount = _accumulated;
        _accumulated = 0;
        
        // Create a new instance of the resource with the collected amount
        if (_resourceType is Iron) return new Iron(amount);
        if (_resourceType is Gold) return new Gold(amount);
        if (_resourceType is Diamond) return new Diamond(amount);
        if (_resourceType is Emerald) return new Emerald(amount);
            
        return null;
    }

    public int GetAccumulated()
    {
        return _accumulated;
    }
}