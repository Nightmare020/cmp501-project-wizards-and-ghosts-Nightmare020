using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utils;

public enum PlayerState
{
    Wizard,
    Ghost,
    Dead
}

public class PlayerManager : NetworkBehaviour
{
    public NetworkVariable<PlayerState> currentState = new NetworkVariable<PlayerState>(
        PlayerState.Wizard,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<Vector4> cameraBounds = new NetworkVariable<Vector4>(
        new Vector4(0, 0, 0, 0),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    [SerializeField] private GameObject wizard, ghost, dead;
    [NonSerialized] public Camera _camera;
    [NonSerialized] public CameraShake cameraShake;
    [NonSerialized] public CameraFollow cameraFollow;
    [NonSerialized] public Rigidbody2D _rigidBody2D;

    [SerializeField] private WizardAnimationManager _wizardAnimationManager;
    public PlayerManager otherPlayer;
    public bool isDead = false;
    private ArcadeManager _arcadeManager;
    [NonSerialized] public SoundManager _soundManager;
    [SerializeField] private SpriteRenderer _spriteRendererWizard;
    [SerializeField] private List<Transform> startingPoints;

    private void Awake()
    {
        _soundManager = GetComponentInParent<SoundManager>();
        _arcadeManager = FindObjectOfType<ArcadeManager>();
        _camera = Camera.main;
        cameraShake = _camera.GetComponent<CameraShake>();
        cameraFollow = _camera.GetComponent<CameraFollow>();
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }

    public void SetInitialPlayerState(PlayerState state)
    {
        currentState.Value = state;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentState.OnValueChanged += OnPlayerStateChanged;

        // If value already available, apply it
        OnPlayerStateChanged(PlayerState.Wizard, currentState.Value);

        // Enable control over the wizard player
        if (IsOwner)
        {
            EnableControl();
        }
    }

    public override void OnDestroy()
    {
        currentState.OnValueChanged -= OnPlayerStateChanged;
    }

    private void OnPlayerStateChanged(PlayerState oldState, PlayerState newState)
    {
        SetCurrentState(newState);
    }

    private void EnableControl()
    {
        // Enable input and control for the wizard player
        // Assuming you have an input manager or similar setup
        MyInputManager inputManager = GetComponent<MyInputManager>();
        if (inputManager != null)
        {
            switch (currentState.Value)
            {
                case PlayerState.Wizard:
                    inputManager.SetInputMap(CurrentInputState.Wizard);
                    break;
                case PlayerState.Ghost:
                    inputManager.SetInputMap(CurrentInputState.Ghost);
                    break;
            }
        }
    }

    public PlayerManager GetOtherPlayer()
    {
        if (otherPlayer != null) return otherPlayer;


        foreach (PlayerManager player in FindObjectsOfType<PlayerManager>())
        {
            // Make sure it's not self, and it's spawned
            if (player != this && player.IsSpawned)
            {
                otherPlayer = player;
                break;
            }
        }

        return otherPlayer;
    }

    private void Update()
    {
        if (Time.frameCount % 10 == 0 && transform.position.y < -45)
        {
            Die();
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner || currentState.Value != PlayerState.Wizard) return;

        Camera wizardCam = _camera;
        if (!wizardCam) return;

        Vector3 bottomLeft = wizardCam.ViewportToWorldPoint(new Vector3(0, 0));
        Vector3 topRight = wizardCam.ViewportToWorldPoint(new Vector3(1, 1));

        Vector4 newBounds = new Vector4(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x,
            topRight.y
        );

        if (Vector4.Distance(cameraBounds.Value, newBounds) > 0.01f)
        {
            cameraBounds.Value = newBounds;
        }
    }

    public void SetCurrentState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Wizard:
                currentState.Value = playerState;
                Debug.Log("Setting tag to ActiveWizard");
                tag = "ActiveWizard";
                Debug.Log("Tag Wizard set successfully");
                _rigidBody2D.simulated = true;
                _rigidBody2D.gravityScale = 1;
                _rigidBody2D.drag = 0.1f;
                isDead = false;
                wizard.SetActive(true);
                ghost.SetActive(false);
                dead.SetActive(false);

                if (IsOwner)
                {
                    cameraFollow.m_Target = transform;
                }

                break;
            case PlayerState.Ghost:
                currentState.Value = playerState;
                Debug.Log("Setting tag to ActiveGhost");
                tag = "ActiveGhost";
                Debug.Log("Tag Ghost set successfully");
                _rigidBody2D.gravityScale = 0;
                _rigidBody2D.drag = 1f;
                _rigidBody2D.simulated = true;
                isDead = false;
                wizard.SetActive(false);
                ghost.SetActive(true);
                dead.SetActive(false);
                //cameraFollow.m_Target = null;
                break;
            case PlayerState.Dead:
                isDead = true;
                _rigidBody2D.simulated = false;
                wizard.SetActive(false);
                ghost.SetActive(false);
                dead.SetActive(true);
                break;
        }
    }

    public void Resurrect()
    {
        SetCurrentState(currentState.Value);
        StartCoroutine(InvulnerabilityCoroutine(3));
    }

    public void Die()
    {
        if (GetOtherPlayer().GetComponent<PlayerManager>().isDead)
        {
            //game over
            if (_arcadeManager)
            {
                _arcadeManager.GameOver();
            }
            else
            {
                Debug.Log("Game Over");
            }
        }
        else
        {
            SetCurrentState(PlayerState.Dead);
        }
    }

    public void Die(Vector2 pos)
    {
        if (GetOtherPlayer().GetComponent<PlayerManager>().isDead)
        {
            //game over
            if (_arcadeManager)
            {
                _arcadeManager.GameOver();
            }
            else
            {
                Debug.Log("Game Over");
            }
        }
        else
        {
            transform.position = pos;
            SetCurrentState(PlayerState.Dead);
        }
    }

    IEnumerator InvulnerabilityCoroutine(float seconds)
    {
        Physics2D.IgnoreLayerCollision(7, 8);

        if (currentState.Value is PlayerState.Wizard)
        {
            Physics2D.IgnoreLayerCollision(7, 9);

            _spriteRendererWizard.color = new Color(1, 1, 1, 0.2f);

            yield return new WaitForSeconds(seconds);
            Physics2D.IgnoreLayerCollision(7, 9, false);
            //alfa =1  
            _spriteRendererWizard.color = Color.white;
        }
        else if (currentState.Value is PlayerState.Ghost)
        {
            Physics2D.IgnoreLayerCollision(7, 10);
            yield return new WaitForSeconds(seconds);
            Physics2D.IgnoreLayerCollision(7, 10, false);
        }

        Physics2D.IgnoreLayerCollision(7, 8, false);
    }
}