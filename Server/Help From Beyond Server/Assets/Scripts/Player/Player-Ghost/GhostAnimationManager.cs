
using UnityEngine;

public class GhostAnimationManager : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }
}
