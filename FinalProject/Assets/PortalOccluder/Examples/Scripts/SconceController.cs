using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CirrusPlay.PortalLibrary.Examples
{

    [ExecuteInEditMode]
    public class SconceController : MonoBehaviour
    {

        delegate void Action();

        public bool lightOn = true;
        public Light[] lights = new Light[1];
        public float[] lightIntensityMultipliers = new float[0];
        public float intensity = 1;
        public Color color = new Color(1, 1, 1);
        public bool controlIntensity = true;
        public bool controlColor = true;
        public MeshRenderer[] emissiveMaterialMeshRenderers = new MeshRenderer[2];
        private Material[] emissiveMaterials = new Material[2];
        public Transform lightSource = null;

        private float deltaTime = 0.0f;
        [Header("Light Flicker")]
        public bool flickerEnabled = false;
        private float flickerOffset = 0.0f;
        private float flickerMaxIntensity = 1.0f;
        public float flickerMinIntensity = 0.5f;
        
        private bool playerInside = false;

        private void OnEnable()
        {
            if (Application.isPlaying)
                SetupEmissiveMaterials();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                DestroyEmissiveMaterials();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player") playerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player") playerInside = false;
        }

        private void SetupEmissiveMaterials()
        {
            if (Application.isPlaying)
            {
                if (emissiveMaterialMeshRenderers != null)
                {
                    emissiveMaterials = new Material[emissiveMaterialMeshRenderers.Length];
                    for (var i = 0; i < emissiveMaterialMeshRenderers.Length; i++)
                    {
                        if (emissiveMaterialMeshRenderers[i] != null)
                            emissiveMaterials[i] = emissiveMaterialMeshRenderers[i].material;
                    }
                }
            }
        }

        private void DestroyEmissiveMaterials()
        {
            if (emissiveMaterials != null)
            {
                for (var i = 0; i < emissiveMaterials.Length; i++)
                {
                    if (emissiveMaterials[i] != null)
                        Object.Destroy(emissiveMaterials[i]);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            deltaTime += Time.deltaTime;
            ControlLights();
            UpdateEmissiveMaterial(emissiveMaterials);
            On(flickerEnabled, FlickerLights);
            DoUserLightSwitch();
        }

        void DoUserLightSwitch()
        {
            if (playerInside && Omniput.PressActivate())
            {
                this.lightOn = !this.lightOn;
            }
        }

        // This is me playing around to see how a conditional function works for readability //
        private static bool On(bool condition, Action action)
        {
            if (condition) action();
            return condition;
        }

        private void UpdateEmissiveMaterial(Material[] materials)
        {
            var flickering = this.flickerEnabled ? FlickerIntensity() : 1.0f;
            var onIntensity = lightOn ? 1.0f : 0.0f;
            if (materials != null)
            {
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        materials[i].SetColor("_EmissionColor", this.color * 0.1f * intensity * flickering * onIntensity);
                        if (lightSource != null)
                            materials[i].SetVector("_LightSource", transform.position);
                    }
                }
            }
        }

        private void ControlLights()
        {
            var onIntensity = lightOn ? 1.0f : 0.0f;
            for (var i = 0; i < lights.Length; i++)
            {
                var light = lights[i];
                var intensityMultiplier = (i < lightIntensityMultipliers.Length ? lightIntensityMultipliers[i] : 1) * onIntensity;
                if (light != null)
                {
                    if (controlIntensity) light.intensity = intensity * intensityMultiplier;
                    if (controlColor) light.color = color;
                }
            }
        }

        private float FlickerIntensity()
        {
            return Mathf.Min(flickerMaxIntensity, Mathf.Max(flickerMinIntensity,
                        Mathf.Sin(deltaTime * 13 + flickerOffset) +
                        Mathf.Cos(deltaTime * 33 + flickerOffset) +
                        Mathf.Sin(deltaTime * 21 + flickerOffset)
                        ));
        }

        private void FlickerLights()
        {
            var onIntensity = lightOn ? 1.0f : 0.0f;
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.intensity = FlickerIntensity() * this.intensity * onIntensity;
                }
            }
        }
    }
}