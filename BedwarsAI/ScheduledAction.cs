using BedwarsAI.Commands;

namespace BedwarsAI;

public class ScheduledAction
{
    private readonly ICommand _command;
    private int _remainingTicks;

    public ScheduledAction(ICommand command)
    {
        _command = command;
        _remainingTicks = command.Duration;
    }

    public bool Tick(Player player)
    {
        _command.OnTick(player); // called every tick
        _remainingTicks--;

        if (_remainingTicks <= 0)
        {
            _command.Execute(player);
            return true;
        }

        return false;
    }
}