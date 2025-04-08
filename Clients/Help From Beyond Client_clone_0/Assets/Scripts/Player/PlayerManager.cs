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

    [SerializeField] private GameObject wizard, ghost, dead; // References to the wizard, ghost, and dead game objects
    [NonSerialized] public Camera _camera; // Reference to the main camera
    [NonSerialized] public CameraShake cameraShake; // Reference to the CameraShake component
    [NonSerialized] public CameraFollow cameraFollow; // Reference to the CameraFollow component
    [NonSerialized] public Rigidbody2D _rigidBody2D; // Reference to the Rigidbody2D component

    [SerializeField] private WizardAnimationManager _wizardAnimationManager; // Reference to the WizardAnimationManager component
    public PlayerManager otherPlayer; // Reference to the other player
    public bool isDead = false; // Flag to check if the player is dead
    [NonSerialized] public SoundManager _soundManager; // Reference to the SoundManager component
    [SerializeField] private SpriteRenderer _spriteRendererWizard; // Reference to the SpriteRenderer component for the wizard
    [SerializeField] private List<Transform> startingPoints; // List of starting points for the player

    private void Awake()
    {
        // Initialize references to components
        _soundManager = GetComponentInParent<SoundManager>();
        _camera = Camera.main;
        cameraShake = _camera.GetComponent<CameraShake>();
        cameraFollow = _camera.GetComponent<CameraFollow>();
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }

    public void SetInitialPlayerState(PlayerState state)
    {
        if (!IsServer) return;

        currentState.Value = state;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentState.OnValueChanged += OnPlayerStateChanged;

        // Force state synchronization
        ForceStateSync();

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
        ApplyVisualState(newState);
    }

    private void EnableControl()
    {
        // Enable input and control for the wizard player
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

    public void ForceStateSync()
    {
        // Force the update (visuals, objects)
        SetCurrentState(currentState.Value);
    }

    private void Update()
    {
        // Check if the player has fallen off the map
        if (Time.frameCount % 10 == 0 && transform.position.y < -45)
        {
            Die();
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        Camera wizardCam = _camera;
        if (!wizardCam) return;

        // Update camera bounds
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
        // Prevent feedback loop or redundant assignment
        if (IsServer && currentState.Value != playerState)
        {
            currentState.Value = playerState;
            return; // clients will sync on value change
        }

        // Update local visuals regardless
        ApplyVisualState(playerState);
    }

    private void ApplyVisualState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Wizard:
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
                cameraFollow.m_Target = transform;
                break;

            case PlayerState.Ghost:
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

                PlayerManager wizardPlayer = GetOtherPlayer();
                if (wizardPlayer != null && wizardPlayer.currentState.Value == PlayerState.Wizard)
                {
                    cameraFollow.m_Target = wizardPlayer.transform;
                }

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
        if (!IsServer) return;

        Vector2 spawnPos = GetSafeSpawnNearWizard();

        transform.position = spawnPos;

        SetCurrentState(PlayerState.Ghost);
        StartCoroutine(InvulnerabilityCoroutine(3));
    }

    public void Die()
    {
        if (!IsServer) return;

        if (GetOtherPlayer().GetComponent<PlayerManager>().isDead)
        {
            // Game over
            ArcadeManager.Instance?.TriggerGameOver();
        }

        // Ensure the NetworkVariable is changed
        currentState.Value = PlayerState.Dead;
    }

    public void Die(Vector2 pos)
    {
        if (GetOtherPlayer().GetComponent<PlayerManager>().isDead)
        {
            // Game over
            Debug.Log("Both players dead — triggering game over");
            ArcadeManager.Instance?.TriggerGameOver();
        }

        transform.position = pos;

        // Ensure the NetworkVariable is changed
        currentState.Value = PlayerState.Dead;
    }

    private Vector2 GetSafeSpawnNearWizard()
    {
        Vector2 wizardPos = GetOtherPlayer()?.transform.position ?? transform.position;
        Vector2 candidate = wizardPos + UnityEngine.Random.insideUnitCircle.normalized * 3f;

        // Avoid spawning near ghosts
        foreach (var ghostEnemy in GameObject.FindGameObjectsWithTag("Ghost Enemy"))
        {
            if (Vector2.Distance(ghostEnemy.transform.position, candidate) < 2f)
            {
                candidate += UnityEngine.Random.insideUnitCircle.normalized * 2f;
            }
        }

        return candidate;
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

            // Restore alpha to 1
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