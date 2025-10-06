using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalCharacterController {

    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour {

        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float runAcceleration = 0.25f;
        [SerializeField] private float runSpeed = 4f;
        [SerializeField] private float sprintAcceleration = 0.5f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float drag = 0.1f;
        [SerializeField] private float movingThreshold = 0.01f;

        [Header("Camera Settings")]
        [SerializeField] private float lookSensH = 0.1f;
        [SerializeField] private float lookSensV = 0.1f;
        [SerializeField] private float lookLimitV = 90f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;

        private void Awake() {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
        }

        private void Update() {

            UpdateMovementState();
            HandleMovemene();
        }

        private void UpdateMovementState() {

            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingHorizontally = IsMovingHorizontally();
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingHorizontally;

            PlayerMovementState horizontalState = isSprinting ? PlayerMovementState.Sprinting :
                                                  isMovingHorizontally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(horizontalState);
        }

        private void HandleMovemene() {

            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            float horizontalAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
            float clampHorizontalMagnitude = isSprinting ? sprintSpeed : runSpeed;

            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;

            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * horizontalAcceleration * Time.deltaTime;
            Vector3 newVelocity = characterController.velocity + movementDelta;

            Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampHorizontalMagnitude);

            characterController.Move(newVelocity * Time.deltaTime);
        }

        private void LateUpdate() {

            cameraRotation.x += lookSensH * playerLocomotionInput.LookInput.x;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSensV * playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            playerTargetRotation.x += transform.eulerAngles.x + lookSensH * playerLocomotionInput.LookInput.x;
            transform.rotation = Quaternion.Euler(0f, playerTargetRotation.x, 0f);

            playerCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);
        }

        private bool IsMovingHorizontally() {

            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);

            return horizontalVelocity.magnitude > movingThreshold;
        }
    }
}
