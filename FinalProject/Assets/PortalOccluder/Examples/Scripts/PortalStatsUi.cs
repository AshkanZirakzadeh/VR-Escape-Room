using CirrusGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CirrusPlay.PortalLibrary.Examples
{

    public class PortalStatsUi : MonoBehaviour
    {

        public enum ProcessState
        {
            Disable,
            Enable,
            Freeze
        }

        private ProcessState State = ProcessState.Enable;
        public Text PortalStatusText;
        public Text PortalFPSText;

        private int frames = 0;
        private float frameTime = 0;
        private int lastFrameRate = 0;

        private static readonly string[] StateText = new string[]
        {
        "Disabled",
        "Enabled",
        "Frozen"
        };

        void DebugControls()
        {
            // Do portal debugging //
            if (Input.GetKey(KeyCode.F1)) SetState(ProcessState.Enable);
            if (Input.GetKey(KeyCode.F2)) SetState(ProcessState.Freeze);
            if (Input.GetKey(KeyCode.F3)) SetState(ProcessState.Disable);
        }

        void UpdateText()
        {
            frameTime += Time.deltaTime;
            frames += 1;
            if (frameTime >= 1.0f)
            {
                frameTime -= 1.0f;
                lastFrameRate = frames;
                frames = 0;
            }

            if (PortalStatusText != null)
            {
                PortalStatusText.text = "Portal Mode (F1/F2/F3): " + StateText[(int)State];
            }

            if (PortalFPSText != null)
            {
                PortalFPSText.text = "FPS: " + lastFrameRate.ToString();
            }
        }

        void SetState(ProcessState state)
        {
            this.State = state;
            PortalManager.EnableProcessing(state == ProcessState.Enable, state == ProcessState.Freeze);
        }

        // Update is called once per frame
        void Update()
        {
            DebugControls();
            UpdateText();
        }
    }

}