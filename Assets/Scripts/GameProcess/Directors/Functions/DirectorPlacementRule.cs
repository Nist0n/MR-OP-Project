using UnityEngine;

namespace GameProcess.Directors.Functions
{
    public class DirectorPlacementRule
    {
        public Transform SpawnOnTarget;
        public Vector3 Position;
        public PlacementMode placementMode;
        public float MinDistance;
        public float MaxDistance;
        public bool PreventOverhead;

        public Vector3 targetPosition => !SpawnOnTarget ? Position : SpawnOnTarget.position;

        public enum PlacementMode
        {
            Direct,
            Approximate,
            ApproximateSimple,
            NearestNode,
            Random,
            RandomNormalized,
        }
    }
}
