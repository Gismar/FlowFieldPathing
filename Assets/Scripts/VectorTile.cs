using UnityEngine;

public class VectorTile
{
    public Vector2Int Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Distance { get; set; }

    public VectorTile(Vector2Int position, Vector2 direction = default)
    {
        Position = position;
        Direction = direction;
    }

    public VectorTile(VectorTile tile)
    {
        Position = tile.Position;
        Direction = tile.Direction;
        Distance = tile.Distance;
    }

    public VectorTile Reset()
    {
        Distance = 0;
        Direction = Vector2.zero;
        return this;
    }
}
