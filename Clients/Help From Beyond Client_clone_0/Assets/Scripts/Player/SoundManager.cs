using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource; // Reference to the AudioSource component

    // Movement sounds
    [SerializeField] private AudioClip jumpSound, doubleJump; // Audio clips for jump and double jump sounds

    // Ghost sounds
    [Header("Ghost")][SerializeField] private AudioClip trampolineShoot; // Audio clip for trampoline shoot sound

    // Method to play the jump sound
    public void PlayJumpSound()
    {
        _audioSource.clip = jumpSound;
        _audioSource.Play();
    }

    // Method to play the double jump sound
    public void PlayDoubleJumpSound()
    {
        _audioSource.clip = doubleJump;
        _audioSource.Play();
    }

    // Method to play the trampoline shoot sound
    public void PlayTrampolineShootSound()
    {
        _audioSource.clip = trampolineShoot;
        _audioSource.Play();
    }
}