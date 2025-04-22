using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyEndstone : ICommand
{
    private int _amount; 
    
    public int Duration { get; } = 1;
    
    public BuyEndstone(int amount = 12)
    {
        _amount = amount;  
    }
    
    public void Execute(Player player)
    {
        var endstone = new Endstone();
        
        // Calculate total cost for the amount
        var totalCost = endstone.Cost;
        
        // Assuming the Cost is Iron
        if (endstone.Cost is Iron ironCost)
        {
            // Create new cost with amount multiplied
            totalCost = new Iron(ironCost.Count * _amount);
        }
        
        // Check if player can afford the total cost
        if (player.Inventory.hasEnoughMoney(totalCost))
        {
            player.Inventory.SubtractMoney(totalCost);
            for (int i = 0; i < _amount; i++)
            {
                player.Inventory.AddItem(new Endstone());
            }
        }
    }
}

