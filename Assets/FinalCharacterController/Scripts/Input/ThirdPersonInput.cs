using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FinalCharacterController {

    [DefaultExecutionOrder(-2)]
    public class ThirdPlayerInput : MonoBehaviour, PlayerControls.IThirdPersonMapActions {

        public Vector2 ScrollInput { get; private set; }

        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private float cameraZoomSpeed = 1f;
        [SerializeField] private float cameraMinZoom = 1f;
        [SerializeField] private float cameraMaxZoom = 2f;

        private Cinemachine3rdPersonFollow thirdPersonFollow;

        private void Awake() {

            thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        private void Update() {

            thirdPersonFollow.CameraDistance = Mathf.Clamp(thirdPersonFollow.CameraDistance + ScrollInput.y, cameraMinZoom, cameraMaxZoom);
        }

        private void LateUpdate() {

            ScrollInput = Vector2.zero;
        }

        private void OnEnable() {

            if(PlayerInputManager.Instance?.PlayerControls == null) {
                Debug.LogError("Player Controls is not set");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Enable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
        }

        private void OnDisable() {

            if(PlayerInputManager.Instance?.PlayerControls == null) {
                Debug.LogError("Player Controls is not set");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.Disable();
            PlayerInputManager.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
        }

        public void OnScrollCamera(InputAction.CallbackContext context) {

            if(!context.performed) {
                return;
            }

            Vector2 scrollInput = context.ReadValue<Vector2>();
            ScrollInput = scrollInput.normalized * cameraZoomSpeed * -1f;
        }
    }
}