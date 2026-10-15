using System.Reflection;
using Tetris.Game;
using Xunit;

namespace Tetris.Tests;

public class TetrominoTests
{
    [Theory]
    [MemberData(nameof(AllPieceRotations))]
    public void Every_piece_rotation_has_four_unique_cells_in_four_by_four_box(TetrominoType type, int rotation)
    {
        var cells = Tetromino.GetCells(type, rotation);
        Assert.Equal(4, cells.Length);
        Assert.Equal(4, cells.Distinct().Count());
        Assert.All(cells, cell => Assert.InRange(cell.row, 0, 3));
        Assert.All(cells, cell => Assert.InRange(cell.col, 0, 3));
    }

    public static IEnumerable<object[]> AllPieceRotations() =>
        from type in Tetromino.All from rotation in Enumerable.Range(0, 4) select new object[] { type, rotation };

    [Theory]
    [InlineData(-5, 3)]
    [InlineData(4, 0)]
    [InlineData(9, 1)]
    public void Rotation_indices_wrap_in_both_directions(int rotation, int equivalent) =>
        Assert.Equal(Tetromino.GetCells(TetrominoType.T, equivalent), Tetromino.GetCells(TetrominoType.T, rotation));

    [Fact]
    public void Every_piece_has_a_color_and_is_listed_once() {
        Assert.Equal(7, Tetromino.All.Count);
        Assert.Equal(7, Tetromino.All.Distinct().Count());
        Assert.All(Tetromino.All, type => Assert.StartsWith("#", Tetromino.GetColorHex(type)));
    }
}

public class ActivePieceTests
{
    [Fact]
    public void Absolute_cells_apply_piece_origin_and_clone_is_independent()
    {
        var piece = new ActivePiece(TetrominoType.T, 5, 2) { Rotation = 1 };
        var cells = piece.GetAbsoluteCells().ToArray();
        Assert.Equal(Tetromino.GetCells(TetrominoType.T, 1).Select(c => (c.row + 5, c.col + 2)), cells);
        Assert.Equal(cells, piece.GetAbsoluteCellsFor(1, 5, 2));

        var clone = piece.Clone();
        clone.Row++;
        clone.Rotation++;
        Assert.Equal(5, piece.Row);
        Assert.Equal(1, piece.Rotation);
        Assert.Equal(piece.Type, clone.Type);
    }
}

public class PieceBagTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(999)]
    public void Each_seven_piece_batch_contains_each_type_once(int seed)
    {
        var bag = new PieceBag(new Random(seed));
        for (var batch = 0; batch < 20; batch++)
            Assert.Equal(Tetromino.All.OrderBy(x => (int)x), Enumerable.Range(0, 7).Select(_ => bag.Next()).OrderBy(x => (int)x));
    }

    [Fact]
    public void Peek_does_not_consume_the_next_piece()
    {
        var bag = new PieceBag(new Random(123));
        Assert.Equal(bag.PeekNext(), bag.Next());
    }
}

public class GameBoardTests
{
    [Fact]
    public void Board_starts_with_valid_piece_and_expected_dimensions()
    {
        var board = new GameBoard(new Random(1));
        Assert.Equal(20, board.Grid.GetLength(0));
        Assert.Equal(10, board.Grid.GetLength(1));
        Assert.NotNull(board.Current);
        Assert.Equal(0, board.Score);
        Assert.Equal(0, board.Lines);
        Assert.False(board.IsGameOver);
        Assert.Equal(1000, board.GetDropIntervalMs());
    }

    [Fact]
    public void Movement_stops_at_walls_and_notifies_only_on_success()
    {
        var board = new GameBoard(new Random(2));
        var piece = SetPiece(board, TetrominoType.O, 0, 4);
        var events = 0;
        board.OnChanged += () => events++;
        for (var i = 0; i < 10; i++) board.MoveLeft();
        Assert.Equal(-1, piece.Col);
        Assert.Equal(5, events); // O cells begin one column into the piece's 4x4 box.
        for (var i = 0; i < 10; i++) board.MoveLeft();
        Assert.Equal(-1, piece.Col);
        Assert.Equal(5, events);
    }

    [Fact]
    public void Tick_and_soft_drop_move_one_row_and_soft_drop_scores_one()
    {
        var board = new GameBoard(new Random(3));
        var piece = SetPiece(board, TetrominoType.O, 0, 3);
        Assert.True(board.Tick());
        Assert.Equal(1, piece.Row);
        board.SoftDrop();
        Assert.Equal(2, piece.Row);
        Assert.Equal(1, board.Score);
    }

