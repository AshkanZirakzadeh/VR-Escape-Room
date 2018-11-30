using UnityEngine;
using System.Collections;
using CirrusGames;

namespace CirrusPlay.PortalLibrary.Examples
{
    public class Door : MonoBehaviour
    {

        [Header("System Settings")]
        public Animator doorAnimator = null;
        public Collider doorCollider;

        [Header("Custom Configuration")]
        public bool locked = false;
        public bool autoClose = true;

        [Header("Sound Effects")]
        public AudioSource soundSource;
        public AudioClip soundOpen = null;
        public AudioClip soundClosed = null;
        public AudioClip soundLocked = null;

        private int canOpen = 0;
        private bool open = false;
        private Portal portal;

        // Use this for initialization
        void Start()
        {
            portal = GetComponentInChildren<Portal>();
            if (portal != null)
                portal.open = open;
            this.soundSource = this.soundSource ?? GetComponent<AudioSource>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player") canOpen++;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player") canOpen--;

            AutoCloseDoors();
        }

        void AutoCloseDoors()
        {
            // Automatically close doors //
            if (canOpen == 0 && open && autoClose)
            {
                OpenDoor(false);
                PlaySound(soundClosed);
            }
        }

        void OpenDoor(bool value)
        {
            if (doorAnimator != null)
            {
                // Simply sets the "Open" state of the door to value //
                open = value;
                // Then triggers the Open/Close animation depending on value //
                if (value)
                    doorAnimator.SetTrigger("OpenDoor");
                else
                    doorAnimator.SetTrigger("CloseDoor");
            }
        }

        void PlaySound(AudioClip clip)
        {
            // If we have a sound source and clip, play it! //
            if (soundSource != null && clip != null)
            {
                soundSource.PlayOneShot(clip);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Check to see if the user pressed the "Activate" button input //
            // (For this demo, that's probably Left+Click) //

			/*
            if (Omniput.PressActivate() && canOpen > 0)
            {
                // If the door is not locked, we'll open/close it //
                if (!locked)
                {
                    if (open) { OpenDoor(false); PlaySound(soundClosed); }
                    else { OpenDoor(true); PlaySound(soundOpen); }
                }
                // Otherwise, the door is locked, so we'll play the locked-door noise //
                else
                {
                    PlaySound(soundLocked);
                }
            }*/

            // Set the state of the Portal component if we have one //
            if (portal != null)
            {
                portal.open = IsOpen();
            }

            // We'll shut the collider off if the door is not exactly closed //
            if (doorCollider != null)
            {
                doorCollider.enabled = !IsOpen();
            }
        }

        // Determine if the door is in any animation status other than closed //
        private bool IsOpen()
        {
            return doorAnimator != null ? !doorAnimator.GetCurrentAnimatorStateInfo(0).IsName("Door1-Idle") : open;
        }

		public void grab(){
			
			if (!locked)
			{
				if (open) { OpenDoor(false); PlaySound(soundClosed); }
				else { OpenDoor(true); PlaySound(soundOpen); }
			}
			// Otherwise, the door is locked, so we'll play the locked-door noise //
			else
			{
				PlaySound(soundLocked);
			}

		}
    }
}