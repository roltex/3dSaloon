using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class BakedMeshCollider : MonoBehaviour
    {
        [SerializeField] private Mesh baked;

        public void Bind(Mesh mesh)
        {
            baked = mesh;
        }

        void OnDestroy()
        {
            if (baked == null)
                return;
            if (Application.isPlaying)
                Destroy(baked);
            else
                DestroyImmediate(baked);
            baked = null;
        }
    }
}
