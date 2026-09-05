//    https://tetris.fandom.com/wiki/Tetris_Guideline to know what shape represent the letter
namespace Tetris.Game;

public enum TetrominoType
{
    I, O, T, S, Z, J, L
}


public static class Tetromino
{
     // Each rotation for each tetrimino type.
    private static readonly Dictionary<TetrominoType, (int row, int col)[][]> Shapes = new()
    {
        [TetrominoType.I] = new[]
        {
            new[] { (1, 0), (1, 1), (1, 2), (1, 3) },
            new[] { (0, 2), (1, 2), (2, 2), (3, 2) },
            new[] { (2, 0), (2, 1), (2, 2), (2, 3) },
            new[] { (0, 1), (1, 1), (2, 1), (3, 1) },
        },
        [TetrominoType.O] = new[]
        {
            new[] { (0, 1), (0, 2), (1, 1), (1, 2) },
            new[] { (0, 1), (0, 2), (1, 1), (1, 2) },
            new[] { (0, 1), (0, 2), (1, 1), (1, 2) },
            new[] { (0, 1), (0, 2), (1, 1), (1, 2) },
        },
        [TetrominoType.T] = new[]
        {
            new[] { (0, 1), (1, 0), (1, 1), (1, 2) },
            new[] { (0, 1), (1, 1), (1, 2), (2, 1) },
            new[] { (1, 0), (1, 1), (1, 2), (2, 1) },
            new[] { (0, 1), (1, 0), (1, 1), (2, 1) },
        },
        [TetrominoType.S] = new[]
        {
            new[] { (0, 1), (0, 2), (1, 0), (1, 1) },
            new[] { (0, 1), (1, 1), (1, 2), (2, 2) },
            new[] { (1, 1), (1, 2), (2, 0), (2, 1) },
            new[] { (0, 0), (1, 0), (1, 1), (2, 1) },
        },
        [TetrominoType.Z] = new[]
        {
            new[] { (0, 0), (0, 1), (1, 1), (1, 2) },
            new[] { (0, 2), (1, 1), (1, 2), (2, 1) },
            new[] { (1, 0), (1, 1), (2, 1), (2, 2) },
            new[] { (0, 1), (1, 0), (1, 1), (2, 0) },
        },
        [TetrominoType.J] = new[]
        {
            new[] { (0, 0), (1, 0), (1, 1), (1, 2) },
            new[] { (0, 1), (0, 2), (1, 1), (2, 1) },
            new[] { (1, 0), (1, 1), (1, 2), (2, 2) },
            new[] { (0, 1), (1, 1), (2, 0), (2, 1) },
        },
        [TetrominoType.L] = new[]
        {
            new[] { (0, 2), (1, 0), (1, 1), (1, 2) },
            new[] { (0, 1), (1, 1), (2, 1), (2, 2) },
            new[] { (1, 0), (1, 1), (1, 2), (2, 0) },
            new[] { (0, 0), (0, 1), (1, 1), (2, 1) },
        },
    };

    public static int GridSize(TetrominoType type) => 4;

    public static (int row, int col)[] GetCells(TetrominoType type, int rotation)
    {
        var states = Shapes[type];
        return states[((rotation % 4) + 4) % 4];
    }

    public static string GetColorHex(TetrominoType type) => type switch  // Each tetromino colors according to the rules.
    {
        TetrominoType.I => "#00e5ff", //cyan I
        TetrominoType.O => "#ffd500", // yellow O
        TetrominoType.T => "#c264ff",//purple T
        TetrominoType.S => "#4dff4d",// green S
        TetrominoType.Z => "#ff4d4d", //red Z
        TetrominoType.J => "#4d7bff", // blue  L
        TetrominoType.L => "#ff9f1a",   //orange L
        _ => "#ffffff"
    };

    public static IReadOnlyList<TetrominoType> All { get; } =
        Enum.GetValues<TetrominoType>().ToList();
}

