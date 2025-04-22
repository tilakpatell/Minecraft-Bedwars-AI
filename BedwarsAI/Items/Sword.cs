namespace BedwarsAI.Items;

public interface Sword : Item
{
    int Damage { get; set; }
    string Material { get; }
    Money Cost { get; }

    void AddSharpness();
}