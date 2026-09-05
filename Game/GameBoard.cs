// taken inspiration from https://github.com/delpitec/Csharp_.NETFramework_WindowsForms_Tetris
namespace Tetris.Game;

public enum MoveDirection { Left, Right }

//Game engine for the tetris 
public class GameBoard
{
    public const int Width = 10;
    public const int Height = 20;

    /// Locked cells on the board
    public string?[,] Grid { get; private set; } = new string?[Height, Width];

    public ActivePiece? Current { get; private set; }
    public TetrominoType NextType { get; private set; }
    public TetrominoType? HoldType { get; private set; }
    private bool _holdUsedThisTurn;

    public int Score { get; private set; }
    public int Lines { get; private set; }
    public int Level { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; set; }

    /// Rows that were just cleared
    public List<int> LastClearedRows { get; private set; } = new();

    private readonly PieceBag _bag;

    public event Action? OnChanged;
    public event Action? OnLineClear;
    public event Action? OnGameOver;
    public event Action? OnPieceLocked;

    public GameBoard(Random? random = null)
    {
        _bag = new PieceBag(random);
        NextType = _bag.Next();
        SpawnNext();
    }

    public void Reset()
    {
        Grid = new string?[Height, Width];
        Score = 0;
        Lines = 0;
        Level = 0;
        IsGameOver = false;
        IsPaused = false;
        HoldType = null;
        _holdUsedThisTurn = false;
        LastClearedRows.Clear();
        NextType = _bag.Next();
        SpawnNext();
        OnChanged?.Invoke();
    }

    /// Time it takes for the piece to fall, faster based on the level you're at
    public int GetDropIntervalMs()
    {
       
        int level = Math.Min(Level, 20);
        double seconds = Math.Pow(0.8 - (level * 0.007), level);
        int ms = (int)(seconds * 1000);
        return Math.Clamp(ms, 80, 1000);
    }

