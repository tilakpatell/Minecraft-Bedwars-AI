using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyIronPickaxe : ICommand
{
    public int Duration { get; } = 1;
    
    public void Execute(Player player)
    {
        var pickaxe = new IronPickaxe();
        if (Shop.BuyItem(player, pickaxe))
        {
            player.Pickaxe = pickaxe;
            Console.WriteLine("Bought Iron Pickaxe");
        }
        else
        {
            Console.WriteLine("Cannot buy Iron Pickaxe.");
        }
    }
}