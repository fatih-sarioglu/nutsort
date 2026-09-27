# Nut Sort

A small nuts and bolts sorting puzzle made in Unity.

I kept seeing these sort games on mobile and got curious about how they actually work. So I made my own version to figure it out.

## How to play

Tap a bolt to lift the nuts on top, then tap another bolt to move them there. Nuts can only go onto an empty bolt or onto a nut of the same color. When a bolt is filled with a single color it gets capped and locked. Sort every color to finish the level.

- Undo and restart
- Hint button that highlights the next move
- 1 to 3 stars depending on how close you get to the minimum moves
- 10 levels

## Project structure

All scripts are under `Assets/_Project/Scripts`:

- **Core**: the game logic, plain C# without any Unity code
  - `BoardState` holds the board, each bolt is a list of color ids
  - `MoveRules` checks if a move is allowed
  - `GameSession` applies moves, handles undo/restart and fires events
  - `BfsSolver` finds the shortest solution with a breadth-first search, used for the hint button
- **Data**: loads the levels from JSON (`LevelParser`, `LevelCatalog`)
- **Presentation**: everything on screen
  - `BoardView` spawns the bolts and nuts and plays the animations with DOTween
  - `LevelController` connects input, the game session and the UI for one level
  - `GameBootstrapper` handles the menu and switching levels

The logic updates right away and the view just listens to the events and animates, so the animations never block the input.

## Levels

Levels are JSON files in `Assets/_Project/Levels`. Bolts are listed from bottom to top:

```json
{
  "id": 1,
  "capacity": 3,
  "minMoves": 4,
  "bolts": [
    ["blue", "blue", "red"],
    ["red", "blue", "red"],
    []
  ]
}
```

Every color has to appear exactly `capacity` times. `minMoves` is used for the star rating. To add a new level, create a JSON file and add it to the `LevelCatalog` asset in `Assets/_Project/ScriptableObjects`.

## Running

Open the project with Unity 6, open `Assets/_Project/Scenes/Game.unity` and press Play.
