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

        public List<SweepNode> AdjacentNodes => _adjacentNodes;

        public Vector3 Pos => transform.position;

        private Dictionary<ChallengeStep, SweepConnectionVisuals> _stepVisuals = new();

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

        public void Tick(GameState state)
        {
            if (!state.TryGetCurrentChallenge(out Challenge challenge))
            {
                CleanupVisuals();
                return;
            }

            List<ChallengeStep> relevantSteps = challenge.StepsThatGoTo(this);

            foreach (ChallengeStep step in relevantSteps)
            {
                UpdateStepVisual(step);
            }
        }

        private void UpdateStepVisual(ChallengeStep step)
        {
            if (!_stepVisuals.TryGetValue(step, out SweepConnectionVisuals visuals))
            {
                SweepConnectionVisuals newVisual = Instantiate(_connectionVisualsPrefab, transform);
                newVisual.Initialize();
                _stepVisuals.Add(step, newVisual);
                visuals = newVisual;
            }

            if (step.IsNext)
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
            foreach (KeyValuePair<ChallengeStep, SweepConnectionVisuals> pair in _stepVisuals)
            {
                Destroy(pair.Value.gameObject);
            }

            _stepVisuals.Clear();
        }

        public void ChallengeEnded()
        {
            CleanupVisuals();
        }
    }
}