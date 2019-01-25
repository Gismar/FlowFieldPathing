using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private VectorFieldPathing _vectorFieldPathing;
    [SerializeField] private int _speed;

    private Rigidbody2D _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // To prevent enemy drifting
        _rigidbody.velocity = Vector2.zero;
        var currentField = _vectorFieldPathing.CurrentField;

        // Checks to seee if the enemy is within range to reduce computation.
        if (Vector2.Distance(currentField.Key, transform.position) > 20)
            return;

        // Finds the tile with the same position as the enemy.
        VectorTile tile = currentField.Value.Find(v => v.Position == Vector2Int.FloorToInt(transform.position));
        if (tile == null)
            return;
        
        // Moves itself based on the tile's direction.
        _rigidbody.MovePosition((Vector2)transform.position + tile.Direction * (_speed / 32f) * Time.deltaTime);
        var direcetion = (currentField.Key - (Vector2)transform.position).normalized;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direcetion.y, direcetion.x) * Mathf.Rad2Deg + 90);
    }
}
