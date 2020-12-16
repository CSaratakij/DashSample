using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    enum MoveType
    {
        Normal,
        Dash
    }

    [Header("Normal")]
    [SerializeField]
    float moveSpeed = 10.0f;

    [SerializeField]
    float gravity = -9.8f;

    [Header("Dash")]
    [SerializeField]
    float dashRange = 30.0f;

    [SerializeField]
    float dashDuration = 1.0f;

    [SerializeField]
    float dashCooldown = 0.25f;

    float dashSpeed;
    float dashTime;
    float allowDashTime;

    Vector2 inputVector;

    Vector3 velocity;
    Vector3 dashDirection;
    Vector3 lookDirection;

    MoveType moveType;
    CharacterController characterController;

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        InputHandler();
        MoveHandler();
    }

    void FixedUpdate()
    {
        DashCanceler();
    }

    void LateUpdate()
    {
        RotateHandler();
    }

    void Initialize()
    {
        characterController = GetComponent<CharacterController>();
        lookDirection = transform.forward;
    }

    void InputHandler()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");

        inputVector = Vector3.ClampMagnitude(inputVector, 1.0f);

        var shouldDash = Input.GetKeyDown(KeyCode.LeftShift) && (Time.time > allowDashTime);
        var shouldUpdateLookDirection = (MoveType.Normal == moveType) && inputVector.sqrMagnitude > 0.0f;

        if (shouldUpdateLookDirection) {
            lookDirection = new Vector3(inputVector.x, 0.0f, inputVector.y);
        }

        if (shouldDash) {
            moveType = MoveType.Dash;

            dashTime = (Time.time + dashDuration);
            allowDashTime = (dashTime + dashCooldown);

            dashDirection = (inputVector == Vector2.zero) ? lookDirection : new Vector3(inputVector.x, 0.0f, inputVector.y);
            lookDirection = dashDirection;
        }
    }

    void MoveHandler()
    {
        switch (moveType)
        {
            case MoveType.Normal:
                NormalMoveHandler();
            break;

            case MoveType.Dash:
            {
                if (Time.time >= dashTime) {
                    moveType = MoveType.Normal;
                }

                DashHandler();
            }
            break;
        }
    }

    void NormalMoveHandler()
    {
        if (characterController.isGrounded) {
            velocity = new Vector3(inputVector.x, 0.0f, inputVector.y) * moveSpeed;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void DashHandler()
    {
        if (characterController.isGrounded) {
            velocity.y = 0.0f;
        }

        dashSpeed = (dashRange / dashDuration);
        velocity = dashDirection * dashSpeed;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void RotateHandler()
    {
        var lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.2f);
    }

    void DashCanceler()
    {
        var shouldCancelDash = (MoveType.Dash == moveType) && Physics.Raycast(transform.position, transform.forward, 1.0f);

        if (shouldCancelDash) {
            moveType = MoveType.Normal;
            dashTime = Time.time;
            allowDashTime = (dashTime + dashCooldown);
        }
    }
}

