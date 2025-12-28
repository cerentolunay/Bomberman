# Bomberman — Online Multiplayer Game

Bomberman-inspired game developed using **Unity and C#**, featuring both
**single-player** and **online multiplayer (host–client)** gameplay.

The project focuses on building a **clean, extensible architecture**
while implementing classic Bomberman mechanics such as grid-based movement,
bomb placement, destructible environments, and power-ups.

---

## Overview

This project recreates the core gameplay loop of the classic Bomberman series:

- Grid-based tilemap movement
- Bomb placement with delayed explosions
- Directional explosion propagation
- Destructible and indestructible walls
- Power-ups that enhance player abilities
- Enemy AI with different behaviors
- Online multiplayer with synchronized game state
- Local persistence of player and match data

---

## Key Features

- **Tilemap-based grid system**
- **Bomb mechanics**
  - Timed detonation
  - Explosion propagation in four directions
  - Interaction with walls, players, and enemies
- **Wall types**
  - Unbreakable walls
  - Breakable walls
- **Power-ups**
  - Increased movement speed
  - Increased bomb power
  - Increased bomb count
- **Single-player mode**
  - AI-controlled enemies with interchangeable behaviors
- **Multiplayer mode**
  - Host–client architecture
  - Authoritative host for gameplay validation
- **Player data persistence**
  - Match statistics
  - Leaderboard support using SQLite

---

## Technology Stack

- **Game Engine:** Unity
- **Programming Language:** C#
- **Networking:** Host–Client multiplayer model
- **Database:** SQLite

---

## Architecture

Although Unity uses a component-based approach, the project follows an
**MVC-inspired layered architecture** to improve clarity and maintainability.

- **Model**
  - Game rules, player statistics, match data
  - Wall, bomb, and power-up definitions
- **Controller**
  - Input handling
  - Bomb and explosion logic
  - Enemy AI behaviors
  - Game state transitions
- **View**
  - Scenes, prefabs, UI panels, animations, and visual effects

This structure keeps gameplay logic independent from presentation details.

---

## Design Patterns

The project applies several design patterns to keep the codebase modular
and extensible.

- **Factory** – Theme-based wall and map generation
- **Decorator** – Dynamic power-up system
- **Strategy** – Interchangeable enemy AI behaviors
- **State** – Game flow management (menu, play, pause, game over)
- **Command** – Encapsulated player input actions
- **Template Method** – Structured map generation workflow
- **Repository** – Abstracted SQLite data access
- **Singleton** – Global game services
- **Facade** – Simplified access to core subsystems
- **Adapter** – Logical grid abstraction over Unity Tilemaps

---

## Getting Started

### Requirements
- Unity (compatible version with the project)
- No external database setup required

### Run the Project
1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```
2.Open the project via Unity Hub.
3.Load the main scene (e.g., MainMenu).
4.Press Play to start.

## Multiplayer
One player starts the session as Host
Other players join as Clients
The host is responsible for:
Validating player actions
Synchronizing bombs and explosions
Managing game state transitions

## Controls
Default controls (may vary by configuration):
Move: W / A / S / D or Arrow Keys
Place Bomb: Space
Pause: Esc

## Data Storage

The project uses SQLite for local storage of:
Player profiles
Match statistics
Leaderboard data
All database interactions are handled through repository classes,
keeping persistence logic separate from gameplay logic.

## Project Structure
```bash
   Assets/
    Scripts/
        Model/
        Controller/
        View/
        Patterns/
  Prefabs/
  Scenes/

   ```
## Notes
The project is designed to be easily extensible.
Additional game modes, themes, or AI strategies can be added
without modifying existing core systems.