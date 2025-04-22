namespace BedwarsAI.Items;

public class IronPickaxe(int power = 4) : Pickaxe
{
    public int Power { get; set; } = power;
    public string Material { get; } = "Iron";
    public Money Cost { get; } = new Gold(12);

    public void AddManiacMiner()
    {
        this.Power += 2;
    }
    
}