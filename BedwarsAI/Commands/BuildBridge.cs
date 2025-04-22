namespace BedwarsAI.Commands;

public class BuildBridge : ICommand
{
    private readonly Connection _connection;
    private readonly Player _player;

    public BuildBridge(Connection connection, Player player)
    {
        _connection = connection;
        _player = player;
    }

    public int Duration => 1; // One tick per bridge segment

    public void Execute(Player player)
    {
        _connection.Extend(_player.BridgingSkill);
        Console.WriteLine($"{_player.Color} extended bridge by {_player.BridgingSkill} units.");
    }
}
