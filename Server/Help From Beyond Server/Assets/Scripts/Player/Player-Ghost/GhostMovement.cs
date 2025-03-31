using UnityEngine;
using Unity.Netcode;

public class GhostMovement : NetworkBehaviour
{
    private GhostValues _ghostValues;
    private float _gamePadAddedSpeed;
    private MyInputManager _inputs;

    private Vector2 _screenBounds;
    private Vector2 _upperBound;
    private Vector2 _lowerBound;
    private float _objectWidth;
    private float _objectHeight;

    private Vector2 _cachedInput;
    private float speed = 0;

    private void Start()
    {
        _ghostValues = GetComponent<GhostValues>();
        _inputs = GetComponentInParent<MyInputManager>();
        //_screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height) * 0.5f);

        //_screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        _objectWidth = _ghostValues.spriteRenderer.bounds.extents.x; //extents = size of width / 2
        _objectHeight = _ghostValues.spriteRenderer.bounds.extents.y; //extents = size of height / 2
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 input = _inputs.GhostMovement();
        _cachedInput = input;
        _gamePadAddedSpeed = input.magnitude;

        UpdateFacingDirection(input);

        SendInputToServerRpc(input);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        //MoveGhost(_cachedInput);

        // Fly
        //if (MoveDirectionVectorNormalized() != Vector2.zero)
        //{
        //    Vector2 moveDirection = MoveDirectionVectorNormalized();
        //    speed = _ghostValues.moveSpeed;
        //    moveDirection *= _gamePadAddedSpeed;
        //    _ghostValues.rigidBody.AddForce(moveDirection * speed -
        //                                     _ghostValues.rigidBody.velocity);
        //}
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input)
    {
        MoveGhost(input);
        UpdateGhostVisualClientRpc(input);
    }

    private void MoveGhost(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        //direction *= _gamePadAddedSpeed;

        float speed = _ghostValues.moveSpeed;
        Vector2 desiredVelocity = direction * speed;
        Vector2 velocityDiff = desiredVelocity - _ghostValues.rigidBody.velocity;

        _ghostValues.rigidBody.AddForce(velocityDiff);
    }

    private void UpdateFacingDirection(Vector2 input)
    {
        if (input.x > 0)
        {
            _ghostValues.spriteRenderer.flipX = false;
            _ghostValues.aimDirection.x = 1;
            _ghostValues.facing = 1;
        }
        else if (input.x < 0)
        {
            _ghostValues.spriteRenderer.flipX = true;
            _ghostValues.aimDirection.x = -1;
            _ghostValues.facing = -1;
        }

        _ghostValues.aimDirection.y = input.y == 0 ? 0 : (input.y > 0 ? 1 : -1);
    }

    [ClientRpc]
    private void UpdateGhostVisualClientRpc(Vector2 input)
    {
        if (IsOwner || !Application.isPlaying) return;

        if (input.x > 0)
        {
            _ghostValues.spriteRenderer.flipX = false;
            _ghostValues.aimDirection.x = 1;
            _ghostValues.facing = 1;
        }
        else if (input.x < 0)
        {
            _ghostValues.spriteRenderer.flipX = true;
            _ghostValues.aimDirection.x = -1;
            _ghostValues.facing = -1;
        }

        _ghostValues.aimDirection.y = input.y == 0 ? 0 : (input.y > 0 ? 1 : -1);
    }

    //private Vector2 MoveDirectionVectorNormalized()
    //{
    //    Vector2 direction = _inputs.GhostMovement();
    //    //print(direction);
    //    _gamePadAddedSpeed = direction.magnitude;

    //    if (direction.x == 0) _ghostValues.aimDirection.x = 0;
    //    else if (direction.x > 0)
    //    {
    //        _ghostValues.spriteRenderer.flipX = false;
    //        _ghostValues.aimDirection.x = 1;
    //        _ghostValues.facing = 1;
    //    }
    //    else if (direction.x < 0)
    //    {
    //        _ghostValues.spriteRenderer.flipX = true;
    //        _ghostValues.aimDirection.x = -1;
    //        _ghostValues.facing = -1;
    //    }

    //    if (direction.y == 0) _ghostValues.aimDirection.y = 0;
    //    else _ghostValues.aimDirection.y = direction.y > 0 ? 1 : -1;

    //    return new Vector2(direction.x, direction.y);
    //}

    void LateUpdate()
    {
        if (!IsOwner) return;

        _screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 viewPos = transform.root.position;

        _upperBound = new Vector2(_screenBounds.x - _objectWidth, _screenBounds.y - _objectHeight);
        _lowerBound = new Vector2(_screenBounds.x - ((_screenBounds.x - Camera.main.transform.position.x) * 2) + _objectWidth, _screenBounds.y - ((_screenBounds.y - Camera.main.transform.position.y) * 2) + _objectHeight);

        viewPos.x = Mathf.Clamp(viewPos.x, _screenBounds.x - ((_screenBounds.x - Camera.main.transform.position.x) * 2) + _objectWidth, _screenBounds.x - _objectWidth);
        viewPos.y = Mathf.Clamp(viewPos.y, _screenBounds.y - ((_screenBounds.y - Camera.main.transform.position.y) * 2) + _objectHeight, _screenBounds.y - _objectHeight);
        transform.root.position = viewPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(_upperBound.x, _upperBound.y, 0), 1);
        Gizmos.DrawSphere(new Vector3(_lowerBound.x, _lowerBound.y, 0), 1);
    }

}
