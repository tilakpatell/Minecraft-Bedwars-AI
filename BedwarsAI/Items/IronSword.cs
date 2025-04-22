namespace BedwarsAI.Items;

public class IronSword(int damage = 6) : Sword
{
    public string Material { get; } = "Iron";
    public Money Cost { get; } = new Gold(7);
    public int Damage { get; set; } = damage;

    public void AddSharpness()
    {
        Damage += 2;
    }
}