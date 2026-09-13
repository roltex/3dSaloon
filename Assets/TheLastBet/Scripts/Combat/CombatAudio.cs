using UnityEngine;

namespace TheLastBet.Saloon
{
    public static class CombatAudio
    {
        static AudioClip _fire;
        static AudioClip _reload;

        public static AudioClip Fire => _fire != null ? _fire : (_fire = BuildFire());
        public static AudioClip Reload => _reload != null ? _reload : (_reload = BuildReload());

        public static void AssignGenerated(ref AudioClip fire, ref AudioClip reload)
        {
            if (fire == null)
                fire = Fire;
            if (reload == null)
                reload = Reload;
        }

        static AudioClip BuildFire()
        {
            const int hz = 44100;
            int count = Mathf.RoundToInt(hz * 0.16f);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)hz;
                float env = Mathf.Exp(-t * 26f);
                float bang = Mathf.Sin(t * 180f * Mathf.PI * 2f) * 0.35f;
                float click = Mathf.Sin(t * 920f * Mathf.PI * 2f) * 0.18f;
                float grit = (Hash(i) * 2f - 1f) * 0.55f;
                data[i] = Mathf.Clamp(env * (bang + click + grit), -1f, 1f);
            }

            return Make("RevolverFire", data, hz);
        }

        static AudioClip BuildReload()
        {
            const int hz = 44100;
            int count = Mathf.RoundToInt(hz * 0.28f);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)hz;
                float env = Mathf.Exp(-Mathf.Abs(t - 0.08f) * 18f);
                float metal = Mathf.Sin(t * 640f * Mathf.PI * 2f) * 0.4f;
                float tick = Mathf.Sin(t * 1400f * Mathf.PI * 2f) * 0.12f;
                data[i] = Mathf.Clamp(env * (metal + tick), -1f, 1f);
            }

            return Make("RevolverReload", data, hz);
        }

        static AudioClip Make(string name, float[] data, int hz)
        {
            var clip = AudioClip.Create(name, data.Length, 1, hz, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Hash(int value)
        {
            uint x = (uint)value * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return (x & 0xFFFF) / 65535f;
        }
    }
}
