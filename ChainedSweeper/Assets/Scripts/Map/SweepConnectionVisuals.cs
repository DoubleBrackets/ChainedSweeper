using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public enum LineIndicator
    {
        None,
        Unswept,
        Next,
        Swept
    }

    public class SweepConnectionVisuals : MonoBehaviour
    {
        [SerializeField]
        private List<LineRenderer> _indicatorLineRenderers;

        [SerializeField]
        private ParticleSystem _targetedParticles;

        public void Initialize()
        {
            SetIndicator(LineIndicator.None);
        }

        public void SetIndicator(LineIndicator indicator, Vector3 a = default, Vector3 b = default)
        {
            for (var i = 0; i < _indicatorLineRenderers.Count; i++)
            {
                LineRenderer lineRen = _indicatorLineRenderers[i];
                if (i == (int)indicator - 1)
                {
                    lineRen.enabled = true;
                    lineRen.positionCount = 3;
                    lineRen.SetPosition(0, a);
                    lineRen.SetPosition(1, (a + b) / 2);
                    lineRen.SetPosition(2, b);
                }
                else
                {
                    lineRen.enabled = false;
                }
            }

            if (indicator is LineIndicator.Next)
            {
                _targetedParticles.Play();
            }
            else
            {
                _targetedParticles.Stop();
            }
        }
    }
}