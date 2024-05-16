using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip pieceSelect;
    public AudioClip piecePlace;
    public AudioClip pieceStack;
    public AudioClip pieceAttack;
    public AudioClip pieceDestroy;
    public AudioClip pieceRotate;
    public AudioClip pieceDraw;
    public AudioClip pieceMove;
    public AudioClip cardDraw;
    public AudioClip cardSelect;
    public AudioClip abilityHeal;
    public AudioClip abilityIncreaseAngle;
    public AudioClip abilityDecreaseAngle;
    public AudioClip abilityRemoveAll;

    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
