
using Priority_Queue;

public class PNode : FastPriorityQueueNode
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public static PNode Create(int x, int y)
    {
        return new PNode(x, y);
    }

    private PNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        var other = (PNode)obj;
        return this.X == other.X && this.Y == other.Y;
    }

    public override int GetHashCode()
    {
        return X + Y * 7;
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ")";
    }
}