using UnityEngine;

namespace Protag
{
    public class ProtagController : MonoBehaviour
    {
        private IGameInput _gameInput;

        public void Initialize(IGameInput gameInput)
        {
            _gameInput = gameInput;
        }

        public void Cleanup()
        {
        }
    }
}