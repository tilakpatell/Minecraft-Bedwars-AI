# Bedwars AI Simulation

A C# simulation environment for testing AI strategies in a Bedwars-style game, inspired by the popular Minecraft minigame. This project implements a custom game engine with different AI agent strategies competing against each other.

## Overview

This simulation models a Bedwars game where players start on their home islands, gather resources, build bridges to other islands, upgrade their equipment, and attempt to destroy other players' beds. Once a player's bed is destroyed, they cannot respawn when eliminated. The last player standing wins.

## Game Elements

### Islands
- **Bed Islands**: Home bases for players with resource generators (iron and gold)
- **Diamond Islands**: Generate diamond resources
- **Emerald Islands**: Generate emerald resources

### Resources
- **Iron**: Basic resource, generated fastest
- **Gold**: Intermediate resource, generated at a moderate rate
- **Diamond**: Advanced resource, generated slowly
- **Emerald**: Premium resource, generated most slowly

### Items
- **Weapons**: Stone, Iron, and Diamond swords
- **Tools**: Wooden, Iron, and Diamond pickaxes
- **Defensive Blocks**: Wool, Endstone, Obsidian
- **Consumables**: Golden Apple (heals the player)

### Mechanics
- **Bridges**: Players must build bridges to travel between islands
- **Combat**: Players can engage in combat to eliminate opponents
- **Bed Breaking**: Destroying an opponent's bed prevents them from respawning
- **Resource Collection**: Players automatically collect resources from islands they occupy

## AI Strategies

The simulation includes four different AI strategies:

1. **Optimal Player (Red)**: Uses sophisticated minimax decision making with alpha-beta pruning to make intelligent decisions
2. **Aggressive Player (Blue)**: Focuses on combat and bed destruction, often at the expense of resource gathering
3. **Resource Player (Green)**: Prioritizes resource collection and upgrades before engaging in combat
4. **Defensive Player (Yellow)**: Builds strong defenses and plays conservatively

## Project Structure

- **Core Game Logic**:
 - `AIsland.cs`: Base class for all islands
 - `BedIsland.cs`, `DiamondIsland.cs`, `EmeraldIsland.cs`: Specific island types
 - `Connection.cs`: Represents bridges between islands
 - `GameEngine.cs`: Manages game simulation
 - `GameState.cs`: Stores the current state of the game
 - `Generator.cs`: Handles resource generation
 - `Player.cs`: Base player class
 - `Inventory.cs`: Player inventory management

- **AI Logic**:
 - `BedwarsAIAgent.cs`: Main AI decision-making engine using minimax
 - Strategy-specific player classes in `Program.cs`

- **Items and Commands**:
 - Various item classes (sword, pickaxe, etc.)
 - Command classes for actions (move, combat, buy, etc.)

## How to Run

1. Compile the project using a C# compiler (.NET Core compatible)
2. Run the executable to start the simulation
3. The program will run multiple simulations and output statistics about which AI strategy performs best

```bash
dotnet build
dotnet run
