using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyDiamondPickaxe : ICommand
{
   public int Duration => 1;
   public void Execute(Player player)
   {
      if (Shop.BuyItem(player, new DiamondPickaxe()))
      {
         Console.WriteLine("You bought a diamond pickaxe.");
      }
      else
      {
         Console.WriteLine("Not enough to buy a diamond pickaxe.");
      }
   }
}