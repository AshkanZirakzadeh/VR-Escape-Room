using UnityEngine;
using System.Collections;

public class AudioMaterialReference : MonoBehaviour {
    public AudioMaterial AudioMaterial = null;

    public void PlayRandomFootstep(AudioSource source)
    {
        if (AudioMaterial != null) AudioMaterial.PlayRandomFootstep(source);
    }
}
