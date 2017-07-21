public struct Point : System.IEquatable<Point>{

    private readonly int x;
    private readonly int y;

    public int X { get { return x; } }
    public int Y { get { return y; } }


    public Point(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public override string ToString() {
        return X + "." + Y;
    }

    public static bool operator ==(Point p1, Point p2){
        return p1.X == p2.X && p1.Y == p2.Y;
    }

    public static bool operator !=(Point p1, Point p2){
        return p1.X != p2.X || p1.Y != p2.Y;
    }

    public override int GetHashCode()
    {
        if (this == null)
            return 0;

        return 17 * X + 23 * Y;
    }

    public override bool Equals(object obj) {
        return obj is Point && Equals((Point)obj);
    }

    public bool Equals(Point other) {
        return X == other.X && Y == other.Y;
    }

    public static implicit operator Point(UnityEngine.Vector2 v2)
    {
        return new Point(UnityEngine.Mathf.RoundToInt(v2.x), UnityEngine.Mathf.RoundToInt(v2.y));
    }
}
