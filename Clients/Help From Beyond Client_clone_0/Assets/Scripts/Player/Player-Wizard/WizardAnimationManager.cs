using UnityEngine;

public class WizardAnimationManager : MonoBehaviour
{
    // Reference to the Animator component
    private Animator _animator;

    // Hashes for the animator parameters
    private static readonly int Jumping = Animator.StringToHash("Jumping");
    private static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int JoystickMultipiler = Animator.StringToHash("JoystickMultiplier");
    private static readonly int Dashing = Animator.StringToHash("Dashing");
    private static readonly int Invulnerability = Animator.StringToHash("Invulnerability");

    // Reference to the SpriteRenderer component
    [SerializeField] private SpriteRenderer _spriteRenderer;

    void Start()
    {
        // Get the Animator component from the child object
        _animator = GetComponentInChildren<Animator>();
    }

    // Set the Jumping parameter in the animator
    public void SetJumping(bool value)
    {
        _animator.SetBool(Jumping, value);
    }

    // Set the Falling parameter in the animator
    public void SetFalling(bool value)
    {
        _animator.SetBool(Falling, value);
    }

    // Set the Speed parameter in the animator
    public void SetSpeed(float value)
    {
        _animator.SetFloat(Speed, value);
    }

    // Set the JoystickMultiplier parameter in the animator
    public void SetJoystickMultiplier(float value)
    {
        _animator.SetFloat(JoystickMultipiler, Mathf.Min(value, 2));
    }

    // Set the Dashing parameter in the animator
    public void SetDashing(bool value)
    {
        _animator.SetBool(Dashing, value);
    }
}