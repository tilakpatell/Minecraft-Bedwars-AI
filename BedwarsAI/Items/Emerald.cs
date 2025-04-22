namespace BedwarsAI.Items;

public class Emerald(int count) : Money
{
    public int Count { get; set;  } = count;
}