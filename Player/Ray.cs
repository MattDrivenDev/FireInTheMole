namespace FireInTheHole.Player;

public struct Ray
{
    public float Sin { get; init; }
    public float Cos { get; init; }
    public float Depth { get; init; }
    public int? Tile { get; init; }
    public float TextureOffset { get; init; }
}