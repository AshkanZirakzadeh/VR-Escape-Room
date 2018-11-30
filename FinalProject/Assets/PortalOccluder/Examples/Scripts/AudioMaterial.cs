using UnityEngine;
using System.Collections;

/// <summary>
/// An audio material used for footsteps.
/// </summary>
public class AudioMaterial : MonoBehaviour {

    public AudioClip[] footsteps = new AudioClip[1];
    private int nextSound = 0;

    private int NextSoundIndex()
    {
        nextSound += Random.Range(1, 3);
        //nextSound += 1;
        while (nextSound >= footsteps.Length) nextSound -= footsteps.Length;
        if (nextSound < 0) nextSound = 0;
        return nextSound;
    }

    /// <summary>
    /// Play one of the given material sounds.
    /// </summary>
    public void PlayRandomFootstep(AudioSource source)
    {
        if (source != null && footsteps != null && footsteps.Length > 0)
        {
            var clip = footsteps[NextSoundIndex()];
            if (clip != null)
                source.PlayOneShot(clip);
        }
    }
}