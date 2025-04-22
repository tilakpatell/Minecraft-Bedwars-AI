using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyStoneSword : ICommand
{
    public int Duration => 1;
    public void Execute(Player player)
    {
        if (Shop.BuyItem(player, new StoneSword()))
        {
            Console.WriteLine("You bought a diamond sword.");
        }
        else
        {
            Console.WriteLine("Not enough to buy a diamond sword.");
        }
    } 
}