using UnityEngine;
using System.Collections;

namespace CirrusPlay.PortalLibrary.Examples
{
    [RequireComponent(typeof(Room))]
    public class DebugRoom : MonoBehaviour
    {

        Room room;
        public Material invisible = null;
        public Material visible = null;

        // Use this for initialization
        void Start()
        {
            this.room = GetComponent<Room>();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material = room.ContainsCamera(Camera.main) ? visible : invisible;
            }
        }
    }

}