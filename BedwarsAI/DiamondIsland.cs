using BedwarsAI.Items;

namespace BedwarsAI;

public class DiamondIsland : AIsland
{
    private Generator _diamondGenerator;
    
    public DiamondIsland(Dictionary<AIsland, Connection> neighbors = null, int spawnRate = 120) : base(neighbors ?? new Dictionary<AIsland, Connection>())
    {
        _diamondGenerator = new Generator(new Diamond(0), spawnRate);
    }
    
    public void Tick()
    {
        _diamondGenerator.Tick();
    }
    
    public Generator GetGenerator() => _diamondGenerator;
    
    public Money CollectResources()
    {
        return _diamondGenerator.Collect();
    }
    
    public int GetResourcesAvailable()
    {
        return _diamondGenerator.GetAccumulated();
    }
}