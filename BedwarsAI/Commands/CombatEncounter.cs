using BedwarsAI.Commands;

namespace BedwarsAI;

public class CombatEncounter : ICommand
{
    public readonly Player _attacker;
    public readonly Player _defender;
    public readonly bool _onBridge;
    private bool _firstTick = true;
    private static Random _rng = new();

    public CombatEncounter(Player attacker, Player defender, bool onBridge)
    {
        _attacker = attacker;
        _defender = defender;
        _onBridge = onBridge;
    }

    public int Duration => 999; // effectively infinite; can be cut off externally

    public void OnTick(Player player)
    {
        if (!_attacker.getIsAlive() || !_defender.getIsAlive())
            return;

        if (_firstTick && _onBridge)
        {
            Player crit = _attacker.BridgingSkill >= _defender.BridgingSkill ? _attacker : _defender;
            Player victim = crit == _attacker ? _defender : _attacker;
            victim.PlayerHealth -= 2;
            Console.WriteLine($"{crit.Color} landed a critical hit on bridge!");
            _firstTick = false;
        }

        ApplyHit(_attacker, _defender);
        if (_defender.getIsAlive())
            ApplyHit(_defender, _attacker);
    }

    public void Execute(Player player)
    {
        // Optionally log or finalize combat
    }

    private void ApplyHit(Player attacker, Player defender)
    {
        int total = attacker.CombatSkill + defender.CombatSkill;
        if (total == 0) return;

        int roll = _rng.Next(total);
        if (roll < attacker.CombatSkill)
        {
            defender.PlayerHealth -= 1;
            Console.WriteLine($"{attacker.Color} hits {defender.Color}! Remaining HP: {defender.PlayerHealth}");

            if (defender.PlayerHealth <= 0)
            {
                defender.setIsAlive(false);
                Console.WriteLine($"{defender.Color} has died.");
            }
        }
    }
}