using BedwarsAI.Items;

namespace BedwarsAI.Commands;

public class UseGoldenApple : ICommand
{
    private Player player;
    public int Duration => 1;
    public void Execute(Player player)
    {
        if (player.Inventory.HasItems<GoldenApple>(1))
        {
            player.Inventory.RemoveItems<GoldenApple>(1);
            int overallHealth = player.getPlayerHealth() + 6;
            player.setPlayerHealth(overallHealth);
        }
        else
        {
            Console.WriteLine("You do not have enough GoldenApple to use this command.");
        }
    }
}