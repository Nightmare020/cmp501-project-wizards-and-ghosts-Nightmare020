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
        float inputStrength = input.magnitude;
        _cachedInput = input;
        _gamePadAddedSpeed = inputStrength;

        UpdateFacingDirection(input);

        SendInputToServerRpc(input, inputStrength);
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, float inputStrength)
    {
        MoveGhost(input, inputStrength);
        UpdateGhostVisualClientRpc(input);
    }

    private void MoveGhost(Vector2 direction, float strength)
    {
        if (direction == Vector2.zero) return;

        float speed = _ghostValues.moveSpeed;
        Vector2 desiredVelocity = direction.normalized * strength * speed;
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

    void LateUpdate()
    {
        //if (!IsOwner) return;

        var wizard = _ghostValues._playerManager.GetOtherPlayer();
        if (wizard == null || wizard.cameraFollow == null) return;

        Vector4 bounds = wizard.cameraBounds.Value;
        if (bounds == Vector4.zero) return;

        Vector3 viewPos = transform.root.position;
        float width = _objectWidth;
        float heigth = _objectHeight;

        viewPos.x = Mathf.Clamp(viewPos.x, bounds.x + width, bounds.z - width);
        viewPos.y = Mathf.Clamp(viewPos.y, bounds.y + heigth, bounds.w - heigth);

        // Get world bounds from wizard's camera
        //_screenBounds = wizardCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, wizardCam.transform.position.z));
        //_screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        //Vector3 viewPos = transform.root.position;

        //_upperBound = new Vector2(_screenBounds.x - _objectWidth, _screenBounds.y - _objectHeight);
        //_lowerBound = new Vector2(_screenBounds.x - ((_screenBounds.x - Camera.main.transform.position.x) * 2) + _objectWidth, _screenBounds.y - ((_screenBounds.y - Camera.main.transform.position.y) * 2) + _objectHeight);

        //viewPos.x = Mathf.Clamp(viewPos.x, _screenBounds.x - ((_screenBounds.x - Camera.main.transform.position.x) * 2) + _objectWidth, _screenBounds.x - _objectWidth);
        //viewPos.y = Mathf.Clamp(viewPos.y, _screenBounds.y - ((_screenBounds.y - Camera.main.transform.position.y) * 2) + _objectHeight, _screenBounds.y - _objectHeight);
        transform.root.position = viewPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(_upperBound.x, _upperBound.y, 0), 1);
        Gizmos.DrawSphere(new Vector3(_lowerBound.x, _lowerBound.y, 0), 1);
    }
}
