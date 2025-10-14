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
        [SerializeField] private float walkAcceleration = 0.15f;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runAcceleration = 0.25f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float sprintAcceleration = 0.5f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float inAirAcceleration = 0.15f;
        [SerializeField] private float inAirDrag = 5f;
        [SerializeField] private float drag = 0.1f;
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float jumpSpeed = 1.0f;
        [SerializeField] private float movingThreshold = 0.01f;
        [SerializeField] private float playerRotationThreshold = 75f;
        [SerializeField] private float maxVerticalVelocity = 50f;

        [Header("Animation Settings")]
        [SerializeField] private float playerModelRotationSpeed = 10f;
        [SerializeField] private float rotateToTargetTime = 0.25f;

        [Header("Camera Settings")]
        [SerializeField] private float lookSensH = 0.1f;
        [SerializeField] private float lookSensV = 0.1f;
        [SerializeField] private float lookLimitV = 90f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayers;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;
        private float verticalVelocity = 0f;
        private float rotatingToTargetTimer = 0f;
        private float antiBump;
        private bool isRotatingClockwise = false;
        private bool jumpedLastFrame = false;
        private float stepOffset;
        private PlayerMovementState lastMovementState = PlayerMovementState.Falling;

        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        private void Awake() {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
            antiBump = sprintSpeed;
            stepOffset = characterController.stepOffset;
        }

        private void Update() {

            UpdateMovementState();
            HandleVerticalMovement();
            HandleHorizontalMovement();
        }

        private void UpdateMovementState() {

            lastMovementState = playerState.CurrentPlayerMovementState;

            bool canRun = CanRun();
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;
            bool isMovingHorizontally = IsMovingHorizontally();
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingHorizontally;
            bool isWalking = isMovingHorizontally && (!canRun || playerLocomotionInput.WalkToggledOn);
            bool isGrounded = IsGrounded();

            PlayerMovementState horizontalState = isWalking ? PlayerMovementState.Walking :
                                                  isSprinting ? PlayerMovementState.Sprinting :
                                                  isMovingHorizontally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(horizontalState);

            if((!isGrounded || jumpedLastFrame) && characterController.velocity.y > 0f) {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                jumpedLastFrame = false;
                characterController.stepOffset = 0f;

            } else if((!isGrounded || jumpedLastFrame) && characterController.velocity.y <= 0f) {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                jumpedLastFrame = false;
                characterController.stepOffset = 0f;

            } else {
                characterController.stepOffset = stepOffset;
            }
        }

        private void HandleVerticalMovement() {

            bool isGrounded = playerState.InGroundedState();

            verticalVelocity -= gravity * Time.deltaTime;

            if(isGrounded && verticalVelocity < 0f) {
                verticalVelocity = -antiBump;
            }

            if(playerLocomotionInput.JumpPressed && isGrounded) {
                verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                jumpedLastFrame = true;
            }

            if(playerState.IsStateGroundedState(lastMovementState) && !isGrounded) {
                verticalVelocity += antiBump;
            }

            if(Mathf.Abs(verticalVelocity) > Mathf.Abs(maxVerticalVelocity)) {
                verticalVelocity = -1f * Mathf.Abs(maxVerticalVelocity);
            }
        }

        private void HandleHorizontalMovement() {

            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = playerState.InGroundedState();
            bool isWalking = playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

            float horizontalAcceleration = !isGrounded ? inAirAcceleration :
                                           isWalking ? walkAcceleration :
                                           isSprinting ? sprintAcceleration : runAcceleration;

            float clampHorizontalMagnitude = !isGrounded ? sprintSpeed :
                                             isWalking ? walkSpeed :
                                             isSprinting ? sprintSpeed : runSpeed;

            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;

            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * horizontalAcceleration * Time.deltaTime;
            Vector3 newVelocity = characterController.velocity + movementDelta;

            float dragMagnitude = isGrounded ? drag : inAirDrag;
            Vector3 currentDrag = newVelocity.normalized * dragMagnitude * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > dragMagnitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampHorizontalMagnitude);
            newVelocity.y += verticalVelocity;
            newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

            characterController.Move(newVelocity * Time.deltaTime);
        }

        private Vector3 HandleSteepWalls(Vector3 velocity) {

            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            if(!validAngle && verticalVelocity < 0f) {
                velocity = Vector3.ProjectOnPlane(velocity, normal);
            }

            return velocity;
        }

        private void LateUpdate() {

            UpdateCameraRotation();
        }

        private void UpdateCameraRotation() {

            cameraRotation.x += lookSensH * playerLocomotionInput.LookInput.x;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSensV * playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            playerTargetRotation.x += transform.eulerAngles.x + lookSensH * playerLocomotionInput.LookInput.x;

            bool isidling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = rotatingToTargetTimer > 0;

            //ROTATE if we are not idling
            if(!isidling) {

                RotatePlayerToTarget();
            }
            //If rotating mismatch not within threshold, or rotate to target is active, then ROTATE
            else if(Mathf.Abs(RotationMismatch) > playerRotationThreshold || IsRotatingToTarget) {

                UpdateIdleRotation(playerRotationThreshold);
            }

            playerCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);

            //Get angle between player and camera
            Vector3 cameraForwardProjectedXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, cameraForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, cameraForwardProjectedXZ);
        }

        private void RotatePlayerToTarget() {

            Quaternion targetRotationX = Quaternion.Euler(0f, playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
        }

        private void UpdateIdleRotation(float _rotationThreshold) {

            //Initiate new rotation direction
            if(Mathf.Abs(RotationMismatch) > _rotationThreshold) {

                rotatingToTargetTimer = rotateToTargetTime;
                isRotatingClockwise = RotationMismatch > _rotationThreshold;
            }

            rotatingToTargetTimer -= Time.deltaTime;

            //Rotate Player
            if(isRotatingClockwise && RotationMismatch > 0f || !isRotatingClockwise && RotationMismatch < 0f) {
                RotatePlayerToTarget();
            }
        }

        private bool IsMovingHorizontally() {

            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);

            return horizontalVelocity.magnitude > movingThreshold;
        }

        private bool IsGrounded() {

            bool grounded = playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

            return grounded;
        }

        private bool IsGroundedWhileGrounded() {

            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - characterController.radius, transform.position.z);
            bool grounded = Physics.CheckSphere(spherePosition, characterController.radius, groundLayers, QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne() {

            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            return characterController.isGrounded && validAngle;
        }

        private bool CanRun() {

            // Run only when player is moving forward or diagonally at 45 degrees
            return playerLocomotionInput.MovementInput.y >= Mathf.Abs(playerLocomotionInput.MovementInput.x);
        }
    }
}
