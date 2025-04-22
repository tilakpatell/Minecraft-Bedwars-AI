using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class PlaceEndstone : ICommand
{
    private BedIsland _targetIsland;
    
    // Duration scales with the number of blocks
    public int Duration => CalculateDuration();
    
    public PlaceEndstone(BedIsland targetIsland)
    {
        _targetIsland = targetIsland;
    }
    
    private int CalculateDuration()
    {
        int currentLayers = _targetIsland.DefenseLayers.Count;
        int blocksNeeded = GetBlocksNeededForLayer(currentLayers + 1);
        
        // Base duration of 1 tick per 4 blocks, minimum 4 ticks
        return Math.Max(2, blocksNeeded / 5);
    }
    
    public void Execute(Player player)
    {
        int currentLayers = _targetIsland.DefenseLayers.Count;
        int blocksNeeded = GetBlocksNeededForLayer(currentLayers + 1);
    
        // Check if player has enough endstone blocks
        if (player.Inventory.HasItems<Endstone>(blocksNeeded))
        {
            // Remove the blocks from the inventory
            player.Inventory.RemoveItems<Endstone>(blocksNeeded);
        
            // Add a defense layer
            _targetIsland.AddLayer(new Endstone());
        
            Console.WriteLine($"Player {player.getColor()} added an endstone defense layer to {_targetIsland.GetColor()}'s bed (used {blocksNeeded} endstone blocks)");
        }
        else
        {
            Console.WriteLine($"Player {player.getColor()} does not have enough endstone blocks to add a defense layer to {_targetIsland.GetColor()}'s bed");
        }
    }
    
    private int GetBlocksNeededForLayer(int layerNumber)
    {
        // Return blocks needed based on layer number
        return layerNumber switch
        {
            1 => 8,   // 1st layer = 8 blocks
            2 => 18,  // 2nd layer = 18 blocks
            3 => 32,  // 3rd layer = 32 blocks
            4 => 50,  // 4th layer = 50 blocks
            _ => 50   // Any additional layers also need 50 blocks
        };
    }
}