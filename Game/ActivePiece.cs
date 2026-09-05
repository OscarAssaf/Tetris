namespace Tetris.Game;



/// Represents the currently falling piece: its type, rotation state,
public class ActivePiece
{
    public TetrominoType Type { get; }
    public int Rotation { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }

    public ActivePiece(TetrominoType type, int startRow, int startCol)
    {
        Type = type;
        Rotation = 0;
        Row = startRow;
        Col = startCol;
    }


    /// Returns the (row, col) board positions this piece currently occupies.

    public IEnumerable<(int row, int col)> GetAbsoluteCells()
    {
        foreach (var (r, c) in Tetromino.GetCells(Type, Rotation))
        {
            yield return (Row + r, Col + c);
        }
    }


    /// Returns the absolute cells for a hypothetical rotation/position without mutating
    public IEnumerable<(int row, int col)> GetAbsoluteCellsFor(int rotation, int row, int col)
    {
        foreach (var (r, c) in Tetromino.GetCells(Type, rotation))
        {
            yield return (row + r, col + c);
        }
    }

    public ActivePiece Clone()
    {
        return new ActivePiece(Type, Row, Col) { Rotation = Rotation };
    }
}
