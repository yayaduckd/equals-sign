# Tetris Game

A fully functional Tetris game implemented in HTML, CSS, and JavaScript!

![Tetris Screenshot](https://github.com/user-attachments/assets/b08b9fa7-3653-424d-9dba-76832eed13e2)

## How to Play

1. Open `tetris.html` in your web browser
2. Use the following controls:
   - **←** **→** Arrow keys: Move piece left/right
   - **↓** Arrow key: Soft drop (move piece down faster)
   - **↑** Arrow key: Rotate piece
   - **Space**: Hard drop (instantly drop piece to bottom)
   - **P**: Pause/unpause game

## Game Features

- **Classic Tetris gameplay** with all 7 traditional piece shapes (I, O, T, S, Z, J, L)
- **Line clearing** - Complete horizontal lines to score points and clear them
- **Progressive difficulty** - Game speed increases every 10 lines cleared
- **Scoring system** - Earn points for dropping pieces and clearing lines
- **Level progression** - Higher levels = faster gameplay
- **Game over detection** - Game ends when pieces reach the top
- **Responsive controls** - Smooth piece movement and rotation
- **Beautiful styling** - Modern gradient background and clean UI

## Technical Details

- Single HTML file with embedded CSS and JavaScript
- Canvas-based rendering for smooth graphics
- Game loop with collision detection
- No external dependencies - runs in any modern browser
- Fully responsive design

## Running the Game

Simply open `tetris.html` in any modern web browser. For local development, you can serve it with:

```bash
python3 -m http.server 8000
# Then open http://localhost:8000/tetris.html
```

Enjoy playing Tetris!
