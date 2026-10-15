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

        [SerializeField]
        private SpriteRenderer _spriteRen;

        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _chargedSprite;

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

        public void SetVisuals(GameState state)
        {
            _spriteRen.sprite = state.PlayerState.BroomCharges > 0 ? _chargedSprite : _normalSprite;
        }
    }
}