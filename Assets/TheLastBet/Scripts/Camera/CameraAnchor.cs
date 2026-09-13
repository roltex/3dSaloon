using UnityEngine;

namespace TheLastBet.Saloon
{
    /// <summary>
    /// Marker/authoring component for cinematic or gameplay camera positions.
    /// A camera controller can move/blend to this transform without hard-coded coordinates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = "camera-anchor";
        [SerializeField] private float fieldOfView = 45f;
        [SerializeField] private float blendDuration = 0.65f;

        public string AnchorId => anchorId;
        public float FieldOfView => fieldOfView;
        public float BlendDuration => blendDuration;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);
        }
#endif
    }
}
