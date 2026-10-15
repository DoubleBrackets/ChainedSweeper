using Map;
using UnityEngine;

namespace Protag
{
    public class ProtagController : MonoBehaviour
    {
        [SerializeField]
        private Transform _body;

        [SerializeField]
        private Vector3 _offset;

        public void Initialize()
        {
        }

        public void Cleanup()
        {
        }

        public void OnSweep(SweepNode node)
        {
            _body.transform.position = node.Pos + _offset;
        }
    }
}