using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyDiamondSword : ICommand
{
    public int Duration => 1;
    public void Execute(Player player)
    {
        var pickaxe = new DiamondPickaxe();
        if (Shop.BuyItem(player, pickaxe))
        {
            player.Pickaxe = pickaxe;
            Console.WriteLine("You bought a diamond sword.");
        }
        else
        {
            Console.WriteLine("Not enough to buy a diamond sword.");
        }
    } 
}