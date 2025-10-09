using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalCharacterController {

    public class PlayerAnimation : MonoBehaviour {

        private static int InputXHash = Animator.StringToHash("inputX");
        private static int InputYHash = Animator.StringToHash("inputY");
        private static int InputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static int IsJumpingHash = Animator.StringToHash("ifJumping");
        private static int IsFallingHash = Animator.StringToHash("isFalling");
        private static int RotationMismatchHash = Animator.StringToHash("rotationMismatch");
        private static int IsIdlingHash = Animator.StringToHash("isIdling");
        private static int IsRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");

        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private Vector3 currentBlendInput = Vector3.zero;
        private PlayerController playerController;

        private float sprintMaxBlendValue = 1.5f;
        private float runMaxBlenValue = 1f;
        private float walkMaxBlendValue = 0.5f;

        private void Awake() {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
            playerController = GetComponent<PlayerController>();
        }

        private void Update() {

            UpdateAnimationState();
        }

        private void UpdateAnimationState() {

            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isRunning = playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isGrounded = playerState.InGroundedState();

            bool isRunBlendValue = isRunning || isJumping || isFalling;

            Vector2 inputTarget = isSprinting ? playerLocomotionInput.MovementInput * sprintMaxBlendValue :
                                  isRunBlendValue ? playerLocomotionInput.MovementInput * runMaxBlenValue : playerLocomotionInput.MovementInput * walkMaxBlendValue;

            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetFloat(InputXHash, currentBlendInput.x);
            animator.SetFloat(InputYHash, currentBlendInput.y);
            animator.SetBool(IsIdlingHash, isIdling);
            animator.SetFloat(InputMagnitudeHash, currentBlendInput.magnitude);
            animator.SetBool(IsGroundedHash, isGrounded);
            animator.SetBool(IsJumpingHash, isJumping);
            animator.SetBool(IsFallingHash, isFalling);
            animator.SetFloat(RotationMismatchHash, playerController.RotationMismatch);
            animator.SetBool(IsRotatingToTargetHash, playerController.IsRotatingToTarget);
        }
    }
}
