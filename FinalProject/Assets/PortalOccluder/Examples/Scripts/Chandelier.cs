using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Chandelier : MonoBehaviour {


    public ParticleSystem particles;

    private ChandelierBulb[] bulbs;
    private ParticleSystem.Particle[] flares;

    private void OnEnable()
    {
        if (particles != null)
        {
            particles.Clear();
            this.bulbs = this.GetComponentsInChildren<ChandelierBulb>();
            this.flares = new ParticleSystem.Particle[bulbs.Length];

            particles.Emit(bulbs.Length);
            particles.GetParticles(flares);

            for (var i = 0; i < bulbs.Length; i++)
            {
                var bulb = bulbs[i];
                flares[i].position = bulb.transform.position;
            }

            particles.SetParticles(flares, flares.Length);
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
