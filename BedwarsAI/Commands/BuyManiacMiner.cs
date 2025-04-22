using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyManiacMiner : ICommand
{
    public int Duration => 1;
    
    public void Execute(Player player)
    {
        player._upgrades.BuyManiacMiner(player);
        Console.WriteLine($"Player {player.getColor()} purchased the Maniac Miner upgrade");
    }
}