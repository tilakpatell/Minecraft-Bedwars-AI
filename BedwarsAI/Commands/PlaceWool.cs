using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class PlaceWool : ICommand
{
    private BedIsland _targetIsland;
    
    // Duration now scales with the number of blocks needed for the layer
    public int Duration => CalculateDuration();
    
    public PlaceWool(BedIsland targetIsland)
    {
        _targetIsland = targetIsland;
    }
    
    private int CalculateDuration()
    {
        int currentLayers = _targetIsland.DefenseLayers.Count;
        int blocksNeeded = GetBlocksNeededForLayer(currentLayers + 1);
        
        // Base duration of 1 tick per 4 blocks, minimum 2 ticks
        return Math.Max(2, blocksNeeded / 5);
    }
    
    public void Execute(Player player)
    {
        int currentLayers = _targetIsland.DefenseLayers.Count;
        int blocksNeeded = GetBlocksNeededForLayer(currentLayers + 1);
    
        // Check if player has enough wool blocks
        if (player.Inventory.HasItems<Wool>(blocksNeeded))
        {
            // Remove the blocks from the inventory
            player.Inventory.RemoveItems<Wool>(blocksNeeded);
        
            // Add a defense layer
            _targetIsland.AddLayer(new Wool());
        
            Console.WriteLine($"Player {player.getColor()} added a wool defense layer to {_targetIsland.GetColor()}'s bed (used {blocksNeeded} wool blocks)");
        }
        else
        {
            Console.WriteLine($"Player {player.getColor()} does not have enough wool blocks to add a defense layer to {_targetIsland.GetColor()}'s bed");
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