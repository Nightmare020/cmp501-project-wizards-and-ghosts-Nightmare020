using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resurrect : MonoBehaviour
{
    [SerializeField] private PlayerManager _playerManager;

    // Start is called before the first frame update
    [SerializeField] private LayerMask ground;
    [SerializeField] private CanvasGroup sliderCanvas;
    [SerializeField] private Slider _slider;
    [SerializeField] private float min = -2, max = -1;
    [SerializeField] private Transform referencePoint;
    private float value = 0;
    [SerializeField] private float factor = 0.1f;
    private List<Transform> _spawnPoints;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<Sprite> treeSprites;
    private float spritePercent = 0;

    void Awake()
    {
        spritePercent = 1f / treeSprites.Count;
        _spawnPoints = new List<Transform>();
        foreach (var spawn in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            _spawnPoints.Add(spawn.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(referencePoint.position, Mathf.Abs(min));
    }

    private void OnEnable()
    {
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


    // Update is called once per frame
    void Update()
    {
        if (_playerManager.GetOtherPlayer())
        {
            float distance = -Vector2.Distance(_playerManager.otherPlayer.transform.position, referencePoint.position);
            if (distance < min)
            {
                sliderCanvas.alpha = 0;
            }
            else
            {
                sliderCanvas.alpha = Mathf.Clamp01(MyUtils.Normalice(distance, min, max));
                value += Time.fixedDeltaTime * factor;
                _slider.value = Mathf.Clamp01(value);
                int idx = Mathf.Min(Mathf.FloorToInt(value / spritePercent), treeSprites.Count - 1);
                _spriteRenderer.sprite = treeSprites[idx];
                if (value >= 1)
                {
                    _spriteRenderer.sprite = treeSprites[0];
                    value = 0;
                    _playerManager.Resurrect();
                }
            }
        }
    }

    private Vector2 GetClosestCheckpoint()
    {
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