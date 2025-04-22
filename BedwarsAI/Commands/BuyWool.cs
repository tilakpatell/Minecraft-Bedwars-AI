using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyWool : ICommand
{
    private int _amount;
    
    public int Duration { get; } = 1;
    
    public BuyWool(int amount = 16)
    {
        _amount = amount;
    }
    
    public void Execute(Player player)
    {
        var wool = new Wool();
        
        // Calculate total cost
        var totalCost = wool.Cost;
        if (wool.Cost is Iron ironCost)
        {
            totalCost = new Iron(ironCost.Count * _amount);
        }
        
        // Check if player can afford the total cost
        if (player.Inventory.hasEnoughMoney(totalCost))
        {
            player.Inventory.SubtractMoney(totalCost);
            for (int i = 0; i < _amount; i++)
            {
                player.Inventory.AddItem(new Wool());
            }
        }
    }
}