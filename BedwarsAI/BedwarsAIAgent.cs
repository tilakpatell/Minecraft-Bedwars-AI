using BedwarsAI.Commands;
using BedwarsAI.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BedwarsAI
{
    public class BedwarsAIAgent
    {
        private readonly Player _player;
        private readonly GameState _gameState;
        private readonly int _maxDepth = 3;
        private readonly Random _random = new Random();
        private AIsland _lastIsland = null;
        private int _oscillationCounter = 0;

        private StrategicGoal _lastGoal = StrategicGoal.GatherResources;
        private int _goalStickiness = 15;
        private int _currentGoalTicks = 0;
        private AIsland _targetIsland = null;
        private int _stuckCounter = 0;
        private int _lastTick = 0;

        private const int EARLY_GAME_TICKS = 100000;
        private const int MID_GAME_TICKS = 300000;

        private const float BED_DESTROY_PRIORITY = 10000f;
        private const float PLAYER_KILL_PRIORITY = 2000f;
        private const float RESOURCE_PRIORITY = 500f;
        private const float DEFENSE_PRIORITY = 800f;
        private const float SURVIVAL_PRIORITY = 15000f;

        public BedwarsAIAgent(Player player, GameState gameState)
        {
            _player = player;
            _gameState = gameState;
        }

        public ICommand GetOptimalAction()
        {
            int currentTick = _gameState.GetCurrentTick();
            if (currentTick > _lastTick + 5)
            {
                _stuckCounter++;
            }
            else
            {
                _stuckCounter = 0;
            }

            _lastTick = currentTick;

            if (_stuckCounter > 10)
            {
                _lastGoal = StrategicGoal.GatherResources;
                _targetIsland = null;
                _stuckCounter = 0;
            }

            Console.WriteLine(
                $"{_player.getColor()} AI thinking: Health={_player.getPlayerHealth()}, Island={_player.CurrentIsland?.GetType().Name}, HasPickaxe={_player.Pickaxe != null}, HasSword={_player.Sword != null}");

            if (_player.getPlayerHealth() < 5 && _player.Inventory.HasItems<GoldenApple>(1))
            {
                return new UseGoldenApple();
            }

            var enemiesAtCurrentIsland = GetEnemiesAtCurrentIsland();
            if (enemiesAtCurrentIsland.Any() && _player.getPlayerHealth() < 8 &&
                _player.CurrentIsland != _player.HomeIsland)
            {
                var safePath = FindSafePath();
                if (safePath != null)
                {
                    Console.WriteLine($"*** {_player.getColor()} RETREATING from danger ***");
                    return safePath;
                }
            }

            var bedBreakAction = TryGetBedBreakingAction();
            if (bedBreakAction != null)
            {
                Console.WriteLine($"*** {_player.getColor()} DIRECTLY executing bed breaking action ***");
                return bedBreakAction;
            }

            var directPathToBedAction = TryGetDirectPathToBedAction();
            if (directPathToBedAction != null)
            {
                Console.WriteLine($"*** {_player.getColor()} DIRECTLY taking path to enemy bed ***");
                return directPathToBedAction;
            }

            bool allBedsDestroyed = _gameState.GetBedIslands().All(b => !b.IsBedAlive());
            if (allBedsDestroyed)
            {
                var survivalAction = GetSurvivalAction();
                if (survivalAction != null)
                {
                    Console.WriteLine($"*** {_player.getColor()} executing SURVIVAL action ***");
                    return survivalAction;
                }
            }

            StrategicGoal currentGoal = DetermineStrategicGoal();
            Console.WriteLine($"{_player.getColor()} goal: {currentGoal}");

            List<ICommand> possibleActions = GenerateActionsForGoal(currentGoal);

            possibleActions = FilterViableActions(possibleActions);

            if (possibleActions.Count == 0)
            {
                Console.WriteLine($"{_player.getColor()} has no viable actions, using default action");
                return GetDefaultAction();
            }

            ICommand selectedAction = SelectOptimalActionUsingMinimax(possibleActions);

            Console.WriteLine($"{_player.getColor()} selected action: {selectedAction.GetType().Name}");
            return selectedAction;
        }

        private List<Player> GetEnemiesAtCurrentIsland()
        {
            return _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() &&
                            p.getIsAlive() &&
                            p.CurrentIsland == _player.CurrentIsland)
                .ToList();
        }

        private ICommand FindSafePath()
        {
            var dangerRatings = new Dictionary<AIsland, int>();

            foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
            {
                if (neighbor.Value.IsComplete())
                {
                    int enemyCount = _gameState.GetPlayers()
                        .Count(p => p.getColor() != _player.getColor() &&
                                    p.getIsAlive() &&
                                    p.CurrentIsland == neighbor.Key);

                    dangerRatings[neighbor.Key] = enemyCount;
                }
            }

            var safestIsland = dangerRatings
                .OrderBy(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key == _player.HomeIsland ? 0 : 1)
                .FirstOrDefault();

            if (safestIsland.Key != null)
            {
                return new Move(_player.CurrentIsland, safestIsland.Key,
                    _player.CurrentIsland.GetNeighbors()[safestIsland.Key], _player);
            }

            return null;
        }

        private ICommand GetSurvivalAction()
        {
            if (_lastIsland == _player.CurrentIsland)
                _oscillationCounter++;
            else
                _oscillationCounter = 0;

            _lastIsland = _player.CurrentIsland;

            if (_oscillationCounter > 5)
            {
                _oscillationCounter = 0;

                var enemies = _gameState.GetPlayers()
                    .Where(p => p.getColor() != _player.getColor() && p.getIsAlive())
                    .OrderBy(p => CalculatePathDistance(_player.CurrentIsland, p.CurrentIsland))
                    .ToList();

                if (enemies.Any())
                {
                    var target = enemies.First();
                    _targetIsland = target.CurrentIsland;
                    Console.WriteLine(
                        $"*** {_player.getColor()} breaking oscillation pattern, targeting {target.getColor()} ***");
                }
            }

            if (_player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    return new UseGoldenApple();
                }
                else if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    return new BuyGoldenApple();
                }
            }

            if (_player.Sword == null || _player.Sword is StoneSword)
            {
                if (_player.Inventory.hasEnoughMoney(new IronSword().Cost))
                {
                    return new BuyIronSword();
                }
            }

            var enemiesAtCurrentIsland = GetEnemiesAtCurrentIsland();
            if (enemiesAtCurrentIsland.Any())
            {
                if (_player.getPlayerHealth() >= 15 && (_player.Sword != null || _player.CombatSkill > 5))
                {
                    return new CombatEncounter(_player, enemiesAtCurrentIsland.First(), false);
                }
                else
                {
                    var safePath = FindSafePath();
                    if (safePath != null)
                    {
                        return safePath;
                    }
                }
            }

            var vulnerableEnemies = _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() &&
                            p.getIsAlive() &&
                            p.getPlayerHealth() < _player.getPlayerHealth() - 5)
                .OrderBy(p => p.getPlayerHealth())
                .ToList();

            if (vulnerableEnemies.Any())
            {
                var targetEnemy = vulnerableEnemies.First();
                if (targetEnemy.CurrentIsland == _player.CurrentIsland)
                {
                    return new CombatEncounter(_player, targetEnemy, false);
                }
                else
                {
                    foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                    {
                        if (neighbor.Key == targetEnemy.CurrentIsland && neighbor.Value.IsComplete())
                        {
                            return new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player);
                        }
                    }
                }
            }

            // Actively seek ANY enemy when all beds are gone
            var anyEnemies = _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() && p.getIsAlive())
                .OrderBy(p => CalculatePathDistance(_player.CurrentIsland, p.CurrentIsland))
                .ToList();

            if (anyEnemies.Any())
            {
                var targetEnemy = anyEnemies.First();
                if (targetEnemy.CurrentIsland == _player.CurrentIsland)
                {
                    return new CombatEncounter(_player, targetEnemy, false);
                }
                else
                {
                    _targetIsland = targetEnemy.CurrentIsland;

                    foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                    {
                        int currentDistance = CalculatePathDistance(_player.CurrentIsland, targetEnemy.CurrentIsland);
                        int neighborDistance = CalculatePathDistance(neighbor.Key, targetEnemy.CurrentIsland);

                        if (neighborDistance < currentDistance && neighbor.Value.IsComplete())
                        {
                            return new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player);
                        }
                        else if (neighborDistance < currentDistance)
                        {
                            return new BuildBridge(neighbor.Value, _player);
                        }
                    }
                }
            }

            return null;
        }

        private ICommand TryGetBedBreakingAction()
        {
            if (_player.CurrentIsland is BedIsland bedIsland &&
                bedIsland.GetColor() != _player.getColor() &&
                bedIsland.IsBedAlive())
            {
                if (_player.Pickaxe != null)
                {
                    return new MineBlockCommand(bedIsland, _player);
                }
                else
                {
                    return new BuyWoodenPickaxe();
                }
            }

            return null;
        }

        private ICommand TryGetDirectPathToBedAction()
        {
            foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
            {
                if (neighbor.Key is BedIsland enemyBed &&
                    enemyBed.GetColor() != _player.getColor() &&
                    enemyBed.IsBedAlive())
                {
                    Console.WriteLine($"** {_player.getColor()} found direct path to {enemyBed.GetColor()}'s bed! **");
                    if (neighbor.Value.IsComplete())
                    {
                        return new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player);
                    }
                    else
                    {
                        return new BuildBridge(neighbor.Value, _player);
                    }
                }
            }

            return null;
        }

        private StrategicGoal DetermineStrategicGoal()
        {
            bool allBedsDestroyed = _gameState.GetBedIslands().All(b => !b.IsBedAlive());
            if (allBedsDestroyed)
            {
                _lastGoal = StrategicGoal.EliminatePlayers;
                return StrategicGoal.EliminatePlayers;
            }

            if (_lastGoal == StrategicGoal.DefendBase && !IsBaseUnderAttack())
            {
                _lastGoal = StrategicGoal.DestroyBeds;
                _currentGoalTicks = 0;
                return StrategicGoal.DestroyBeds;
            }

            if (_currentGoalTicks < _goalStickiness)
            {
                _currentGoalTicks++;
                return _lastGoal;
            }

            _currentGoalTicks = 0;

            if (_player.CurrentIsland is BedIsland bedIsland &&
                bedIsland.GetColor() != _player.getColor() &&
                bedIsland.IsBedAlive())
            {
                _lastGoal = StrategicGoal.DestroyBeds;
                return StrategicGoal.DestroyBeds;
            }

            bool shouldCheckDefense = (_player.CurrentIsland == _player.HomeIsland) ||
                                      (_gameState.GetCurrentTick() % 20 == 0);

            if (shouldCheckDefense && IsBaseUnderAttack())
            {
                _lastGoal = StrategicGoal.DefendBase;
                return StrategicGoal.DefendBase;
            }

            bool hasBasicGear = _player.Pickaxe != null && _player.Sword != null;

            var vulnerableEnemyBeds = FindVulnerableEnemyBeds();

            var vulnerableEnemies = FindVulnerableEnemies();

            if (!hasBasicGear)
            {
                _lastGoal = StrategicGoal.GatherResources;
                return StrategicGoal.GatherResources;
            }

            if (vulnerableEnemyBeds.Any())
            {
                _lastGoal = StrategicGoal.DestroyBeds;
                return StrategicGoal.DestroyBeds;
            }

            if (vulnerableEnemies.Any())
            {
                _lastGoal = StrategicGoal.EliminatePlayers;
                return StrategicGoal.EliminatePlayers;
            }

            _lastGoal = StrategicGoal.GatherResources;
            return StrategicGoal.GatherResources;
        }

        private List<BedIsland> FindVulnerableEnemyBeds()
        {
            return _gameState.GetBedIslands()
                .Where(b => b.GetColor() != _player.getColor() && b.IsBedAlive())
                .OrderBy(b => CalculatePathDistance(_player.CurrentIsland, b))
                .ToList();
        }

        private List<Player> FindVulnerableEnemies()
        {
            return _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() && p.getIsAlive() && !p.HasBed())
                .OrderBy(p => p.getPlayerHealth())
                .ToList();
        }

        private bool IsBaseUnderAttack()
        {
            bool underAttack = _gameState.GetPlayers().Any(p =>
                p.getColor() != _player.getColor() &&
                p.getIsAlive() &&
                p.CurrentIsland == _player.HomeIsland);

            if (underAttack)
            {
                Console.WriteLine($"*** {_player.getColor()}'s base is under attack! ***");
            }

            return underAttack;
        }

        private int CalculatePathDistance(AIsland source, AIsland destination)
        {
            if (source == null || destination == null)
                return int.MaxValue;

            if (source == destination)
                return 0;

            var visited = new HashSet<AIsland>();
            var queue = new Queue<(AIsland, int)>();
            queue.Enqueue((source, 0));

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                if (current == destination)
                    return distance;

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (!visited.Contains(neighbor.Key))
                    {
                        queue.Enqueue((neighbor.Key, distance + 1));
                    }
                }
            }

            return int.MaxValue;
        }

        private List<ICommand> GenerateActionsForGoal(StrategicGoal goal)
        {
            var actions = new List<ICommand>();

            switch (goal)
            {
                case StrategicGoal.GatherResources:
                    actions.AddRange(GenerateResourceGatheringActions());
                    break;
                case StrategicGoal.DestroyBeds:
                    actions.AddRange(GenerateBedDestroyingActions());
                    break;
                case StrategicGoal.DefendBase:
                    actions.AddRange(GenerateDefensiveActions());
                    break;
                case StrategicGoal.EliminatePlayers:
                    actions.AddRange(GeneratePlayerEliminationActions());
                    break;
            }

            actions.AddRange(GenerateImediateCombatActions());

            actions.AddRange(GenerateMovementActions());

            return actions;
        }

        private ICommand SelectOptimalActionUsingMinimax(List<ICommand> actions)
        {
            ICommand bestAction = null;
            float bestScore = float.NegativeInfinity;

            foreach (var action in actions)
            {
                if (action is CombatEncounter combat)
                {
                    float score = EvaluateExpectedCombatOutcome(combat);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAction = action;
                    }
                }
                else if (action is MineBlockCommand)
                {
                    return action;
                }
                else if (action is UseGoldenApple && _player.getPlayerHealth() < 10)
                {
                    return action;
                }
                else if (action is BuyGoldenApple && _player.getPlayerHealth() < 10 &&
                         _player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    return action;
                }
                else
                {
                    GameState simulatedState = SimulateAction(_gameState, action);
                    float score = Minimax(simulatedState, _maxDepth - 1, false, float.NegativeInfinity,
                        float.PositiveInfinity);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAction = action;
                    }
                }
            }

            return bestAction ?? actions.FirstOrDefault() ?? GetDefaultAction();
        }

        private float Minimax(GameState state, int depth, bool isMaximizingPlayer, float alpha, float beta)
        {
            if (depth == 0 || state.IsGameOver())
            {
                return EvaluateGameState(state);
            }

            if (isMaximizingPlayer)
            {
                float maxEval = float.NegativeInfinity;
                foreach (var action in GenerateAllPossibleActions())
                {
                    GameState childState = SimulateAction(state, action);
                    float eval = Minimax(childState, depth - 1, false, alpha, beta);
                    maxEval = Math.Max(maxEval, eval);

                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }

                return maxEval;
            }
            else
            {
                float minEval = float.PositiveInfinity;

                var enemies = state.GetPlayers()
                    .Where(p => p.getColor() != _player.getColor() && p.getIsAlive())
                    .ToList();

                if (enemies.Count == 0)
                    return Minimax(state, depth - 1, true, alpha, beta);

                foreach (var enemy in enemies)
                {
                    var enemyActions = GenerateActionsForPlayer(enemy, state);

                    foreach (var action in enemyActions)
                    {
                        GameState childState = SimulateEnemyAction(state, action, enemy);
                        float eval = Minimax(childState, depth - 1, true, alpha, beta);
                        minEval = Math.Min(minEval, eval);

                        beta = Math.Min(beta, eval);
                        if (beta <= alpha)
                            break;
                    }

                    if (beta <= alpha)
                        break;
                }

                return minEval;
            }
        }

        private List<ICommand> GenerateResourceGatheringActions()
        {
            var actions = new List<ICommand>();

            if (_player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    actions.Insert(0, new UseGoldenApple());
                }
                else if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    actions.Insert(0, new BuyGoldenApple());
                }
            }

            if (_player.Pickaxe == null)
                actions.Insert(0, new BuyWoodenPickaxe());

            if (_player.Sword == null)
                actions.Insert(0, new BuyStoneSword());

            if (_player.Pickaxe != null && _player.Pickaxe is WoodenPickaxe)
                actions.Add(new BuyIronPickaxe());

            if (_player.Pickaxe != null && _player.Pickaxe is IronPickaxe)
                actions.Add(new BuyDiamondPickaxe());

            if (_player.Sword != null && _player.Sword is StoneSword)
                actions.Add(new BuyIronSword());

            if (_player.Sword != null && _player.Sword is IronSword)
                actions.Add(new BuyDiamondSword());

            if (_player.Sword != null && !_player._upgrades.Sharpness &&
                _player.Inventory.hasEnoughMoney(_player._upgrades.SharpnessCost))
                actions.Add(new BuySharpness());

            if (_player.Pickaxe != null && !_player._upgrades.ManiacMiner &&
                _player.Inventory.hasEnoughMoney(_player._upgrades.ManiacMinerCost))
                actions.Add(new BuyManiacMiner());

            if (!_player.HomeIsland.HasDefense() && _player.CurrentIsland == _player.HomeIsland)
            {
                actions.Add(new BuyWool(16));

                if (_player.Inventory.HasItems<Wool>(8))
                {
                    actions.Add(new PlaceWool(_player.HomeIsland));
                }
            }

            if (_player.CurrentIsland is DiamondIsland || _player.CurrentIsland is EmeraldIsland)
            {
                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key == _player.HomeIsland && neighbor.Value.IsComplete())
                    {
                        actions.Add(new Move(_player.CurrentIsland, _player.HomeIsland, neighbor.Value, _player));
                        break;
                    }
                }
            }

            if (_player.CurrentIsland == _player.HomeIsland)
            {
                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if ((neighbor.Key is DiamondIsland || neighbor.Key is EmeraldIsland) &&
                        neighbor.Value.IsComplete())
                    {
                        actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                    }
                    else if ((neighbor.Key is DiamondIsland || neighbor.Key is EmeraldIsland))
                    {
                        actions.Add(new BuildBridge(neighbor.Value, _player));
                    }
                }
            }

            return actions;
        }

        private List<ICommand> GenerateBedDestroyingActions()
        {
            var actions = new List<ICommand>();

            if (_player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    actions.Insert(0, new UseGoldenApple());
                }
                else if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    actions.Insert(0, new BuyGoldenApple());
                }
            }

            if (_player.Pickaxe == null)
                actions.Insert(0, new BuyWoodenPickaxe());
            else if (_player.Pickaxe is WoodenPickaxe && _player.Inventory.hasEnoughMoney(new IronPickaxe().Cost))
                actions.Insert(0, new BuyIronPickaxe());
            else if (_player.Pickaxe is IronPickaxe && _player.Inventory.hasEnoughMoney(new DiamondPickaxe().Cost))
                actions.Add(new BuyDiamondPickaxe());

            if (_player.Pickaxe != null && !_player._upgrades.ManiacMiner &&
                _player.Inventory.hasEnoughMoney(_player._upgrades.ManiacMinerCost))
                actions.Add(new BuyManiacMiner());

            var enemyBedIslands = _gameState.GetBedIslands()
                .Where(b => b.GetColor() != _player.getColor() && b.IsBedAlive())
                .ToList();

            if (enemyBedIslands.Any())
            {
                var nearestBed = enemyBedIslands
                    .OrderBy(b => CalculatePathDistance(_player.CurrentIsland, b))
                    .First();

                _targetIsland = nearestBed;

                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    int currentDistance = CalculatePathDistance(_player.CurrentIsland, nearestBed);
                    int neighborDistance = CalculatePathDistance(neighbor.Key, nearestBed);

                    if (neighborDistance < currentDistance)
                    {
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));

                        return actions;
                    }
                }
            }

            return actions;
        }

        private List<ICommand> GenerateDefensiveActions()
        {
            var actions = new List<ICommand>();

            if (_player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    actions.Insert(0, new UseGoldenApple());
                }
                else if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    actions.Insert(0, new BuyGoldenApple());
                }
            }

            if (_player.CurrentIsland != _player.HomeIsland && IsBaseUnderAttack())
            {
                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key == _player.HomeIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, _player.HomeIsland, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));
                        return actions;
                    }
                }
            }

            if (_player.CurrentIsland == _player.HomeIsland)
            {
                if (!_player.HomeIsland.HasDefense() && _player.Inventory.HasItems<Wool>(8))
                    actions.Add(new PlaceWool(_player.HomeIsland));

                if (_player.HomeIsland.DefenseLayers.Count == 1 && _player.Inventory.HasItems<Endstone>(8))
                    actions.Add(new PlaceEndstone(_player.HomeIsland));

                if (_player.HomeIsland.DefenseLayers.Count == 2 && _player.Inventory.HasItems<Obsidian>(4))
                    actions.Add(new PlaceObsidian(_player.HomeIsland));

                if (!_player.Inventory.HasItems<Wool>(8))
                    actions.Add(new BuyWool());

                if (!_player.Inventory.HasItems<Endstone>(8) && _player.HomeIsland.DefenseLayers.Count >= 1)
                    actions.Add(new BuyEndstone());

                if (!_player.Inventory.HasItems<Obsidian>(4) && _player.HomeIsland.DefenseLayers.Count >= 2)
                    actions.Add(new BuyObsidian());
            }

            var enemiesAtHome = _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() &&
                            p.getIsAlive() &&
                            p.CurrentIsland == _player.HomeIsland)
                .ToList();

            foreach (var enemy in enemiesAtHome)
            {
                if (_player.getPlayerHealth() > enemy.getPlayerHealth() || _player.getPlayerHealth() > 15)
                {
                    actions.Add(new CombatEncounter(_player, enemy, false));
                }
            }

            if (_player.Sword == null)
                actions.Add(new BuyStoneSword());

            if (_player.Sword != null && _player.Sword is StoneSword)
                actions.Add(new BuyIronSword());

            if (_player.getPlayerHealth() < 10)
                actions.Add(new BuyGoldenApple());

            if (_player.Inventory.HasItems<GoldenApple>(1) && _player.getPlayerHealth() < 15)
                actions.Add(new UseGoldenApple());

            return actions;
        }

        private List<ICommand> GeneratePlayerEliminationActions()
        {
            var actions = new List<ICommand>();

            if (_player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    actions.Insert(0, new UseGoldenApple());
                }
                else if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                {
                    actions.Insert(0, new BuyGoldenApple());
                }
            }

            var vulnerableEnemies = FindVulnerableEnemies();

            if (!vulnerableEnemies.Any())
                return actions;

            var enemiesAtSameIsland = vulnerableEnemies.Where(p => p.CurrentIsland == _player.CurrentIsland).ToList();

            if (enemiesAtSameIsland.Any())
            {
                foreach (var enemy in enemiesAtSameIsland)
                {
                    if (_player.getPlayerHealth() > enemy.getPlayerHealth() || _player.getPlayerHealth() > 15)
                    {
                        actions.Add(new CombatEncounter(_player, enemy, false));
                    }
                }

                return actions;
            }

            if (_player.Sword == null || _player.Sword is StoneSword)
                actions.Add(new BuyIronSword());

            if (_player.Sword != null && _player.Sword is IronSword)
                actions.Add(new BuyDiamondSword());

            if (_player.Sword != null && !_player._upgrades.Sharpness &&
                _player.Inventory.hasEnoughMoney(_player._upgrades.SharpnessCost))
                actions.Add(new BuySharpness());

            if (_player.getPlayerHealth() < 10)
                actions.Add(new BuyGoldenApple());

            if (_player.Inventory.HasItems<GoldenApple>(1) && _player.getPlayerHealth() < 15)
                actions.Add(new UseGoldenApple());

            var nearestEnemy = vulnerableEnemies.OrderBy(p =>
                CalculatePathDistance(_player.CurrentIsland, p.CurrentIsland)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                _targetIsland = nearestEnemy.CurrentIsland;

                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key == nearestEnemy.CurrentIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));
                        return actions;
                    }

                    int currentDist = CalculatePathDistance(_player.CurrentIsland, nearestEnemy.CurrentIsland);
                    int neighborDist = CalculatePathDistance(neighbor.Key, nearestEnemy.CurrentIsland);

                    if (neighborDist < currentDist)
                    {
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));
                        return actions;
                    }
                }
            }

            return actions;
        }

        private List<ICommand> GenerateImediateCombatActions()
        {
            var actions = new List<ICommand>();

            var enemies = _gameState.GetPlayers()
                .Where(p => p.getColor() != _player.getColor() &&
                            p.getIsAlive() &&
                            p.CurrentIsland == _player.CurrentIsland)
                .ToList();

            bool allBedsDestroyed = _gameState.GetBedIslands().All(b => !b.IsBedAlive());
            bool isCombatAdvantageous = false;

            if (enemies.Any())
            {
                var enemy = enemies.First();
                isCombatAdvantageous = (_player.getPlayerHealth() > enemy.getPlayerHealth() + 3) ||
                                       (_player.Sword != null && enemy.Sword == null) ||
                                       (_player.getPlayerHealth() > 15 && !allBedsDestroyed);
            }

            if (isCombatAdvantageous)
            {
                foreach (var enemy in enemies)
                {
                    actions.Add(new CombatEncounter(_player, enemy, false));
                }
            }
            else if (enemies.Any() && _player.getPlayerHealth() < 10)
            {
                if (_player.Inventory.HasItems<GoldenApple>(1))
                {
                    actions.Add(new UseGoldenApple());
                }
                else
                {
                    var safePath = FindSafePath();
                    if (safePath != null)
                    {
                        actions.Add(safePath);
                    }
                }
            }

            return actions;
        }

        private List<ICommand> GenerateMovementActions()
        {
            var actions = new List<ICommand>();

            if (_player.CurrentIsland == null)
                return actions;

            var enemyBedIslands = _gameState.GetBedIslands()
                .Where(b => b.GetColor() != _player.getColor() && b.IsBedAlive())
                .ToList();

            if (enemyBedIslands.Any())
            {
                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key is BedIsland bedIsland &&
                        bedIsland.GetColor() != _player.getColor() &&
                        bedIsland.IsBedAlive())
                    {
                        Console.WriteLine($"** {_player.getColor()} found path to {bedIsland.GetColor()}'s bed! **");
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));

                        return actions;
                    }
                }
            }

            if (_targetIsland != null)
            {
                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    int currentDistance = CalculatePathDistance(_player.CurrentIsland, _targetIsland);
                    int neighborDistance = CalculatePathDistance(neighbor.Key, _targetIsland);

                    if (neighborDistance < currentDistance)
                    {
                        if (neighbor.Value.IsComplete())
                            actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                        else
                            actions.Add(new BuildBridge(neighbor.Value, _player));
                        return actions;
                    }
                }
            }

            foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
            {
                if (neighbor.Value.IsComplete())
                {
                    actions.Add(new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player));
                }
                else
                {
                    bool isValueable = neighbor.Key is DiamondIsland ||
                                       neighbor.Key is EmeraldIsland ||
                                       (neighbor.Key is BedIsland &&
                                        ((BedIsland)neighbor.Key).GetColor() != _player.getColor());

                    if (isValueable)
                        actions.Add(new BuildBridge(neighbor.Value, _player));
                }
            }

            return actions;
        }

        private float EvaluateExpectedCombatOutcome(CombatEncounter combat)
        {
            Player enemy = null;
            if (combat._attacker == _player)
                enemy = combat._defender;
            else
                enemy = combat._attacker;

            float ourStrength = CalculateCombatStrength(_player);
            float enemyStrength = CalculateCombatStrength(enemy);

            float winProbability = ourStrength / (ourStrength + enemyStrength);

            float winReward = 100.0f;

            if (!enemy.HasBed())
                winReward += 5000.0f;

            if (_player.CurrentIsland is BedIsland bedIsland &&
                bedIsland.GetColor() != _player.getColor() &&
                bedIsland.IsBedAlive())
                winReward += 2000.0f;

            float losePenalty = -500.0f;

            if (!_player.HasBed())
                losePenalty -= 5000.0f;

            return winProbability * winReward + (1 - winProbability) * losePenalty;
        }

        private float CalculateCombatStrength(Player player)
        {
            float strength = 10.0f + player.CombatSkill * 2.5f;

            if (player.Sword != null)
            {
                if (player.Sword is DiamondSword)
                    strength += 21.0f;
                else if (player.Sword is IronSword)
                    strength += 12.0f;
                else
                    strength += 5.0f;
            }

            float healthFactor = player.getPlayerHealth() / 20.0f;
            strength *= healthFactor * 1.5f;

            if (player._upgrades.Sharpness)
                strength *= 1.4f;

            return strength;
        }

        private GamePhase DetermineGamePhase()
        {
            int currentTick = _gameState.GetCurrentTick();

            if (currentTick < EARLY_GAME_TICKS)
                return GamePhase.Early;
            else if (currentTick < MID_GAME_TICKS)
                return GamePhase.Mid;
            else
                return GamePhase.Late;
        }

        private List<ICommand> FilterViableActions(List<ICommand> actions)
        {
            return actions.Where(a => IsActionViable(a)).ToList();
        }

        private bool IsActionViable(ICommand action)
        {
            if (action is BuyStoneSword)
                return CanAffordItem(new StoneSword());
            else if (action is BuyIronSword)
                return CanAffordItem(new IronSword());
            else if (action is BuyDiamondSword)
                return CanAffordItem(new DiamondSword());
            else if (action is BuyWoodenPickaxe)
                return CanAffordItem(new WoodenPickaxe());
            else if (action is BuyIronPickaxe)
                return CanAffordItem(new IronPickaxe());
            else if (action is BuyDiamondPickaxe)
                return CanAffordItem(new DiamondPickaxe());
            else if (action is BuyShears)
                return CanAffordItem(new Shears());
            else if (action is BuyWool)
                return CanAffordItem(new Wool());
            else if (action is BuyEndstone)
                return CanAffordItem(new Endstone());
            else if (action is BuyObsidian)
                return CanAffordItem(new Obsidian());
            else if (action is BuyGoldenApple)
                return CanAffordItem(new GoldenApple());

            else if (action is BuySharpness)
                return _player.Sword != null &&
                       _player.Inventory.hasEnoughMoney(_player._upgrades.SharpnessCost) &&
                       !_player._upgrades.Sharpness;
            else if (action is BuyManiacMiner)
                return _player.Pickaxe != null &&
                       _player.Inventory.hasEnoughMoney(_player._upgrades.ManiacMinerCost) &&
                       !_player._upgrades.ManiacMiner;

            else if (action is Move moveAction)
                return moveAction._connection.IsComplete();

            else if (action is CombatEncounter)
                return _gameState.GetPlayers().Any(p =>
                    p.getColor() != _player.getColor() &&
                    p.getIsAlive() &&
                    p.CurrentIsland == _player.CurrentIsland);

            else if (action is MineBlockCommand mineAction)
                return _player.Pickaxe != null &&
                       mineAction._targetIsland.IsBedAlive() &&
                       mineAction._targetIsland.GetColor() != _player.getColor();

            else if (action is PlaceWool)
                return _player.Inventory.HasItems<Wool>(8);
            else if (action is PlaceEndstone)
                return _player.Inventory.HasItems<Endstone>(8);
            else if (action is PlaceObsidian)
                return _player.Inventory.HasItems<Obsidian>(4);

            else if (action is UseGoldenApple)
                return _player.Inventory.HasItems<GoldenApple>(1) &&
                       _player.getPlayerHealth() < 20;

            return true;
        }

        private bool CanAffordItem(Item item)
        {
            return _player.Inventory.hasEnoughMoney(item.Cost);
        }

        private List<ICommand> GenerateAllPossibleActions()
        {
            var actions = new List<ICommand>();

            actions.AddRange(GenerateResourceGatheringActions());
            actions.AddRange(GenerateBedDestroyingActions());
            actions.AddRange(GenerateImediateCombatActions());
            actions.AddRange(GenerateDefensiveActions());
            actions.AddRange(GenerateMovementActions());

            return actions;
        }

        private List<ICommand> GenerateActionsForPlayer(Player player, GameState state)
        {
            var actions = new List<ICommand>();

            if (player.CurrentIsland == _player.CurrentIsland)
                actions.Add(new CombatEncounter(player, _player, false));

            if (player.CurrentIsland == _player.HomeIsland && _player.HomeIsland.IsBedAlive())
                actions.Add(new MineBlockCommand(_player.HomeIsland, player));

            foreach (var neighbor in player.CurrentIsland.GetNeighbors())
            {
                if (neighbor.Key == _player.HomeIsland)
                {
                    if (neighbor.Value.IsComplete())
                        actions.Add(new Move(player.CurrentIsland, _player.HomeIsland, neighbor.Value, player));
                    else if (CalculatePathDistance(neighbor.Key, _player.HomeIsland) <
                             CalculatePathDistance(player.CurrentIsland, _player.HomeIsland))
                        actions.Add(new BuildBridge(neighbor.Value, player));
                }
            }

            return actions;
        }

        private float EvaluateGameState(GameState state)
        {
            float score = 0;

            if (!_player.getIsAlive())
                return float.NegativeInfinity;

            score += _player.getPlayerHealth() * ((_player.HasBed() ? 10 : 50));

            bool ourBedAlive = _player.HasBed();
            score += ourBedAlive ? BED_DESTROY_PRIORITY : -BED_DESTROY_PRIORITY / 2;

            int enemyBedsDestroyed = 0;
            foreach (var island in state.GetBedIslands())
            {
                if (island.GetColor() != _player.getColor() && !island.IsBedAlive())
                    enemyBedsDestroyed++;
            }

            score += enemyBedsDestroyed * BED_DESTROY_PRIORITY * 2;

            bool allBedsDestroyed = state.GetBedIslands().All(b => !b.IsBedAlive());
            if (allBedsDestroyed)
            {
                score += _player.getPlayerHealth() * (SURVIVAL_PRIORITY * 0.5f);

                if (_player.getPlayerHealth() > 10)
                {
                    score += SURVIVAL_PRIORITY * 0.5f;
                }

                var enemies = state.GetPlayers()
                    .Where(p => p.getColor() != _player.getColor() && p.getIsAlive())
                    .ToList();

                if (enemies.Any())
                {
                    var closestEnemy = enemies
                        .OrderBy(p => CalculatePathDistance(_player.CurrentIsland, p.CurrentIsland))
                        .First();

                    int distanceToClosest = CalculatePathDistance(_player.CurrentIsland, closestEnemy.CurrentIsland);
                    score += (10 - Math.Min(distanceToClosest, 10)) * 1000f;

                    if (_player.CurrentIsland == closestEnemy.CurrentIsland)
                        score += 5000f;
                }

                int playersLeft = state.GetPlayers().Count(p => p.getIsAlive());
                if (playersLeft > 1)
                {
                    int enemiesKilled = 4 - playersLeft;
                    score += enemiesKilled * PLAYER_KILL_PRIORITY * 5; // Increased reward for kills
                }
            }
            else
            {
                if (_player.CurrentIsland is DiamondIsland)
                    score += RESOURCE_PRIORITY;
                if (_player.CurrentIsland is EmeraldIsland)
                    score += RESOURCE_PRIORITY * 2;

                if (_player.CurrentIsland is BedIsland bedIsland &&
                    bedIsland.GetColor() != _player.getColor() &&
                    bedIsland.IsBedAlive())
                    score += BED_DESTROY_PRIORITY * 4f;

                score += _player.HomeIsland.DefenseLayers.Count * DEFENSE_PRIORITY;

                int enemiesWithoutBeds = state.GetPlayers()
                    .Count(p => p.getColor() != _player.getColor() && p.getIsAlive() && !p.HasBed());

                score += enemiesWithoutBeds * PLAYER_KILL_PRIORITY;
            }

            return score;
        }

        private GameState SimulateAction(GameState currentState, ICommand action)
        {
            return currentState;
        }

        private GameState SimulateEnemyAction(GameState currentState, ICommand action, Player enemy)
        {
            return currentState;
        }

        private ICommand GetDefaultAction()
        {
            if (_player.getPlayerHealth() < 10 && _player.Inventory.HasItems<GoldenApple>(1))
            {
                return new UseGoldenApple();
            }

            if (_player.CurrentIsland == _player.HomeIsland)
            {
                if (_player.Pickaxe == null && _player.Inventory.hasEnoughMoney(new WoodenPickaxe().Cost))
                    return new BuyWoodenPickaxe();

                if (_player.Sword == null && _player.Inventory.hasEnoughMoney(new StoneSword().Cost))
                    return new BuyStoneSword();

                foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
                {
                    if (neighbor.Key is DiamondIsland)
                    {
                        if (neighbor.Value.IsComplete())
                            return new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player);
                        else
                            return new BuildBridge(neighbor.Value, _player);
                    }
                }
            }

            foreach (var neighbor in _player.CurrentIsland.GetNeighbors())
            {
                if (neighbor.Key == _player.HomeIsland)
                {
                    if (neighbor.Value.IsComplete())
                        return new Move(_player.CurrentIsland, neighbor.Key, neighbor.Value, _player);
                    else
                        return new BuildBridge(neighbor.Value, _player);
                }
            }

            if (_player.Inventory.HasItems<GoldenApple>(1) && _player.getPlayerHealth() < 20)
                return new UseGoldenApple();

            if (_player.Inventory.hasEnoughMoney(new GoldenApple().Cost))
                return new BuyGoldenApple();

            return new BuyWool();
        }

        private enum GamePhase
        {
            Early,
            Mid,
            Late
        }

        private enum StrategicGoal
        {
            GatherResources,
            DestroyBeds,
            DefendBase,
            EliminatePlayers
        }
    }
}