using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class VectorFieldPathing : MonoBehaviour
{
    [SerializeField] private Tilemap _map;
    [SerializeField] private Transform _player;

    private Vector2Int _oldPosition;
    private List<VectorTile> _vectorTilesPool;
    private Dictionary<Vector2Int, List<VectorTile>> _savedFields;
    private ConcurrentDictionary<Vector2Int, List<VectorTile>> _savedFieldsConcurrent;

    public KeyValuePair<Vector2Int, List<VectorTile>> CurrentField { get; private set; }

    private void Awake()
    {
        InitizalizeVectorTilePool();
        
        // Initilize Dictionaries
        _savedFieldsConcurrent = new ConcurrentDictionary<Vector2Int, List<VectorTile>>(Environment.ProcessorCount * 2, _vectorTilesPool.Count + 1);
        _savedFields = new Dictionary<Vector2Int, List<VectorTile>>();

        // Create the player's current Vector Field
        var pos = Vector2Int.FloorToInt(_player.position);
        CurrentField = new KeyValuePair<Vector2Int, List<VectorTile>>(pos, _savedFieldsConcurrent.GetOrAdd(pos, VectorFieldAlgorithm(pos)));

        // Creates a thread to create threads to create Vector Fields
        // because calling `thread.Join()` will freeze Unity's thread
        var threadCreater = new Thread(new ThreadStart(CreatePathThreads));
        threadCreater.Start();
    }

    private void Update()
    {
        var pos = Vector2Int.FloorToInt(_player.position);
        
        // Checks if the player has moved
        if (pos != _oldPosition)
        {
            _oldPosition = pos;
            if (_savedFields.ContainsKey(pos))
            {
                CurrentField = new KeyValuePair<Vector2Int, List<VectorTile>>(pos, _savedFields[pos]);
            }
            else // Only gets called 
            {
                // Adds the current player's position to the Concurrent dictionary.
                CurrentField = new KeyValuePair<Vector2Int, List<VectorTile>>(pos, _savedFieldsConcurrent.GetOrAdd(pos, VectorFieldAlgorithm(pos)));
            }
        }
    }

    private void InitizalizeVectorTilePool()
    {
        // Creates VectorTile Pool for reusability and reduced memory usage.
        _vectorTilesPool = new List<VectorTile>();
        _map.CompressBounds(); // Important step when every working with Unity Tilemaps.

        RectInt.PositionEnumerator positions = new RectInt
            (Vector2Int.FloorToInt(_map.localBounds.min),                       // Bottom Right Corner.
            Vector2Int.FloorToInt(_map.localBounds.size)).allPositionsWithin;   // Width x Height.
        
        while (positions.MoveNext())
            if (_map.HasTile(Vector3Int.FloorToInt((Vector2)positions.Current)))
                _vectorTilesPool.Add(new VectorTile(positions.Current));
    }

    private void CreatePathThreads()
    {
        // Creates threads of half from half of the CPU.
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int threadAmounts = Environment.ProcessorCount / 2;
        var amount = Mathf.FloorToInt(_vectorTilesPool.Count / (float)threadAmounts);
        var threads = new Thread[threadAmounts];

        // Assigns the methods needed for each thread.
        for (int i = 0; i < threadAmounts; i++)
        {
            var tiles = _vectorTilesPool.GetRange(i * amount, amount - 1);
            threads[i] = new Thread(() => CreateVectorFields(tiles));
            threads[i].Start();
        }

        // And joins the threads created.
        for (int i = 0; i < threadAmounts; i++)
        {
            threads[i].Join();
        }

        // Finally, locks the dictionary and assigns all the info from 
        // the concurrent dictionary to it. 
        lock (_savedFields) 
        {
            _savedFields = _savedFieldsConcurrent.ToDictionary(f => f.Key, f => f.Value);
        }
        Debug.Log($"Finished in {watch.ElapsedMilliseconds}");
    }

    private void CreateVectorFields(List<VectorTile> vectorField)
    {
        // Does the Vector Field Pathing calculations for every tile
        // and adds it to the concurrent dictionary
        foreach (var tile in vectorField)
            _savedFieldsConcurrent.TryAdd(tile.Position, VectorFieldAlgorithm(tile.Position));
    }

    private List<VectorTile> VectorFieldAlgorithm(Vector2Int startPosition)
    {
        VectorTile current = new VectorTile(Vector2Int.zero);

        // Locks and selects all the tiles within a 20
        // unit distance in the VectorTile pool
        // while reseting the values for all tiles in the pool.
        var tiles = new List<VectorTile>();
        lock (_vectorTilesPool)
        {
            tiles = _vectorTilesPool.Where(t =>
            {
                t.Reset();
                if (t.Position == startPosition)
                    current = t;
                return Vector2.Distance(startPosition, t.Position) < 10f;
            }).Select(t => new VectorTile(t)).ToList();
        }

        var open = new HashSet<VectorTile>() { current };
        var close = new HashSet<VectorTile>();

        // Modified version of Djistra's Algorithm.
        while (open.Count != 0)
        {
            current = open.Aggregate((a, b) => a.Distance < b.Distance ? a : b);
            open.Remove(current);
            close.Add(current);

            // Loops through every neighbor of the current tile.
            foreach (var neighbor in GetNeighbors(current, tiles))
            {
                if (close.Contains(neighbor))
                    continue;

                // Sets neighbor's distance and direction and adds it to the open list.
                if (!open.Contains(neighbor))
                {
                    neighbor.Distance = current.Distance + Vector2.Distance(neighbor.Position, current.Position);
                    neighbor.Direction = current.Position - neighbor.Position;
                    open.Add(neighbor);
                }
            }
        }

        return tiles;
    }

    private List<VectorTile> GetNeighbors(VectorTile tile, List<VectorTile> tiles)
    {
        List<VectorTile> neighbors = new List<VectorTile>();

        // Creates the box to check all the neighbors
        RectInt.PositionEnumerator positions = new RectInt(tile.Position.x - 1, tile.Position.y - 1, 3, 3).allPositionsWithin;

        while (positions.MoveNext())
        {
            // Skip to next tile if it is the original tile.
            if (positions.Current == tile.Position)
                continue;

            // Checks the VectorTile list to see if there is a tile with the current position.
            VectorTile neighborTile = tiles.Find(t => t.Position == positions.Current);

            // If tile was found, add it to the neighbors list
            if (neighborTile != default(VectorTile))
                neighbors.Add(neighborTile);
        }

        return neighbors;
    }
}
