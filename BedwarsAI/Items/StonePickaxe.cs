namespace BedwarsAI.Items;

public class StonePickaxe(int power = 2) : Pickaxe
{
    public int Power { get; set; } = power;
    public string Material { get; } = "Stone";
    public Money Cost { get; } = new Gold(20);
    
    public void AddManiacMiner()
    {
        this.Power += 2;
    }
}