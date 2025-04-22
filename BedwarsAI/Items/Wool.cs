namespace BedwarsAI.Items;

public class Wool() : Block 
{ 
    public override int Strength => 2;
    public override Money Cost => new Iron(4);
    
    public bool Mine(Shears shears)
    {
        return true;
    }

    public bool Mine()
    {
        if (CurrentMined >= Strength)
        {
            return true;
        }

        CurrentMined += 1;
        return false; 
    }
    
}