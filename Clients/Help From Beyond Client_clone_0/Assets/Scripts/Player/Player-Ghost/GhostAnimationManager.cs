
using UnityEngine;

public class GhostAnimationManager : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
