namespace BedwarsAI.Commands;

public class Move : ICommand
{
    public readonly AIsland _from;
    public readonly AIsland _to;
    public readonly Connection _connection;
    public readonly Player _player;

    public Move(AIsland from, AIsland to, Connection connection, Player player)
    {
        _from = from;
        _to = to;
        _connection = connection;
        _player = player;
    }

    public int Duration => 1; // One tick per movement step

    public void Execute(Player player)
    {
        if (!_connection.IsComplete())
        {
            Console.WriteLine("Can't move, bridge incomplete!");
            return;
        }

        Console.WriteLine($"{_player.Color} moved from {_from} to {_to}.");
        _player.CurrentIsland = _to;
    }
}
