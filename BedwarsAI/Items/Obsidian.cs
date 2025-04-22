namespace BedwarsAI.Items;

public class Obsidian : Block
{
    public override int Strength => 20;
    public override Money Cost => new Emerald(4);
}