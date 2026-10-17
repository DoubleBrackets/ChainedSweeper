using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class SweepNode : MonoBehaviour
    {
        [SerializeField]
        private List<SweepNode> _adjacentNodes = new();

        [SerializeField]
        private SweepConnectionVisuals _connectionVisualsPrefab;

        [SerializeField]
        private GameObject _activeObject;

        [SerializeField]
        private GameObject _inactiveObject;

        [SerializeField]
        private ParticleSystem _sweptParticles;

        private int _runtimeIndex;

        public int RuntimeIndex => _runtimeIndex;

        public List<SweepNode> AdjacentNodes => _adjacentNodes;

        public Vector3 Pos => transform.position;

        private Dictionary<PatternStep, SweepConnectionVisuals> _stepVisuals = new();

        public SweepNode GetRandomAdjacentNode()
        {
            if (_adjacentNodes.Count == 0)
            {
                return null;
            }

            return _adjacentNodes[Random.Range(0, _adjacentNodes.Count)];
        }

        private void OnDrawGizmosSelected()
        {
            foreach (SweepNode adj in _adjacentNodes)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Pos, adj.Pos);
            }
        }

        public void SetIndex(int index)
        {
            _runtimeIndex = index;
        }

        public void Tick(GameState state)
        {
            if (!state.TryGetCurrentChallenge(out Challenge challenge))
            {
                CleanupVisuals();
                return;
            }

            var relevantSteps = new List<PatternStep>();
            foreach (PatternInstance pattern in challenge.Patterns)
            {
                List<PatternStep> relevant = pattern.StepsThatGoTo(this);

                foreach (PatternStep step in relevant)
                {
                    UpdateStepVisual(step, pattern);
                }

                if (!pattern.Finished)
                {
                    relevantSteps.AddRange(relevant);
                }
            }

            _activeObject.SetActive(relevantSteps.Count > 0);
            _inactiveObject.SetActive(relevantSteps.Count == 0);
        }

        private void UpdateStepVisual(PatternStep step, PatternInstance challenge)
        {
            if (!_stepVisuals.TryGetValue(step, out SweepConnectionVisuals visuals))
            {
                SweepConnectionVisuals newVisual = Instantiate(_connectionVisualsPrefab, transform);
                newVisual.Initialize();
                _stepVisuals.Add(step, newVisual);
                visuals = newVisual;
            }

            if (challenge.IsAfterNext(step))
            {
                visuals.SetIndicator(LineIndicator.AfterNext, step.From.Pos, step.To.Pos);
            }
            else if (challenge.IsNext(step))
            {
                visuals.SetIndicator(LineIndicator.Next, step.From.Pos, step.To.Pos);
            }
            else if (step.Completed)
            {
                visuals.SetIndicator(LineIndicator.Swept, step.From.Pos, step.To.Pos);
            }
            else
            {
                visuals.SetIndicator(LineIndicator.Unswept, step.From.Pos, step.To.Pos);
            }
        }

        private void CleanupVisuals()
        {
            foreach (KeyValuePair<PatternStep, SweepConnectionVisuals> pair in _stepVisuals)
            {
                Destroy(pair.Value.gameObject);
            }

            _stepVisuals.Clear();
            _activeObject.SetActive(false);
            _inactiveObject.SetActive(true);
        }

        public void PatternEnded()
        {
            _sweptParticles?.Play();
            CleanupVisuals();
        }
    }
}