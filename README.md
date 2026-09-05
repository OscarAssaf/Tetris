# Tetris

A complete Tetris game built with Blazor WebAssembly, .NET 10 and C#. 

The project was designed to be hosted for free utilizing GitHub Pages and can be played on over at the deployed page https://oscarassaf.github.io/Tetris

## Features

- Standard 10x20 board, following Tetris 7-bag piece randomizer rule
- Hold piece, next-piece preview, ghost piece (landing preview)
- Soft drop / hard drop, wall kicks on rotation
- Scoring, levels, and increasing speed system
- Keyboard controls, pause, and game-over/restart screen
- No external game engine or JS dependency, as the game logic is built in plain C# and can be played on a browser.

## Controls

| Key | Action |
|---|---|
| ← / → | Move left / right |
| ↑ | Rotate |
| ↓ | Soft drop |
| Space | Hard drop |
| C | Hold piece |
| P | Pause / resume |

## Running locally

You'll need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed (this project is pinned to SDK `10.0.9` via `global.json`).

```bash
dotnet restore
dotnet run
```

Then open the URL shown in the terminal (e.g. `http://localhost:5140`).

For hot-reload during development:

```bash
dotnet watch
```

## Project structure

```
Tetris.csproj              Project file (Blazor WebAssembly, net10.0)
global.json                Pins the .NET SDK version (10.0.9)
Program.cs                 App entry point / WASM host setup
App.razor                  Root router
_Imports.razor             Shared using directives for .razor files
Layout/MainLayout.razor    Page layout wrapper
Pages/Tetris.razor         The game page: rendering, input, game loop timer
Game/GameBoard.cs          Core engine: movement, collision, locking, line clears, scoring
Game/ActivePiece.cs        The currently falling piece (position + rotation state)
Game/Tetromino.cs          Shape/rotation/color definitions for all 7 piece types
Game/PieceBag.cs           7-bag randomizer
wwwroot/index.html         HTML shell that bootstraps Blazor WASM
wwwroot/css/app.css        Styling
.github/workflows/deploy.yml   GitHub Actions workflow which builds and deploys the project to GitHub Pages
```


