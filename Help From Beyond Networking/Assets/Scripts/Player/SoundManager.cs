
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    //movement
    [SerializeField] private AudioClip jumpSound, doubleJump;

    //ghost
    [Header("Ghost")] [SerializeField] private AudioClip trampolineShoot;

    public void PlayJumpSound()
    {
        _audioSource.clip = jumpSound;
        _audioSource.Play();
    }

    public void PlayDoubleJumpSound()
    {
        _audioSource.clip = doubleJump;
        _audioSource.Play();
    }

    public void PlayTrampolineShootSound()
    {        _audioSource.clip = trampolineShoot;
        _audioSource.Play();
    }
}