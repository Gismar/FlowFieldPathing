# Flow Field AI Pathing
>Using multi-threading to generate a Flow Field that allows infinite amount of enemies to chase a player.

## What is this?
This algorithm creates a 10 x 10 (specifically made like this for a game) Vector Flow Field for every tile possible. 
In addition it splits the work into other threads while main thread creates fields for the player's current position. 
This allows players to interact with the game and enemies without having to wait for the level to be finished.

## What is a Flow Field
It is an algorithm using a modified Dijkstra's Algorithm to create a grid with each point having a direction towards its 
parent until it reaches the target. Objects on this field can use the point's direction to move towards the target.
Additionally, it's a simple algorithm with low computation but still very effective. 
Unlike A* which finds the absolute shortest path, Flow Fields find all possible paths to a location.

## Use Case
When you need multiple objects to chase after a target frequently. As it is computes every possible path in a simple manner.

## How I Made It Special
Instead of constantly computing a field, I instead computed a 20 x 20 tile field for every possible tile in a game map,
and stored them into a dictionary. The dictionary is then accessed using the player's position. 
`Dictionary<Vector2Int, List<VectorTile>` Where the `Vector2Int` is the player's position, and `List<VectorTile>` is the flow field.
Furthermore, I use multiple threads to compute the flow field for each chunk of the map.

## Unity-ized
Unity's tilemap currently is currently limited with how you can interact with it, but is extremely use full for quick map generation.
What I did was create a custom class `VectorTile` to store custom data for each tile in a tilemap and work with that instead.
