using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class SelectPreviewSpin : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 28f;

        void Update()
        {
            transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f);
        }
    }
}
