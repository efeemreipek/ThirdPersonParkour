using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float crouchMultiplier = 0.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private CameraController cameraController;
    private Animator animator;
    private CharacterController characterController;
    private EnvironmentScanner environmentScanner;

    private Quaternion targetRotation;
    private bool isGrounded;
    private float ySpeed;
    private bool hasControl = true;
    private Vector3 desiredMoveDirection;
    private Vector3 moveDirection;
    private Vector3 velocity;
    private float moveSpeed;
    private bool isCrouched;
    private Vector3 crouchedCharacterControllerCenter = new Vector3(0f, 0.7f, 0.05f);
    private Vector3 normalCharacterControllerCenter = new Vector3(0f, 0.9f, 0.05f);
    private bool canStand = true;

    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }
    public bool HasControl { get { return hasControl; } set { hasControl = value; } }
    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }

    public float RotationSpeed => rotationSpeed;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        float h = InputManager.Instance.MoveInput.x;
        float v = InputManager.Instance.MoveInput.y;

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        canStand = environmentScanner.CanStandCheck(out RaycastHit hit);

        if(InputManager.Instance.CrouchInput && canStand)
        {
            isCrouched = !isCrouched;
            animator.SetBool("Is Crouched", isCrouched);

            characterController.height = isCrouched ? 1.4f : 1.8f;
            characterController.center = isCrouched ? crouchedCharacterControllerCenter : normalCharacterControllerCenter;
        }

        Vector3 moveInput = new Vector3(h, 0f, v);
        desiredMoveDirection = cameraController.PlanarRotation * moveInput;
        moveDirection = desiredMoveDirection;

        if(!hasControl) return;
        if(IsHanging) return;

        velocity = Vector3.zero;

        GroundCheck();
        animator.SetBool("Is Grounded", isGrounded);

        if(!isGrounded)
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
            ySpeed = Mathf.Clamp(ySpeed, -20f, -2f);

            velocity = transform.forward * walkSpeed / 2f;
        }
        else
        {
            ySpeed = -2f;
            moveSpeed = (InputManager.Instance.SprintInput && !isCrouched ? walkSpeed * sprintMultiplier : (isCrouched ? walkSpeed * crouchMultiplier : walkSpeed));
            velocity = desiredMoveDirection * moveSpeed;

            IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDirection, out LedgeData ledgeData);
            if(IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            float normalizedSpeed = InputManager.Instance.SprintInput && !isCrouched ? 1.5f : 1f;
            animator.SetFloat("Move Amount", (velocity.magnitude / moveSpeed) * normalizedSpeed, 0.1f, Time.deltaTime);
        }

        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if(moveAmount > 0f && moveDirection.magnitude > 0.2f)
        {
            targetRotation = Quaternion.LookRotation(moveDirection);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    private void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDirection, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if(Vector3.Angle(desiredMoveDirection, transform.forward) >= 80)
        {
            velocity = Vector3.zero;
            return;
        }

        if(angle < 60)
        {
            velocity = Vector3.zero;
            moveDirection = Vector3.zero;
        }
        else if(angle < 90)
        {
            Vector3 left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            Vector3 direction = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * direction;
            moveDirection = direction;
        }
    }

    public IEnumerator DoAction(string animationName, MatchTargetParams matchTargetParams = null, Quaternion targetRotation = new(),
                                bool rotate = false, float postActionDelay = 0f, bool mirror = false)
    {
        InAction = true;

        animator.SetBool("Mirror Action", mirror);
        animator.CrossFadeInFixedTime(animationName, 0.2f);

        yield return null;

        AnimatorStateInfo animatorStateInfo = animator.GetNextAnimatorStateInfo(0);
        if(!animatorStateInfo.IsName(animationName))
        {
            Debug.LogError("Parkour animation is wrong");
        }

        float rotateStartTime = matchTargetParams != null ? matchTargetParams.startTime : 0f;

        float timer = 0f;
        while(timer <= animatorStateInfo.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animatorStateInfo.length;

            if(rotate && normalizedTime > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            if(matchTargetParams != null)
            {
                MatchTarget(matchTargetParams);
            }
            if(animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(postActionDelay);

        InAction = false;
    }

    private void MatchTarget(MatchTargetParams matchTargetParams)
    {
        if(animator.IsInTransition(0)) return;
        if(animator.isMatchingTarget) return;

        animator.MatchTarget(matchTargetParams.position,
                             transform.rotation,
                             matchTargetParams.bodyPart,
                             new MatchTargetWeightMask(matchTargetParams.positionWeight, 0f),
                             matchTargetParams.startTime,
                             matchTargetParams.targetTime);
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if(!hasControl)
        {
            animator.SetFloat("Move Amount", 0f);
            targetRotation = transform.rotation;
        }
    }

    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }

    public void EnableCharacterController(bool enabled)
    {
        characterController.enabled = enabled;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}

public class MatchTargetParams
{
    public Vector3 position;
    public AvatarTarget bodyPart;
    public Vector3 positionWeight;
    public float startTime;
    public float targetTime;
}
