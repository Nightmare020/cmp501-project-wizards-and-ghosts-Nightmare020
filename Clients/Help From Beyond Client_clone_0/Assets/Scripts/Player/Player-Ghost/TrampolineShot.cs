
using UnityEngine;

public class TrampolineShot : MonoBehaviour
{
    private GhostValues _ghostValues;
    private MyInputManager _inputs;

    public Collider2D _mainCollider;
    public GameObject _trampolinePrefab;


    public float distance;
    [SerializeField] private Transform shooterSpriteTransform;
    private SpriteRenderer shooterSprite;
    public bool ShootingEnabled = true;
    private bool displayed = false;
    private CameraShake _cameraShake;

    private void Start()
    {
        _cameraShake = FindObjectOfType<CameraShake>();
        _ghostValues = GetComponent<GhostValues>();
        _inputs = GetComponentInParent<MyInputManager>();
        shooterSprite = shooterSpriteTransform.GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 aim = _inputs.GhostAim();

        if (aim != Vector2.zero)
        {
            SetSooterPosition(aim);
        }
        else
        {
            aim.x = _ghostValues.facing;
            if (displayed)
                HideShooter();
        }

        if (_inputs.GhostSetTrampolinePerformedThisFrame())
        {
            // Vector2 aim = _ghostValues.aimDirection;

            //Vector2 force = aim * _ghostValues.trampolineSpeed;

            // if (aim.magnitude < 1) aim.x = _ghostValues.facing;
            // if (aim.magnitude > 1) aim.y = 0;

            GameObject trampoline = Instantiate(_trampolinePrefab, transform.position, Quaternion.identity);

            trampoline.transform.right = transform.position - shooterSpriteTransform.position;
            //if (aim.y != 0) trampoline.transform.eulerAngles = new Vector3(0, 0, 90);
            _ghostValues._playerManager._soundManager.PlayTrampolineShootSound();
            _cameraShake.Shake(0.1f, 0.1f);
            trampoline.GetComponent<Rigidbody2D>().AddForce(aim * _ghostValues.trampolineSpeed, ForceMode2D.Impulse);
            Trampoline trampolineComp = trampoline.GetComponent<Trampoline>();
            trampolineComp._aimDirection = aim;
           
        }
    }

    private void SetSooterPosition(Vector2 direction)
    {
        Ray ray = new Ray(transform.position, direction);
        shooterSpriteTransform.position = ray.GetPoint(distance);
        shooterSpriteTransform.right = transform.position - shooterSpriteTransform.position;
        shooterSprite.enabled = true;
        displayed = true;
    }

    private void HideShooter()
    {
        displayed = false;
        shooterSprite.enabled = false;
    }
}