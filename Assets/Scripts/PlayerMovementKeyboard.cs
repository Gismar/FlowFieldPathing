using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayerMovementKeyboard : MonoBehaviour
{
    [SerializeField] private int _speed;

    private Vector3 _movementDirection;
    private Camera _mainCamera;
    private Rigidbody2D _rigidbody;
    private Vector2Int _oldPosition;

    Dictionary<KeyCode, Vector3> _playerInputs = new Dictionary<KeyCode, Vector3>
    {
        [KeyCode.D] = Vector3.right,
        [KeyCode.A] = Vector3.left,
        [KeyCode.W] = Vector3.up,
        [KeyCode.S] = Vector3.down
    };

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Gets all using inputs that are within the defined _playerInput dictionary and adds their vectors
        foreach (var input in _playerInputs.Where(k => Input.GetKey(k.Key)))
            _movementDirection += input.Value;
    }

    void Update()
    {
        // Looks towards the mouse (Unity's LookAt method has never worked to my full liking).
        var direcetion = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direcetion.y, direcetion.x) * Mathf.Rad2Deg + 90);
        
        // Using _movementDirection from inputs to move.
        _rigidbody.MovePosition(transform.position + (_movementDirection.normalized * (_speed / 32f) * Time.deltaTime));

        _rigidbody.velocity = Vector2.zero; // Prevent player from drifting.
        _movementDirection = Vector3.zero; // Resets the movement vectors.
        _mainCamera.transform.position = transform.position + (Vector3.back * 10); // Moves the camera to player.
    }
}
