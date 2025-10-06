using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalCharacterController {

    public class PlayerAnimation : MonoBehaviour {

        public static int InputXHash = Animator.StringToHash("inputX");
        public static int InputYHash = Animator.StringToHash("inputY");
        public static int InputMagnitudeHash = Animator.StringToHash("inputMagnitude");

        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 0.02f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private Vector3 currentBlendInput = Vector3.zero;

        private void Awake() {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
        }

        private void Update() {

            UpdateAnimationState();
        }

        private void UpdateAnimationState() {

            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            Vector2 inputTarget = isSprinting ? playerLocomotionInput.MovementInput * 1.5f : playerLocomotionInput.MovementInput;
            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetFloat(InputXHash, currentBlendInput.x);
            animator.SetFloat(InputYHash, currentBlendInput.y);
            animator.SetFloat(InputMagnitudeHash, currentBlendInput.magnitude);
        }
    }
}
