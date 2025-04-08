using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Resurrect : MonoBehaviour
{
    [SerializeField] private PlayerManager _playerManager; // Reference to the PlayerManager component

    [SerializeField] private LayerMask ground; // Layer mask for ground detection
    [SerializeField] private CanvasGroup sliderCanvas; // Canvas group for the slider UI
    [SerializeField] private Slider _slider; // Slider UI element
    [SerializeField] private float min = -2, max = -1; // Minimum and maximum distances for the slider
    [SerializeField] private Transform referencePoint; // Reference point for distance calculation
    private float value = 0; // Current value of the slider
    [SerializeField] private float factor = 0.1f; // Factor for increasing the slider value
    private List<Transform> _spawnPoints; // List of spawn points
    [SerializeField] private SpriteRenderer _spriteRenderer; // Sprite renderer for the tree sprites
    [SerializeField] private List<Sprite> treeSprites; // List of tree sprites

    private float _spritePercent = 0; // Percentage for changing tree sprites
    private PlayerManager _cachedOtherPlayer; // Cached reference to the other player

    void Awake()
    {
        // Initialize the sprite percentage and spawn points
        _spritePercent = 1f / treeSprites.Count;
        _spawnPoints = new List<Transform>();
        foreach (var spawn in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            _spawnPoints.Add(spawn.transform);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a wire sphere to visualize the reference point
        Gizmos.DrawWireSphere(referencePoint.position, Mathf.Abs(min));
    }

    private void OnEnable()
    {
        // Set the initial position of the resurrect object
        RaycastHit2D spawnPos =
            Physics2D.Raycast(transform.parent.position + new Vector3(0, 0.1f), Vector2.down, Single.NegativeInfinity,
                ground);
        if (spawnPos)
        {
            transform.root.position = spawnPos.point + new Vector2(0, 0.1f);
        }
        else
        {
            transform.root.position = GetClosestCheckpoint();
        }
    }

    void Update()
    {
        if (_playerManager == null) return;

        // Try to cache the other player once found
        if (_cachedOtherPlayer == null)
        {
            _cachedOtherPlayer = _playerManager.GetOtherPlayer();
            if (_cachedOtherPlayer != null)
            {
                return;
            }
        }

        // Calculate the distance between the other player and the reference point
        float distance = -Vector2.Distance(_playerManager.otherPlayer.transform.position, referencePoint.position);
        if (distance < min)
        {
            sliderCanvas.alpha = 0;
        }
        else
        {
            // Update the slider value and tree sprite based on the distance
            sliderCanvas.alpha = Mathf.Clamp01(MyUtils.Normalice(distance, min, max));
            value += Time.fixedDeltaTime * factor;
            _slider.value = Mathf.Clamp01(value);

            int idx = Mathf.Min(Mathf.FloorToInt(value / _spritePercent), treeSprites.Count - 1);
            _spriteRenderer.sprite = treeSprites[idx];

            if (value >= 1)
            {
                _spriteRenderer.sprite = treeSprites[0];
                value = 0;

                // Trigger resurrection only on the server
                if (NetworkManager.Singleton.IsServer)
                {
                    // Resurrect the other player
                    _playerManager.Resurrect();
                }
            }
        }
    }

    private Vector2 GetClosestCheckpoint()
    {
        // Find the closest checkpoint to the current position
        float minDist = Single.PositiveInfinity;
        Vector2 closest = transform.position;
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            float dist = Vector2.Distance(transform.position, _spawnPoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = _spawnPoints[i].position;
            }
        }

        return closest;
    }
}