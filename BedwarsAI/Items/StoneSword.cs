namespace BedwarsAI.Items;

public class StoneSword(int damage = 5) : Sword
{
    public string Material { get; } = "Stone";
    public Money Cost { get; } = new Iron(10);
    public int Damage { get; set; } = damage;

    public void AddSharpness()
    {
        Damage += 2;
    }
}