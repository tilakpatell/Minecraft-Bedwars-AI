using BedwarsAI.Items;

namespace BedwarsAI;

public class EmeraldIsland : AIsland
{
    private Generator _emeraldGenerator;
    
    public EmeraldIsland(Dictionary<AIsland, Connection> neighbors = null, int spawnRate = 240) : base(neighbors ?? new Dictionary<AIsland, Connection>())
    {
        // Emerald generator - slower than diamond
        _emeraldGenerator = new Generator(new Emerald(0), spawnRate);
    }
    
    public void Tick()
    {
        _emeraldGenerator.Tick();
    }
    
    public Generator GetGenerator() => _emeraldGenerator;
    
    public Money CollectResources()
    {
        return _emeraldGenerator.Collect();
    }
    
    public int GetResourcesAvailable()
    {
        return _emeraldGenerator.GetAccumulated();
    }
}