namespace BedwarsAI;

public abstract class AIsland
{
    private Dictionary<AIsland, Connection> Neighbors = new Dictionary<AIsland, Connection>();

    public AIsland(Dictionary<AIsland, Connection> neighbors)
    {
        this.Neighbors = neighbors;
    }
    
    public Dictionary<AIsland, Connection> GetNeighbors()
    {
        return Neighbors;
    }
}