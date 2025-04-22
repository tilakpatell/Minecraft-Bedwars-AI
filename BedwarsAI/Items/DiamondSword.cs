namespace BedwarsAI.Items;

public class DiamondSword(int damage = 7) : Sword
{
    public string Material { get; } = "Diamond";
    public Money Cost { get; } = new Emerald(3);
    public int Damage { get; set; } = damage;

    public void AddSharpness()
    {
        Damage += 2;
    }
}