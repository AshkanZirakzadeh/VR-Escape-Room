using UnityEngine;
using System.Collections;
using CirrusGames;
using UnityEngine.VR;

namespace CirrusPlay.PortalLibrary.Examples
{

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Omniput))]
    public class Character : MonoBehaviour
    {

        public float speed = 3.0F;
        public float rotateSpeed = 3.0F;
        Vector3 angles;

        public float footstepDelay = 0;
        public float footstepsFrequency = 1.0f;
        //private bool shouldPlayFootsteps = false;
        private bool running = false;

        // Use this for initialization
        void Start()
        {
            var camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                angles = camera.transform.rotation.eulerAngles;
            }
        }

        void DebugControls()
        {
            // Quit on escape //
            if (Omniput.PressExit()) Application.Quit();
        }

        void PlayFootsteps(AudioMaterialReference material)
        {
            var frequency = running ? footstepsFrequency * 0.5f : footstepsFrequency;
            if (material != null && footstepDelay >= frequency)
            {
                footstepDelay = 0.0f;
                material.PlayRandomFootstep(GetComponent<AudioSource>());
            }
        }

        void OnControllerColliderHit(ControllerColliderHit collision)
        {
            PlayFootsteps(collision.gameObject.GetComponent<AudioMaterialReference>());
        }

        void AllowPlayFootsteps(bool value)
        {
            //shouldPlayFootsteps = value;
            if (value) footstepDelay += Time.deltaTime;
        }

        void MovementControls()
        {
            var camera = GetComponentInChildren<Camera>();
            CharacterController controller = GetComponent<CharacterController>();
            angles += Omniput.LookVector() * rotateSpeed;
            var speed = this.speed;
            running = false;
            if (Omniput.PressingRun()) { speed *= 2; running = true; }
            // Clamp vertical view angles //
            if (angles.x < -90) angles.x = -90;
            if (angles.x > 90) angles.x = 90;
            // Wrap horizontal angles //
            if (angles.y > 360) angles.y -= 360;
            if (angles.y < 0) angles.y += 360;
            float xRotMul = UnityEngine.XR.XRSettings.enabled ? 0 : 1;
            // Convert axis-angle to quaternion //
            if (camera != null)
            {
                var rotX = Quaternion.AngleAxis(angles.x * xRotMul, Vector3.right);
                var rotY = Quaternion.AngleAxis(angles.y, Vector3.up);
                transform.rotation = rotY;
                camera.transform.rotation = rotY * rotX;
            }
            else
            {
                var rot = Quaternion.AngleAxis(angles.y, Vector3.up) *
                    Quaternion.AngleAxis(angles.x * xRotMul, Vector3.right);
                // Set rotation of the controller //
                transform.rotation = rot;
            }
            var move = transform.TransformDirection(Omniput.MoveVector());
            move *= speed;
            AllowPlayFootsteps(move.sqrMagnitude > 0);
            //if (move.sqrMagnitude > 0) move = move.normalized * speed;
            controller.SimpleMove(move);
        }

        // Update is called once per frame
        void Update()
        {

            DebugControls();
            MovementControls();

            Cursor.lockState = CursorLockMode.Locked;

        }
    }

}