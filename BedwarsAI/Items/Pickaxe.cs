namespace BedwarsAI.Items;

public interface Pickaxe : Item
{
    int Power { get; set; }
    string Material { get; }
    Money Cost { get; }
    
    void AddManiacMiner();
}