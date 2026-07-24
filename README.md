# Tetris

A modern implementation of the classic **Tetris** game built with **Unity** and **C#**.

---

## Features

- 🎮 Classic Tetris gameplay
- 🧩 All seven standard tetrominoes
- ⬅️➡️ Horizontal movement
- 🔄 Piece rotation
- ⬇️ Soft drop
- ⚡ Hard drop
- 🧱 Collision detection
- 🧹 Line clearing
- 📈 Leveling system and increasing difficulty over time
---

## Controls

| Key | Action |
|------|--------|
| A | Move Left |
| D | Move Right |
| S | Soft Drop |
| W | Rotate |
| H | Hold Piece |
| Space | Hard Drop |

---

## Built With

- Unity
- C#
- VS Code

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Board.cs
│   ├── Piece.cs
│   ├── TetrominoData.cs
│   ├── GameManager.cs
│   └── ...
├── Prefabs/
├── Sprites/
└── Scenes/
```

The project follows a modular architecture where:

- **Board** manages the game grid and completed lines.
- **Piece** controls movement, rotation, collision, and locking.
- **TetrominoData** stores the information for each tetromino.
- Additional scripts handle game flow and UI.

---

## Getting Started

### Prerequisites

- Unity 6 (or the version used in this project)
- Visual Studio or another C# IDE

### Installation

Clone the repository:

```bash
git clone https://github.com/danielmaavre/Tetris.git
```

Open the project with Unity and load the main scene.

Press **Play** to start the game.

---

## Roadmap

Planned improvements include:

- [x] Hold piece
- [ ] Next piece preview
- [x] Ghost piece
- [x] Score system
- [x] Level progression
- [ ] Sound effects
- [ ] Music
- [ ] Main menu
- [ ] Pause menu
- [ ] High score saving
- [ ] Visual polish and animations

---

## Learning Goals

This project focuses on understanding:

- Unity's game loop
- Grid-based game logic
- Object-oriented programming
- Collision detection
- Data-driven game design
- Game state management

---

## Disclaimer

This project is an independent educational implementation inspired by the classic Tetris game. It was developed exclusively for learning purposes and is not affiliated with or endorsed by The Tetris Company.