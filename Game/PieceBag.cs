Tetris.Game;

/// <summary>
/// Implements the standard "7-bag" randomizer: each of the 7 tetromino
/// types appears exactly once per shuffled bag before the bag refills.
/// This avoids long droughts of any one piece, matching modern Tetris games.
/// </summary>
public class PieceBag
{
    private readonly Random _random;
    private readonly Queue<TetrominoType> _queue = new();

    public PieceBag(Random? random = null)
    {
        _random = random ?? new Random();
        RefillBag();
        RefillBag();
    }

    public TetrominoType Next()
    {
        if (_queue.Count <= 7)
        {
            RefillBag();
        }
        return _queue.Dequeue();
    }

    public TetrominoType PeekNext() => _queue.Peek();

    private void RefillBag()
    {
        var bag = Tetromino.All.ToList();
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
        foreach (var piece in bag)
        {
            _queue.Enqueue(piece);
        }
    }
}