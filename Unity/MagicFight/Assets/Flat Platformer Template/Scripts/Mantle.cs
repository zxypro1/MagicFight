using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mantle : MonoBehaviour {
    public Player _player;
    public float _offset;
    private bool _isMirror;
    private Rigidbody2D _playerRig;
    private Vector2 _defCoords;

    void Start()
    {
        _defCoords = transform.localPosition;
        _playerRig = _player.gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 _velocity = _playerRig.velocity;
        _velocity.Normalize();
        _isMirror = _player.mirror;

        if (_isMirror)
            transform.localPosition = new Vector2(_defCoords.x + _offset * _velocity.x, transform.localPosition.y);

        if (!_isMirror)
            transform.localPosition = new Vector2(_defCoords.x - _offset * _velocity.x, transform.localPosition.y);
    }
}
