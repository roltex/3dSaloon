using UnityEngine;

namespace TheLastBet.Saloon
{
    public static class CombatLayers
    {
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";
        public const string ViewmodelName = "Viewmodel";

        public static int Player => LayerMask.NameToLayer(PlayerName);
        public static int Enemy => LayerMask.NameToLayer(EnemyName);
        public static int Viewmodel => LayerMask.NameToLayer(ViewmodelName);

        public static LayerMask HitMask
        {
            get
            {
                int mask = LayerMask.GetMask("Default", PlayerName, EnemyName);
                int view = Viewmodel;
                if (view >= 0)
                    mask &= ~(1 << view);
                return mask == 0 ? ~0 : mask;
            }
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go == null || layer < 0)
                return;
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}
