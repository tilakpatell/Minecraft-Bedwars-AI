using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyWoodenPickaxe : ICommand
{
    public int Duration { get; } = 1;
    
    public void Execute(Player player)
    {
        var pickaxe = new WoodenPickaxe();
        if (Shop.BuyItem(player, pickaxe))
        {
            player.Pickaxe = pickaxe;
        }
    }
}