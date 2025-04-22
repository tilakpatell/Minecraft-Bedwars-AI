namespace BedwarsAI.Items;

public class WoodenPickaxe(int power = 1) : Pickaxe
{
    public int Power { get; set; } = power;
    public string Material { get; } = "Wooden";
    public Money Cost { get; } = new Iron(10);    
    
    public void AddManiacMiner()
    {
        this.Power += 2;
    }
    
    
}