using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo
{
    public Vector2Int Position { get; set; }
    public TileInfo Parent { get; set; }
    public int H { get; set; }
    public int G { get; set; }
    public int I { get; set; }
    public int F { get; set; }

    public void UpdateF() 
        => F = G + H;

    public void UpdateH(Vector2Int target) 
        => H = Mathf.FloorToInt(Mathf.Abs(Position.x - target.x) + Mathf.Abs(Position.y - target.y)) * 10;

    public TileInfo(Vector2Int position, int i = -1, int g = int.MaxValue)
    {
        Position = position;
        I = i;
        G = g;
    }

    public override string ToString()
    {
        string info = $"I: {I}\tPos: {Position}";
        if (Parent != null)
            info += $"\nParent I: {Parent.I}\tParent Position: {Parent.Position}";
        else
            info += "\tNo Parent";
        return info;
    }
}