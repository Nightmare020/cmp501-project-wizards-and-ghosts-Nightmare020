
using UnityEngine;

[DefaultExecutionOrder(1)]
public class ArcadePoint : MonoBehaviour
{
    public enum COIN_TYPE
    {
        Wizard,
        Ghost
    }

    // Start is called before the first frame update
    private ArcadeManager _arcadeManager;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider2D;
    public bool collected = false;
    [SerializeField] private COIN_TYPE _type;
    private ParticleSystem _particleSystem;
    private AudioSource _audioSource;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _arcadeManager = FindObjectOfType<ArcadeManager>();
    }

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected)
        {
            if (_type is COIN_TYPE.Wizard)
            {
                if (other.gameObject.CompareTag("Wizard"))
                {
                    _arcadeManager.AddWizardPoints(1,this);
                    DisablePoint();
                }
            }
            else if (_type is COIN_TYPE.Ghost)
            {
                if (other.gameObject.CompareTag("Ghost"))
                {
                    _arcadeManager.AddGhostPoints(1,this);
                    DisablePoint();
                }
            }
        }
        
    }


    public void EnablePoint(Vector2 pos)
    {
        transform.position = pos;
        _spriteRenderer.color = Color.white;
        _collider2D.enabled = true;
        collected = false;
        _particleSystem.Play();
    }

    public void DisablePoint()
    {
        _audioSource.Play();
        _spriteRenderer.color = Color.clear;
        _collider2D.enabled = false;
        collected = true;
        _particleSystem.Stop();
    }

    public bool GetCollected()
    {
        return collected;
    }
}