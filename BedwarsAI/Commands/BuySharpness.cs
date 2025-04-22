using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuySharpness : ICommand
{
    public int Duration { get; } = 1;
    
    public void Execute(Player player)
    {
        player._upgrades.BuySharpness(player);
    }
}