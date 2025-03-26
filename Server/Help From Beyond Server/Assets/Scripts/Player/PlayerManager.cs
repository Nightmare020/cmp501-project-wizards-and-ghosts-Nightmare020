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
    public NetworkVariable<PlayerState> currentState = new NetworkVariable<PlayerState>();
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

    private void Start()
    {
        _soundManager = GetComponentInParent<SoundManager>();
        _arcadeManager = FindObjectOfType<ArcadeManager>();
        _camera = Camera.main;
        cameraShake = _camera.GetComponent<CameraShake>();
        cameraFollow = _camera.GetComponent<CameraFollow>();
        _rigidBody2D = GetComponent<Rigidbody2D>();

        // Get the PlayerStartingPoints component from the scene
        //PlayersStartingPoints startingPointsComponent = FindObjectOfType<PlayersStartingPoints>();
        //if (startingPointsComponent != null)
        //{
        //    List<Transform> startingPoints = startingPointsComponent.GetStartingPoints();
        //    // Set the player's starting position
        //    if (startingPoints != null && startingPoints.Count > 0)
        //    {
        //        transform.position = startingPoints[0].position; // Use the first starting point for now
        //    }
        //}

        // Initialize player state as Wizard
        SetCurrentState(PlayerState.Wizard);

        // Enable control over the wizard player
        EnableControl();
    }

    private void EnableControl()
    {
        // Enable input and control for the wizard player
        // Assuming you have an input manager or similar setup
        MyInputManager inputManager = GetComponent<MyInputManager>();
        if (inputManager != null)
        {
            inputManager.SetInputMap(CurrentInputState.Wizard);
        }
    }

    public PlayerManager GetOtherPlayer()
    {
        if (otherPlayer == null)
        {
            foreach (PlayerManager player in FindObjectsOfType<PlayerManager>())
            {
                if (player != this)
                {
                    otherPlayer = player;
                }
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
                cameraFollow.m_Target = transform;
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
                cameraFollow.m_Target = transform;
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