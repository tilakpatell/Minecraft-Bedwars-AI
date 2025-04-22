using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyObsidian : ICommand
{
    private int _amount;
    
    public int Duration { get; } = 1;
    
    public BuyObsidian(int amount = 4)
    {
        _amount = amount;
    }
    
    public void Execute(Player player)
    {
        var obsidian = new Obsidian();
        
        // Calculate total cost
        var totalCost = obsidian.Cost;
        if (obsidian.Cost is Emerald emeraldCost)
        {
            totalCost = new Emerald(emeraldCost.Count * _amount);
        }
        
        // Check if player can afford the total cost
        if (player.Inventory.hasEnoughMoney(totalCost))
        {
            player.Inventory.SubtractMoney(totalCost);
            for (int i = 0; i < _amount; i++)
            {
                player.Inventory.AddItem(new Obsidian());
            }
        }
    }
}