using BedwarsAI.Items;

namespace BedwarsAI;

public static class Shop
{
    private static bool CanAfford(Player player, Item item)
    {
        return player.Inventory.hasEnoughMoney(item.Cost);
    }

    public static bool BuyItem(Player player, Item item)
    {
        if (CanAfford(player, item))
        {
            player.Inventory.AddItem(item);
            player.Inventory.SubtractMoney(item.Cost);
            return true;
        }

        return false;
    }
}