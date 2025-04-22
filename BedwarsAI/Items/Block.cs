namespace BedwarsAI.Items;

public abstract class Block : Item
{
    public abstract int Strength { get; }
    protected int CurrentMined;
    public abstract Money Cost { get; }

}
