using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FinalCharacterController {

    [DefaultExecutionOrder(-2)]
    public class PlayerActionInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions {

        private PlayerLocomotionInput locomotionInput;
        private PlayerState playerState;

        public bool AttackPressed { get; private set; }
        public bool GatherPressed { get; private set; }

        private void Awake() {
            locomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
        }

        private void OnEnable() {

            if(PlayerInputManager.Instance?.PlayerControls == null) {
                Debug.LogError("Player Controls is not set");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.SetCallbacks(this);
        }

        private void Update() {

            if(locomotionInput.MovementInput != Vector2.zero ||
                playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping ||
                playerState.CurrentPlayerMovementState == PlayerMovementState.Falling) {

                GatherPressed = false;
            }
        }

        private void OnDisable() {

            if(PlayerInputManager.Instance?.PlayerControls == null) {
                Debug.LogError("Player Controls is not set");
                return;
            }

            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();
            PlayerInputManager.Instance.PlayerControls.PlayerActionMap.RemoveCallbacks(this);
        }

        public void SetAttackToFalse() {
            AttackPressed = false;
        }

        public void SetGatherToFalse() {
            GatherPressed = false;
        }

        public void OnAttack(InputAction.CallbackContext context) {

            if(!context.performed) {
                return;
            }

            AttackPressed = true;
        }

        public void OnGather(InputAction.CallbackContext context) {

            if(!context.performed) {
                return;
            }

            GatherPressed = true;
        }
    }
}