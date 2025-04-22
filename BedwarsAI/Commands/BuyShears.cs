using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyShears : ICommand
{
    public int Duration => 1;
    public void Execute(Player player)
    {
        if (Shop.BuyItem(player, new Shears()))
        {
            Console.WriteLine("You bought shears");
        }
        else
        {
            Console.WriteLine("Not enough to buy shears");
        }
    } 
}