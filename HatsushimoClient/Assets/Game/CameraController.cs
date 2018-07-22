using UniRx;
using System.Collections;
using UnityEngine;

namespace Assets.Game
{
    class CameraController : MonoBehaviour
    {
        Camera cam;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Start()
        {
            MainThreadDispatcher.StartUpdateMicroCoroutine(BeginMoveCamera());   
        }

        IEnumerator BeginMoveCamera()
        {
            while(true)
            {
                var campos = cam.transform.position;
                var playerpos = transform.position;
                campos.x = playerpos.x;
                campos.y = playerpos.y;
                cam.transform.position = campos;
                yield return null;
            }
        }
    }
}
