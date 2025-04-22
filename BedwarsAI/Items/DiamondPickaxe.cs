namespace BedwarsAI.Items;

public class DiamondPickaxe(int power = 8) : Pickaxe
{
    public int Power { get; set; } = power;
    public string Material { get; } = "Diamond";
    public Money Cost { get; } = new Gold(20);
    
    public void AddManiacMiner()
    {
        this.Power += 2;
    }
}