    [Fact]
    public void Hard_drop_awards_two_points_per_row_and_locks_piece()
    {
        var board = new GameBoard(new Random(4));
        SetPiece(board, TetrominoType.O, 0, 3);
        var landing = board.GetGhostRow();
        board.HardDrop();
        Assert.Equal(landing, board.Grid.Cast<string?>().Select((v, i) => (v, i)).Where(x => x.v is not null).Min(x => x.i) / GameBoard.Width);
        Assert.Equal(2 * landing, board.Score);
        Assert.NotNull(board.Current);
    }

    [Fact]
    public void Ghost_row_stops_above_obstacle()
    {
        var board = new GameBoard(new Random(5));
        var piece = SetPiece(board, TetrominoType.O, 0, 3);
        board.Grid[15, 4] = "obstacle";
        Assert.Equal(13, board.GetGhostRow());
        Assert.Equal(0, piece.Row);
    }

    [Fact]
    public void Hold_can_be_used_once_per_turn_and_resets_after_lock()
    {
        var board = new GameBoard(new Random(6));
        SetPiece(board, TetrominoType.T, 0, 3);
        board.HoldPiece();
        Assert.Equal(TetrominoType.T, board.HoldType);
        var heldCurrent = board.Current!.Type;
        board.HoldPiece();
        Assert.Equal(heldCurrent, board.Current!.Type);
        board.HardDrop();
        board.HoldPiece();
        Assert.NotNull(board.HoldType);
    }

    [Fact]
    public void Hold_swaps_piece_type_and_reset_clears_game_state()
    {
        var board = new GameBoard(new Random(7));
        SetPiece(board, TetrominoType.I, 0, 3);
        board.HoldPiece();
        var afterFirstHold = board.Current!.Type;
        board.HardDrop();
        var typeBeforeSecondHold = board.Current!.Type;
        board.HoldPiece();
        Assert.Equal(TetrominoType.I, board.Current!.Type);
        Assert.Equal(typeBeforeSecondHold, board.HoldType);
        board.IsPaused = true;
        board.Reset();
        Assert.Null(board.HoldType);
        Assert.Equal(0, board.Score);
        Assert.Equal(0, board.Lines);
        Assert.Equal(0, board.Level);
        Assert.False(board.IsPaused);
        Assert.False(board.IsGameOver);
    }

    [Fact]
    public void Pause_blocks_gameplay_and_drop_interval_speeds_up_with_level()
    {
        var board = new GameBoard(new Random(8));
        var piece = SetPiece(board, TetrominoType.T, 0, 3);
        board.IsPaused = true;
        board.MoveLeft();
        board.Rotate();
        board.SoftDrop();
        Assert.Equal((0, 3, 0), (piece.Row, piece.Col, piece.Rotation));
        Assert.False(board.Tick());
        board.IsPaused = false;
        Assert.True(board.GetDropIntervalMs() >= 80);
        Assert.True(board.GetDropIntervalMs() <= 1000);
    }

    [Fact]
    public void Completing_a_line_clears_it_and_updates_score_and_events()
    {
        var board = new GameBoard(new Random(9));
        SetPiece(board, TetrominoType.I, 18, 3);
        for (var col = 0; col < GameBoard.Width; col++)
            if (col < 3 || col > 6) board.Grid[19, col] = "filled";
        var lineEvents = 0;
        board.OnLineClear += () => lineEvents++;
        board.HardDrop();
        Assert.Equal(1, board.Lines);
        Assert.Equal(100, board.Score);
        Assert.Equal(new[] { 19 }, board.LastClearedRows);
        Assert.Equal(1, lineEvents);
        Assert.All(Enumerable.Range(0, GameBoard.Width), col => Assert.Null(board.Grid[19, col]));
    }

    [Fact]
    public void Spawn_collision_ends_game_and_fires_event()
    {
        var board = new GameBoard(new Random(10));
        var current = SetPiece(board, TetrominoType.O, 18, 3);
        var next = board.NextType;
        var spawnCol = (GameBoard.Width - Tetromino.GridSize(next)) / 2;
        foreach (var (row, col) in new ActivePiece(next, 0, spawnCol).GetAbsoluteCells()) board.Grid[row, col] = "block";
        var ended = 0;
        board.OnGameOver += () => ended++;
        board.HardDrop();
        Assert.True(board.IsGameOver);
        Assert.Equal(1, ended);
    }

    private static ActivePiece SetPiece(GameBoard board, TetrominoType type, int row, int col)
    {
        var piece = new ActivePiece(type, row, col);
        typeof(GameBoard).GetProperty(nameof(GameBoard.Current))!.SetValue(board, piece);
        return piece;
    }
}
