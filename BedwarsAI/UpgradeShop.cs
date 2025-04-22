namespace BedwarsAI;
using BedwarsAI.Items;

public class UpgradeShop
{
    public bool Sharpness;
    public bool ManiacMiner;
    public Diamond SharpnessCost = new Diamond(4);
    public Diamond ManiacMinerCost = new Diamond(1);

    public void BuySharpness(Player player)
    {
        if (player.Inventory.hasEnoughMoney(SharpnessCost) && !Sharpness)
        {
            Sharpness = true;
            player.Sword.AddSharpness();
        }
    }

    public void BuyManiacMiner(Player player)
    {
        if (player.Inventory.hasEnoughMoney(ManiacMinerCost) && !ManiacMiner)
        {
            ManiacMiner = true;
            player.Pickaxe.AddManiacMiner();
        }
        else
        {
            Console.WriteLine("You don't have enough for maniac miner or already have it!");
        }
    }

}