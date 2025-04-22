namespace BedwarsAI.Items;

public class Diamond(int count) : Money
{
    public int Count { get; set; } = count;
}