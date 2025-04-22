using System;
using System.Collections.Generic;
using System.Threading;
using BedwarsAI.Items;
using BedwarsAI.Commands;
using System.Linq;

namespace BedwarsAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Bedwars AI Simulation Test Suite...");

            int numSimulations = 3;

            Dictionary<Color, int> winsByColor = new Dictionary<Color, int>
            {
                { Color.Red, 0 }, { Color.Blue, 0 }, { Color.Green, 0 }, { Color.Yellow, 0 },
            };

            int draws = 0;

            for (int sim = 1; sim <= numSimulations; sim++)
            {
                Console.WriteLine($"\n===== Starting Simulation {sim}/{numSimulations} =====");

                var islandData = CreateIslands();

                var players = CreatePlayers(islandData);

                var engine = new GameEngine(islandData.bedIslands, islandData.diamondIslands, islandData.emeraldIslands,
                    players);

                Console.WriteLine("Running simulation...");
                engine.RunMatch(maxTicks: 1000000, tickDelayMs: 0);

                Console.WriteLine("\nFinal Bed Status:");
                foreach (var island in engine.GetGameState().GetBedIslands())
                {
                    Console.WriteLine($"{island.GetColor()}: {(island.IsBedAlive() ? "Alive" : "DESTROYED")}");
                }

                Console.WriteLine("\nFinal Player Status:");
                foreach (var player in engine.GetGameState().GetPlayers())
                {
                    Console.WriteLine(
                        $"{player.getColor()}: {(player.getIsAlive() ? "Alive" : "DEAD")}, Has Bed: {player.HasBed()}");
                }

                if (!engine.GetGameState().IsGameOver())
                {
                    Console.WriteLine("Max ticks reached, determining winner based on game state...");
                    ForceWinnerDetermination(engine.GetGameState(), winsByColor, ref draws);
                }
                else
                {
                    var winner = engine.GetGameState().GetWinner();
                    if (winner != null)
                    {
                        Color winnerColor = winner.getColor();
                        winsByColor[winnerColor]++;
                        Console.WriteLine($"Simulation {sim} winner: {winnerColor}");
                    }
                    else
                    {
                        draws++;
                        Console.WriteLine($"Simulation {sim} ended in a draw");
                    }
                }
            }

            Console.WriteLine("\n===== Simulation Results =====");
            Console.WriteLine($"Total simulations: {numSimulations}");
            Console.WriteLine(
                $"Red (Optimal) wins: {winsByColor[Color.Red]} ({(float)winsByColor[Color.Red] / numSimulations * 100:F1}%)");
            Console.WriteLine(
                $"Blue (Aggressive) wins: {winsByColor[Color.Blue]} ({(float)winsByColor[Color.Blue] / numSimulations * 100:F1}%)");
            Console.WriteLine(
                $"Green (Resource) wins: {winsByColor[Color.Green]} ({(float)winsByColor[Color.Green] / numSimulations * 100:F1}%)");
            Console.WriteLine(
                $"Yellow (Defensive) wins: {winsByColor[Color.Yellow]} ({(float)winsByColor[Color.Yellow] / numSimulations * 100:F1}%)");
            Console.WriteLine($"Draws: {draws} ({(float)draws / numSimulations * 100:F1}%)");

            Console.WriteLine("\nSimulation testing complete!");
        }

        private static void ForceWinnerDetermination(GameState state, Dictionary<Color, int> winsByColor, ref int draws)
        {
            var bedsAlive = state.GetBedIslands().Where(b => b.IsBedAlive()).ToList();
            var playersAlive = state.GetPlayers().Where(p => p.getIsAlive()).ToList();

            if (bedsAlive.Count == 1)
            {
                Color winnerColor = bedsAlive[0].GetColor();
                winsByColor[winnerColor]++;
                Console.WriteLine($"Forced winner determination: {winnerColor} (only bed alive)");
                return;
            }

            Player bestPlayer = null;
            float bestScore = float.NegativeInfinity;

            foreach (var player in playersAlive)
            {
                float playerScore = CalculatePlayerScore(player);
                if (playerScore > bestScore)
                {
                    bestScore = playerScore;
                    bestPlayer = player;
                }
            }

            if (bestPlayer != null)
            {
                Color winnerColor = bestPlayer.getColor();
                winsByColor[winnerColor]++;
                Console.WriteLine($"Forced winner determination: {winnerColor} (best player stats)");
            }
            else
            {
                draws++;
                Console.WriteLine("Forced winner determination: Draw (no clear winner)");
            }
        }

        private static float CalculatePlayerScore(Player player)
        {
            float score = 0;

            if (player.HasBed()) score += 10000;

            if (player.getIsAlive()) score += 5000;

            return score;
        }

        private static (List<BedIsland> bedIslands, List<DiamondIsland> diamondIslands, List<EmeraldIsland>
            emeraldIslands) CreateIslands()
        {
            Console.WriteLine("Creating islands and connections...");

            var redIsland = new BedIsland(Color.Red, 5, new Dictionary<AIsland, Connection>());
            var blueIsland = new BedIsland(Color.Blue, 5, new Dictionary<AIsland, Connection>());
            var greenIsland = new BedIsland(Color.Green, 5, new Dictionary<AIsland, Connection>());
            var yellowIsland = new BedIsland(Color.Yellow, 5, new Dictionary<AIsland, Connection>());

            var diamondIsland1 = new DiamondIsland(new Dictionary<AIsland, Connection>(), 60);
            var diamondIsland2 = new DiamondIsland(new Dictionary<AIsland, Connection>(), 60);
            var emeraldIsland = new EmeraldIsland(new Dictionary<AIsland, Connection>(), 120);

            var redToDiamond1 = new Connection(10);
            var blueToDiamond1 = new Connection(10);
            var greenToDiamond2 = new Connection(10);
            var yellowToDiamond2 = new Connection(10);

            var diamond1ToEmerald = new Connection(15);
            var diamond2ToEmerald = new Connection(15);

            var redToBlue = new Connection(20);
            var greenToYellow = new Connection(20);

            var redToGreen = new Connection(30);
            var blueToYellow = new Connection(30);

            redIsland.GetNeighbors().Add(diamondIsland1, redToDiamond1);
            redIsland.GetNeighbors().Add(blueIsland, redToBlue);
            redIsland.GetNeighbors().Add(greenIsland, redToGreen);

            blueIsland.GetNeighbors().Add(diamondIsland1, blueToDiamond1);
            blueIsland.GetNeighbors().Add(redIsland, redToBlue);
            blueIsland.GetNeighbors().Add(yellowIsland, blueToYellow);

            greenIsland.GetNeighbors().Add(diamondIsland2, greenToDiamond2);
            greenIsland.GetNeighbors().Add(yellowIsland, greenToYellow);
            greenIsland.GetNeighbors().Add(redIsland, redToGreen);

            yellowIsland.GetNeighbors().Add(diamondIsland2, yellowToDiamond2);
            yellowIsland.GetNeighbors().Add(greenIsland, greenToYellow);
            yellowIsland.GetNeighbors().Add(blueIsland, blueToYellow);

            diamondIsland1.GetNeighbors().Add(redIsland, redToDiamond1);
            diamondIsland1.GetNeighbors().Add(blueIsland, blueToDiamond1);
            diamondIsland1.GetNeighbors().Add(emeraldIsland, diamond1ToEmerald);

            diamondIsland2.GetNeighbors().Add(greenIsland, greenToDiamond2);
            diamondIsland2.GetNeighbors().Add(yellowIsland, yellowToDiamond2);
            diamondIsland2.GetNeighbors().Add(emeraldIsland, diamond2ToEmerald);

            emeraldIsland.GetNeighbors().Add(diamondIsland1, diamond1ToEmerald);
            emeraldIsland.GetNeighbors().Add(diamondIsland2, diamond2ToEmerald);

            return (new List<BedIsland> { redIsland, blueIsland, greenIsland, yellowIsland },
                new List<DiamondIsland> { diamondIsland1, diamondIsland2 }, new List<EmeraldIsland> { emeraldIsland });
        }

        private static List<Player> CreatePlayers(
            (List<BedIsland> bedIslands, List<DiamondIsland> diamondIslands, List<EmeraldIsland> emeraldIslands)
                islandData)
        {
            Console.WriteLine("Creating players with different AI strategies...");

            var redPlayer = new OptimalPlayer(combatSkill: 5, bridgingSkill: 5, home: islandData.bedIslands[0]);
            var bluePlayer = new AggressivePlayer(combatSkill: 5, bridgingSkill: 5, home: islandData.bedIslands[1]);
            var greenPlayer = new ResourcePlayer(combatSkill: 5, bridgingSkill: 5, home: islandData.bedIslands[2]);
            var yellowPlayer = new DefensivePlayer(combatSkill: 5, bridgingSkill: 5, home: islandData.bedIslands[3]);

            redPlayer.SetHomeIsland(islandData.bedIslands[0]);
            redPlayer.CurrentIsland = islandData.bedIslands[0];
            redPlayer.SetColor();

            bluePlayer.SetHomeIsland(islandData.bedIslands[1]);
            bluePlayer.CurrentIsland = islandData.bedIslands[1];
            bluePlayer.SetColor();

            greenPlayer.SetHomeIsland(islandData.bedIslands[2]);
            greenPlayer.CurrentIsland = islandData.bedIslands[2];
            greenPlayer.SetColor();

            yellowPlayer.SetHomeIsland(islandData.bedIslands[3]);
            yellowPlayer.CurrentIsland = islandData.bedIslands[3];
            yellowPlayer.SetColor();

            Console.WriteLine("Red: OptimalPlayer - Using sophisticated minimax for optimal decisions");
            Console.WriteLine("Blue: AggressivePlayer - Focuses on combat and bed destruction");
            Console.WriteLine("Green: ResourcePlayer - Prioritizes resource gathering and upgrades");
            Console.WriteLine("Yellow: DefensivePlayer - Builds defenses and plays conservatively");

            return new List<Player> { redPlayer, bluePlayer, greenPlayer, yellowPlayer };
        }
    }

    public class OptimalPlayer : Player
    {
        public OptimalPlayer(int combatSkill, int bridgingSkill, BedIsland home) : base(combatSkill, bridgingSkill,
            home)
        {
        }

        public override ICommand DecideNextAction(GameState gameState)
        {
            return new BedwarsAIAgent(this, gameState).GetOptimalAction();
        }
    }

    public class AggressivePlayer : Player
    {
        private readonly Random _random = new Random();

        public AggressivePlayer(int combatSkill, int bridgingSkill, BedIsland home) : base(combatSkill, bridgingSkill,
            home)
        {
        }

        // For AggressivePlayer
        public override ICommand DecideNextAction(GameState gameState)
        {
            // First priority: Break enemy beds if on their island
            if (CurrentIsland is BedIsland bedIsland && bedIsland.GetColor() != this.getColor() &&
                bedIsland.IsBedAlive())
            {
                if (this.Pickaxe != null)
                    return new MineBlockCommand(bedIsland, this);
                else if (Inventory.hasEnoughMoney(new WoodenPickaxe().Cost)) return new BuyWoodenPickaxe();
            }

            // Second priority: Combat enemies on same island
            var enemiesAtSameIsland = gameState.GetPlayers()
                .Where(p => p.getColor() != this.getColor() && p.getIsAlive() && p.CurrentIsland == this.CurrentIsland)
                .ToList();

            if (enemiesAtSameIsland.Count > 0 && (this.Sword != null || this.getPlayerHealth() > 15))
                return new CombatEncounter(this, enemiesAtSameIsland[0], false);

            // Third priority: Get basic equipment
            if (this.Sword == null && Inventory.hasEnoughMoney(new StoneSword().Cost)) return new BuyStoneSword();

            if (this.Pickaxe == null && Inventory.hasEnoughMoney(new WoodenPickaxe().Cost))
                return new BuyWoodenPickaxe();

            // Fourth priority: Seek out enemy beds (more aggressive than optimal AI)
            var enemyBedIslands = gameState.GetBedIslands()
                .Where(b => b.GetColor() != this.getColor() && b.IsBedAlive())
                .ToList();

            if (enemyBedIslands.Count > 0)
            {
                // Aggressively move toward enemy beds, even with minimal preparation
                // This is a strategic weakness - might head into danger unprepared
                foreach (var neighbor in CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key is BedIsland targetBed && targetBed.GetColor() != this.getColor() &&
                        targetBed.IsBedAlive())
                    {
                        if (neighbor.Value.IsComplete())
                            return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                        else
                            return new BuildBridge(neighbor.Value, this);
                    }
                }

                // Move to any island to explore for paths to enemy beds
                foreach (var neighbor in CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Value.IsComplete())
                        return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                    else
                        return new BuildBridge(neighbor.Value, this);
                }
            }

            // Fallback to optimal strategy if no aggressive moves available
            return new BedwarsAIAgent(this, gameState).GetOptimalAction();
        }
    }

    public class ResourcePlayer : Player
    {
        private readonly Random _random = new Random();

        public ResourcePlayer(int combatSkill, int bridgingSkill, BedIsland home) : base(combatSkill, bridgingSkill,
            home)
        {
        }

        public override ICommand DecideNextAction(GameState gameState)
        {
            // Priority: Collect resources from diamond/emerald islands

            // Move to resource islands if not already there
            if (!(CurrentIsland is DiamondIsland) && !(CurrentIsland is EmeraldIsland))
            {
                foreach (var neighbor in CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key is DiamondIsland || neighbor.Key is EmeraldIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                        else
                            return new BuildBridge(neighbor.Value, this);
                    }
                }
            }

            if ((CurrentIsland is DiamondIsland || CurrentIsland is EmeraldIsland) &&
                ((this.getPlayerHealth() < 10) || _random.Next(10) < 3))
            {
                foreach (var neighbor in CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key == HomeIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                        else
                            return new BuildBridge(neighbor.Value, this);
                    }
                }
            }

            if (CurrentIsland == HomeIsland)
            {
                if (this.Pickaxe == null && Inventory.hasEnoughMoney(new WoodenPickaxe().Cost))
                    return new BuyWoodenPickaxe();

                if (this.Sword == null && Inventory.hasEnoughMoney(new StoneSword().Cost)) return new BuyStoneSword();

                if (this.Pickaxe != null && this.Pickaxe is WoodenPickaxe &&
                    Inventory.hasEnoughMoney(new IronPickaxe().Cost))
                    return new BuyIronPickaxe();

                if (!_upgrades.Sharpness && Inventory.hasEnoughMoney(_upgrades.SharpnessCost))
                    return new BuySharpness();

                if (!_upgrades.ManiacMiner && Inventory.hasEnoughMoney(_upgrades.ManiacMinerCost))
                    return new BuyManiacMiner();

                foreach (var neighbor in CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key is DiamondIsland || neighbor.Key is EmeraldIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                        else
                            return new BuildBridge(neighbor.Value, this);
                    }
                }
            }

            return new BedwarsAIAgent(this, gameState).GetOptimalAction();
        }
    }

    public class DefensivePlayer : Player
    {
        private readonly Random _random = new Random();

        public DefensivePlayer(int combatSkill, int bridgingSkill, BedIsland home) : base(combatSkill, bridgingSkill,
            home)
        {
        }

        public override ICommand DecideNextAction(GameState gameState)
        {
            // First priority: Break beds if already on enemy island and properly equipped
            if (CurrentIsland is BedIsland bedIsland && bedIsland.GetColor() != this.getColor() &&
                bedIsland.IsBedAlive() && this.Pickaxe != null)
            {
                return new MineBlockCommand(bedIsland, this);
            }

            // If at home, prioritize defense
            if (CurrentIsland == HomeIsland)
            {
                // Attack enemies at home (highest defense priority)
                var enemiesAtHome = gameState.GetPlayers()
                    .Where(p => p.getColor() != this.getColor() && p.getIsAlive() && p.CurrentIsland == HomeIsland)
                    .ToList();

                if (enemiesAtHome.Count > 0) return new CombatEncounter(this, enemiesAtHome[0], false);

                // Build defenses (second highest priority)
                if (!HomeIsland.HasDefense() && Inventory.HasItems<Wool>(8)) return new PlaceWool(HomeIsland);

                if (HomeIsland.DefenseLayers.Count == 1 && Inventory.HasItems<Endstone>(8))
                    return new PlaceEndstone(HomeIsland);

                if (HomeIsland.DefenseLayers.Count == 2 && Inventory.HasItems<Obsidian>(4))
                    return new PlaceObsidian(HomeIsland);

                // Buy defense materials
                if (!Inventory.HasItems<Wool>(8) && Inventory.hasEnoughMoney(new Wool().Cost)) return new BuyWool();

                if (HomeIsland.DefenseLayers.Count >= 1 && !Inventory.HasItems<Endstone>(8) &&
                    Inventory.hasEnoughMoney(new Endstone().Cost))
                    return new BuyEndstone();

                if (HomeIsland.DefenseLayers.Count >= 2 && !Inventory.HasItems<Obsidian>(4) &&
                    Inventory.hasEnoughMoney(new Obsidian().Cost))
                    return new BuyObsidian();

                // Basic equipment after defenses
                if (this.Sword == null && Inventory.hasEnoughMoney(new StoneSword().Cost)) return new BuyStoneSword();

                if (this.Pickaxe == null && Inventory.hasEnoughMoney(new WoodenPickaxe().Cost))
                    return new BuyWoodenPickaxe();

                // Only go for beds after establishing good defenses
                // Defensive player waits longer than optimal - this is its weakness
                int minDefensesBeforeAttacking = 2;
                int minTicksBeforeAttacking = 150000; // Adjusted for 1,000,000 max ticks

                if (HomeIsland.DefenseLayers.Count >= minDefensesBeforeAttacking ||
                    gameState.GetCurrentTick() > minTicksBeforeAttacking)
                {
                    var enemyBedIslands = gameState.GetBedIslands()
                        .Where(b => b.GetColor() != this.getColor() && b.IsBedAlive())
                        .ToList();

                    if (enemyBedIslands.Count > 0)
                    {
                        foreach (var neighbor in CurrentIsland.GetNeighbors())
                        {
                            if (neighbor.Key is BedIsland bedNeighbor && bedNeighbor.GetColor() != this.getColor() &&
                                bedNeighbor.IsBedAlive())
                            {
                                if (neighbor.Value.IsComplete())
                                    return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                                else
                                    return new BuildBridge(neighbor.Value, this);
                            }
                        }
                    }
                }
            }
            else
            {
                // When away from home, prioritize returning if home is under attack
                var enemiesAtHome = gameState.GetPlayers()
                    .Where(p => p.getColor() != this.getColor() && p.getIsAlive() && p.CurrentIsland == HomeIsland)
                    .ToList();

                if (enemiesAtHome.Count > 0)
                {
                    // Try to return home
                    foreach (var neighbor in CurrentIsland.GetNeighbors())
                    {
                        if (neighbor.Key == HomeIsland)
                        {
                            if (neighbor.Value.IsComplete())
                                return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                            else
                                return new BuildBridge(neighbor.Value, this);
                        }
                    }
                }

                // Periodically return home to check defenses
                if (gameState.GetCurrentTick() % 50000 == 0 ||
                    getPlayerHealth() < 8) // Adjusted for 1,000,000 max ticks
                {
                    foreach (var neighbor in CurrentIsland.GetNeighbors())
                    {
                        if (neighbor.Key == HomeIsland)
                        {
                            if (neighbor.Value.IsComplete())
                                return new Move(CurrentIsland, neighbor.Key, neighbor.Value, this);
                            else
                                return new BuildBridge(neighbor.Value, this);
                        }
                    }
                }
            }

            // Fallback to optimal strategy if no defensive moves available
            return new BedwarsAIAgent(this, gameState).GetOptimalAction();
        }
    }
}