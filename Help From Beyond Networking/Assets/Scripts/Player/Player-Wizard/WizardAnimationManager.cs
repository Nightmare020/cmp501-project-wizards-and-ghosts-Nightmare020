
using UnityEngine;

public class WizardAnimationManager : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator _animator;
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int JoystickMultipiler = Animator.StringToHash("JoystickMultiplier");
    private static readonly int Dashing = Animator.StringToHash("Dashing");
    private static readonly int Invulnerability = Animator.StringToHash("Invulnerability");
    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }


    public void SetJumping(bool value)
    {
        _animator.SetBool(Jumping, value);
    }

    public void SetFalling(bool value)
    {
        _animator.SetBool(Falling, value);
    }

    public void SetSpeed(float value)
    {
        _animator.SetFloat(Speed, value);
    }

    public void SetJoystickMultiplier(float value)
    {
        _animator.SetFloat(JoystickMultipiler, Mathf.Min(value, 2));
    }

    public void SetDashing(bool value)
    {
        _animator.SetBool(Dashing, value);
    }


}