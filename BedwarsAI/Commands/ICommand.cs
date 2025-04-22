namespace BedwarsAI.Commands;

public interface ICommand
{
    int Duration { get; }
    void Execute(Player player);         // Called when duration reaches 0
    void OnTick(Player player) { }       
}