//    https://tetris.fandom.com/wiki/Tetris_Guideline to know what shape represent the letter
namespace Tetris.Game;

public enum TetrominoType
{
    I //TODO: Add more pieces
    // each tetris piece, or "Tetrimino" as its called officially.
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
        } 
    };

 
    public static int GridSize(TetrominoType type) => type == TetrominoType.I ? 4 : (type == TetrominoType.O ? 4 : 4);

    public static (int row, int col)[] GetCells(TetrominoType type, int rotation)
    {
        var states = Shapes[type];
        return states[((rotation % 4) + 4) % 4];
    }

    public static string GetColorHex(TetrominoType type) => type switch // Each tetromino colors according to the rules.
    {
        TetrominoType.I => "#00e5ff", // cyan
        //TODO: Add more colors that represents each individual piece
    };

    public static IReadOnlyList<TetrominoType> All { get; } =
        Enum.GetValues<TetrominoType>().ToList();
}