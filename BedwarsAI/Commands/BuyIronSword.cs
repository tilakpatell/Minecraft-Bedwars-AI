using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyIronSword : ICommand
{
    public int Duration => 1;
    public void Execute(Player player)
    {
        var ironsword = new IronSword();
        if (Shop.BuyItem(player, ironsword))
        {
            player.Sword = ironsword;
            Console.WriteLine("You bought an iron sword.");
        }
        else
        {
            Console.WriteLine("Not enough to buy an iron sword.");
        }
    }
    
    public void OnTick(Player player) { }
}