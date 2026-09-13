using JohnStairs.RPG.Character.Cam;
using JohnStairs.RPG.Character.Cam.Subcomponents;
using JohnStairs.RPG.Character.Cam.Subcomponents.Enums;
using UnityEngine;

namespace TheLastBet.Saloon
{
    public sealed class RpgCameraBinder : MonoBehaviour
    {
        RPGCameraLite _rpg;
        PivotLite _pivot;
        bool _roaming = true;

        public static RpgCameraBinder Bind(GameObject character, Camera usedCamera)
        {
            if (character == null)
                return null;

            var binder = character.GetComponent<RpgCameraBinder>();
            if (binder == null)
                binder = character.AddComponent<RpgCameraBinder>();
            binder.EnsureStack(usedCamera);
            binder.SetRoaming(true);
            return binder;
        }

        public void SetRoaming(bool roaming)
        {
            _roaming = roaming;
            if (_rpg != null)
                _rpg.enabled = roaming;
            enabled = roaming;
        }

        void EnsureStack(Camera usedCamera)
        {
            float scale = Mathf.Max(0.01f, transform.lossyScale.y);
            if (gameObject.CompareTag("Untagged"))
                gameObject.tag = "Player";

            _rpg = GetComponent<RPGCameraLite>();
            if (_rpg == null)
                _rpg = gameObject.AddComponent<RPGCameraLite>();
            _rpg.UsedCamera = usedCamera;
            _rpg.MinPitch = -18f;
            _rpg.MaxPitch = 48f;
            _rpg.MinDistance = 1.15f * scale;
            _rpg.MaxDistance = 12f * Mathf.Lerp(1f, scale, 0.6f);
            _rpg.StartPitch = 12f;
            _rpg.StartDistance = 5.2f * Mathf.Lerp(1f, scale, 0.7f);
            _rpg.StartYaw = 48f;
            _rpg.StartYawRelativeToCharacterRotation = false;

            _pivot = GetComponent<PivotLite>();
            if (_pivot == null)
                _pivot = gameObject.AddComponent<PivotLite>();
            _pivot.LocalPosition = new Vector3(0f, 1.22f * scale, 0f);

            var frustum = GetComponent<ViewFrustum>();
            if (frustum == null)
                frustum = gameObject.AddComponent<ViewFrustum>();
            frustum.CheckedLayers = ~0;
            frustum.IgnoredTags = new System.Collections.Generic.List<string> { "Player" };

            var occlusion = GetComponent<OcclusionHandlerLite>();
            if (occlusion == null)
                occlusion = gameObject.AddComponent<OcclusionHandlerLite>();
            occlusion.ZoomConditions = new System.Collections.Generic.List<OcclusionHandlerLite.Condition>
            {
                new OcclusionHandlerLite.Condition(ConditionType.Layer, "Default")
            };
        }

        void Update()
        {
            if (!_roaming || _rpg == null || Cursor.lockState != CursorLockMode.Locked)
                return;

            ReadLook(out float mouseX, out float mouseY, out float stickX, out float stickY, out float zoom);
            _rpg.Yaw(mouseX * 0.16f + stickX * 210f * Time.deltaTime);
            _rpg.Pitch(-(mouseY * 0.16f + stickY * 165f * Time.deltaTime));
            if (Mathf.Abs(zoom) > 0.001f)
                _rpg.Zoom(-zoom * 0.85f);
        }

        static void ReadLook(out float mouseX, out float mouseY, out float stickX, out float stickY, out float zoom)
        {
            mouseX = 0f;
            mouseY = 0f;
            stickX = 0f;
            stickY = 0f;
            zoom = 0f;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue();
                mouseX = delta.x;
                mouseY = delta.y;
                zoom = mouse.scroll.ReadValue().y * 0.04f;
            }

            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.rightStick.ReadValue();
                stickX = stick.x;
                stickY = stick.y;
            }
#else
            mouseX = Input.GetAxis("Mouse X") * 12f;
            mouseY = Input.GetAxis("Mouse Y") * 12f;
            zoom = Input.GetAxis("Mouse ScrollWheel") * 8f;
            stickX = Input.GetAxis("Look X");
            stickY = Input.GetAxis("Look Y");
#endif
        }
    }
}