    private void SpawnNext()
    {
        var type = NextType;
        NextType = _bag.Next();
        int gridSize = Tetromino.GridSize(type);
        int startCol = (Width - gridSize) / 2;
        Current = new ActivePiece(type, 0, startCol);
        _holdUsedThisTurn = false;

        if (!IsValidPosition(Current, Current.Rotation, Current.Row, Current.Col))
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
        }
    }

    private bool IsValidPosition(ActivePiece piece, int rotation, int row, int col)
    {
        foreach (var (r, c) in piece.GetAbsoluteCellsFor(rotation, row, col))
        {
            if (c < 0 || c >= Width) return false;
            if (r >= Height) return false;
            if (r < 0) continue; 
            if (Grid[r, c] is not null) return false;
        }
        return true;
    }

    public void MoveLeft() => TryMove(MoveDirection.Left);
    public void MoveRight() => TryMove(MoveDirection.Right);

    private void TryMove(MoveDirection dir)
    {
        if (Current is null || IsGameOver || IsPaused) return;
        int delta = dir == MoveDirection.Left ? -1 : 1;
        int newCol = Current.Col + delta;
        if (IsValidPosition(Current, Current.Rotation, Current.Row, newCol))
        {
            Current.Col = newCol;
            OnChanged?.Invoke();
        }
    }
    //Logic for rotating the piece
    public void Rotate(bool clockwise = true)
    {
        if (Current is null || IsGameOver || IsPaused) return;
        int newRotation = clockwise ? Current.Rotation + 1 : Current.Rotation - 1;

      
        (int dRow, int dCol)[] kicks = { (0, 0), (0, -1), (0, 1), (0, -2), (0, 2), (-1, 0) };
        foreach (var (dRow, dCol) in kicks)
        {
            int newRow = Current.Row + dRow;
            int newCol = Current.Col + dCol;
            if (IsValidPosition(Current, newRotation, newRow, newCol))
            {
                Current.Rotation = newRotation;
                Current.Row = newRow;
                Current.Col = newCol;
                OnChanged?.Invoke();
                return;
            }
        }
        // No valid kick found; rotation is rejected.
    }


    public bool Tick()
    {
        if (Current is null || IsGameOver || IsPaused) return false;

        if (IsValidPosition(Current, Current.Rotation, Current.Row + 1, Current.Col))
        {
            Current.Row++;
            OnChanged?.Invoke();
            return true;
        }

        LockPiece();
        return false;
    }

    // Soft drop as its called, when you manually press the down button (s) or arrow down key in order to make the piece slowly fall down
    public void SoftDrop()
    {
        if (Current is null || IsGameOver || IsPaused) return;
        if (IsValidPosition(Current, Current.Rotation, Current.Row + 1, Current.Col))
        {
            Current.Row++;
            Score += 1;
            OnChanged?.Invoke();
        }
        else
        {
            LockPiece();
        }
    }

    /// Hard drop executed by pressing the spacebar which makes the piece instantly drop to the bottom of its current location.
    public void HardDrop()
    {
        if (Current is null || IsGameOver || IsPaused) return;
        int distance = 0;
        while (IsValidPosition(Current, Current.Rotation, Current.Row + 1, Current.Col))
        {
            Current.Row++;
            distance++;
        }
        Score += distance * 2;
        LockPiece();
    }

    // Ghost piece, which essentially showcases where the  current piece would land on if it were to fall to the bottom on its current orientation
    public int GetGhostRow()
    {
        if (Current is null) return 0;
        int row = Current.Row;
        while (IsValidPosition(Current, Current.Rotation, row + 1, Current.Col))
        {
            row++;
        }
        return row;
    }

    public void HoldPiece()
    {
        if (Current is null || IsGameOver || IsPaused || _holdUsedThisTurn) return;

        var currentType = Current.Type;
        if (HoldType is null)
        {
            HoldType = currentType;
            SpawnNext();
        }
        else
        {
            var swap = HoldType.Value;
            HoldType = currentType;
            int gridSize = Tetromino.GridSize(swap);
            int startCol = (Width - gridSize) / 2;
            Current = new ActivePiece(swap, 0, startCol);
            if (!IsValidPosition(Current, Current.Rotation, Current.Row, Current.Col))
            {
                IsGameOver = true;
                OnGameOver?.Invoke();
            }
        }
        _holdUsedThisTurn = true;
        OnChanged?.Invoke();
    }

    private void LockPiece()
    {
        if (Current is null) return;

        string color = Tetromino.GetColorHex(Current.Type);
        foreach (var (r, c) in Current.GetAbsoluteCells())
        {
            if (r >= 0 && r < Height && c >= 0 && c < Width)
            {
                Grid[r, c] = color;
            }
        }

        OnPieceLocked?.Invoke();
        ClearLines();
        SpawnNext();
        OnChanged?.Invoke();
    }

    private void ClearLines()
    {
        var fullRows = new List<int>();
        for (int r = 0; r < Height; r++)
        {
            bool full = true;
            for (int c = 0; c < Width; c++)
            {
                if (Grid[r, c] is null) { full = false; break; }
            }
            if (full) fullRows.Add(r);
        }

        LastClearedRows = fullRows;

        if (fullRows.Count == 0) return;

        // logic for essentially removing a row and moving everything down once its been cleared.
        var newGrid = new string?[Height, Width];
        int writeRow = Height - 1;
        for (int r = Height - 1; r >= 0; r--)
        {
            if (fullRows.Contains(r)) continue;
            for (int c = 0; c < Width; c++)
            {
                newGrid[writeRow, c] = Grid[r, c];
            }
            writeRow--;
        }
        Grid = newGrid;

        Lines += fullRows.Count;
        Score += ScoreForLines(fullRows.Count, Level);
        Level = Lines / 10;

        OnLineClear?.Invoke();
    }

    private static int ScoreForLines(int count, int level) => count switch
    {
        1 => 100 * (level + 1),
        2 => 300 * (level + 1),
        3 => 500 * (level + 1),
        4 => 800 * (level + 1), 
        _ => 0
    };
}
