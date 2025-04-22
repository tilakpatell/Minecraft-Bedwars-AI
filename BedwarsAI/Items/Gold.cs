namespace BedwarsAI.Items;

public class Gold(int count) : Money
{
    public int Count { get; set; } = count;
}