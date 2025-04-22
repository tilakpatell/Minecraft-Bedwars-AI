using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class BuyGoldenApple : ICommand
{
    private int _amount;
    
    public int Duration { get; } = 1;
    
    public BuyGoldenApple(int amount = 1)
    {
        _amount = amount;
    }
    
    public void Execute(Player player)
    {
        var goldenApple = new GoldenApple();
        
        // Calculate total cost
        var totalCost = goldenApple.Cost;
        if (goldenApple.Cost is Gold goldCost)
        {
            totalCost = new Gold(goldCost.Count * _amount);
        }
        
        // Check if player can afford the total cost
        if (player.Inventory.hasEnoughMoney(totalCost))
        {
            player.Inventory.SubtractMoney(totalCost);
            for (int i = 0; i < _amount; i++)
            {
                player.Inventory.AddItem(new GoldenApple());
            }
        }
    }
}