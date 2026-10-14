using Unity.Cinemachine;
using UnityEngine;

namespace Feel
{
    public class CameraSway : MonoBehaviour
    {
        [Header("Tilt, Pan, Dutch")]

        [SerializeField]
        private Vector3 _speed;

        [SerializeField]
        private Vector3 _strength;

        [SerializeField]
        private CinemachineRecomposer _recomposer;

        private void Update()
        {
            float tilt = (Mathf.PerlinNoise1D((Time.time + 1.1f) * _speed.x) - 0.5f) * _strength.x;
            float pan = (Mathf.PerlinNoise1D((Time.time + 2.5f) * _speed.y) - 0.5f) * _strength.y;
            float dutch = (Mathf.PerlinNoise1D((Time.time + 3.8f) * _speed.z) - 0.5f) * _strength.z;

            _recomposer.Dutch = dutch;
            _recomposer.Pan = pan;
            _recomposer.Tilt = tilt;
        }
    }
}