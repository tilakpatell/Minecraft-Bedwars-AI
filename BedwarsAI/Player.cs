using BedwarsAI.Commands;
using BedwarsAI.Items;

namespace BedwarsAI;

public class Player(int combatSkill = 0, int bridgingSkill = 0, BedIsland home = null) 
{
    public Pickaxe Pickaxe;
    private Shears _shears;
    public int CombatSkill = combatSkill;
    public Sword Sword;
    public int BridgingSkill = bridgingSkill;
    public Inventory Inventory = new Inventory();
    public BedIsland HomeIsland = home;
    public Color Color;
    public UpgradeShop _upgrades = new UpgradeShop();
    private bool IsAlive = true;
    public int PlayerHealth = 20;
    public AIsland CurrentIsland = home;
    private int _enemyBedsDestroyed = 0;
    
    public void SetHomeIsland(BedIsland island)
    {
        HomeIsland = island;
    }
    
    public void RecordBedDestroyed(BedIsland island) 
    {
        if (island.GetColor() != this.Color)
        {
            _enemyBedsDestroyed++;
            Console.WriteLine($"{this.Color} destroyed {island.GetColor()}'s bed! Total beds destroyed: {_enemyBedsDestroyed}");
        }
    }

    public int GetEnemyBedsDestroyed()
    {
        return _enemyBedsDestroyed;
    }

    public int getPlayerHealth()
    {
        return PlayerHealth;
    }

    public void setPlayerHealth(int health)
    {
        PlayerHealth = health;
    }

    public bool getIsAlive()
    {
        return IsAlive;
    }

    public bool setIsAlive(bool isAlive)
    { 
        return this.IsAlive = isAlive;
    }

    public void SetColor()
    {
        this.Color = HomeIsland.GetColor();
    }

    public Color getColor()
    {
        return this.Color;
    }
    
    public BedIsland GetHomeIsland()
    {
        return HomeIsland;
    }
    
    public bool HasBed()
    {
        return HomeIsland.IsBedAlive();
    }
    
    private ScheduledAction? currentAction;
    
    public virtual ICommand DecideNextAction(GameState gameState)
    {
        return new BedwarsAIAgent(this, gameState).GetOptimalAction();
    }

    public void Tick(GameState gameState)
    {
        if (!getIsAlive()) return;

        if (currentAction != null)
        {
            bool finished = currentAction.Tick(this);
            if (finished) currentAction = null;
            return;
        }

        ICommand nextCommand = DecideNextAction(gameState);
        currentAction = new ScheduledAction(nextCommand);
    }

    
}