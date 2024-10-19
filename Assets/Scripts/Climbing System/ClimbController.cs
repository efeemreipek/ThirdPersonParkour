using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    private EnvironmentScanner environmentScanner;
    private PlayerController playerController;

    private ClimbPoint currentClimbPoint;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if(!playerController.IsHanging)
        {
            if(InputManager.Instance.JumpInput && !playerController.InAction)
            {
                if(environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentClimbPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", currentClimbPoint.transform, 0.39f, 0.55f, handOffset: new Vector3(0.2f, 0.18f, 0.27f)));
                }
            }

            if(InputManager.Instance.DropInput && !playerController.InAction)
            {
                if(environmentScanner.DropLedgeCheck(out RaycastHit dropLedgeHit))
                {
                    currentClimbPoint = GetNearestClimbPoint(dropLedgeHit.transform, dropLedgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentClimbPoint.transform, 0.3f, 0.45f, handOffset: new Vector3(0.25f, 0.45f, 0.14f)));
                }
            }
        }
        else
        {
            if(InputManager.Instance.DropInput && !playerController.InAction)
            {
                StartCoroutine(JumpFromHang());
                return;
            }

            float h = Mathf.Round(InputManager.Instance.MoveInput.x);
            float v = Mathf.Round(InputManager.Instance.MoveInput.y);

            Vector2 inputDirection = new Vector2(h, v);

            if(playerController.InAction || inputDirection == Vector2.zero) return;

            if(currentClimbPoint.MountPoint && inputDirection.y == 1f)
            {
                StartCoroutine(HangToCrouch());
                return;
            }

            if(currentClimbPoint.DropPoint && inputDirection.y == -1f)
            {
                StartCoroutine(DropFromHang("HangHopDropDown", 0.25f, 0.48f));
                return;
            }
            
            Neighbour neighbour = currentClimbPoint.GetNeighbour(inputDirection);
            if(neighbour == null) return;

            if(neighbour.connectionType == ConnectionType.Jump && InputManager.Instance.JumpInput)
            {
                currentClimbPoint = neighbour.climbPoint;

                if(neighbour.direction.y == 1f)
                {
                    StartCoroutine(JumpToLedge("HangHopUp", currentClimbPoint.transform, 0.34f, 0.65f, handOffset: new Vector3(0.26f, 0.1f, 0.27f)));
                }
                else if(neighbour.direction.y == -1f)
                {
                    StartCoroutine(JumpToLedge("HangHopDown", currentClimbPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(0.26f, 0.12f, 0.25f)));
                }
                else if(neighbour.direction.x == 1f)
                {
                    StartCoroutine(JumpToLedge("HangHopRight", currentClimbPoint.transform, 0.2f, 0.5f, handOffset: new Vector3(0.22f, 0.12f, 0.25f)));
                }
                else if(neighbour.direction.x == -1f)
                {
                    StartCoroutine(JumpToLedge("HangHopLeft", currentClimbPoint.transform, 0.2f, 0.5f, handOffset: new Vector3(0.34f, 0.12f, 0.25f)));
                }
            }
            else if(neighbour.connectionType == ConnectionType.Move)
            {
                currentClimbPoint = neighbour.climbPoint;

                if(neighbour.direction.x == 1f)
                {
                    StartCoroutine(JumpToLedge("HangShimmyRight", currentClimbPoint.transform, 0f, 0.4f, handOffset: new Vector3(0.26f, 0.02f, 0.23f)));
                }
                else if(neighbour.direction.x == -1f)
                {
                    StartCoroutine(JumpToLedge("HangShimmyLeft", currentClimbPoint.transform, 0f, 0.4f, AvatarTarget.LeftHand, new Vector3(0.26f, 0.02f, 0.23f)));
                }
            }
        }
    }

    private IEnumerator JumpToLedge(string animationName, Transform ledge, float matchStartTime, float matchTargetTime,
                                    AvatarTarget avatarTarget = AvatarTarget.RightHand, Vector3? handOffset = null)
    {
        MatchTargetParams matchTargetParams = new MatchTargetParams()
        {
            position = GetHandPosition(ledge, avatarTarget, handOffset),
            bodyPart = avatarTarget,
            positionWeight = Vector3.one,
            startTime = matchStartTime,
            targetTime = matchTargetTime
        };

        Quaternion targetRotation = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(animationName, matchTargetParams, targetRotation, true);

        playerController.IsHanging = true;
    }

    private IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private IEnumerator HangToCrouch()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("HangToCrouch");

        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private IEnumerator DropFromHang(string animationName, float matchStartTime, float matchTargetTime)
    {
        environmentScanner.DropCheck(out RaycastHit dropHit);

        MatchTargetParams matchTargetParams = new MatchTargetParams()
        {
            position = dropHit.point,
            bodyPart = AvatarTarget.RightFoot,
            positionWeight = Vector3.up,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
        };

        playerController.IsHanging = false;
        yield return playerController.DoAction(animationName, matchTargetParams);
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    private Vector3 GetHandPosition(Transform ledge, AvatarTarget avatarTarget, Vector3? handOffset)
    {
        Vector3 offsetValue = handOffset != null ? handOffset.Value : new Vector3(0.26f, 0.12f, 0.23f);

        Vector3 horizontalDirection = avatarTarget == AvatarTarget.RightHand ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - horizontalDirection * offsetValue.x;
    }

    private ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        ClimbPoint[] climbPoints = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestClimbPoint = null;
        float nearestClimbPointDistance = float.MaxValue;

        foreach (ClimbPoint climbPoint in climbPoints)
        {
            float distance = Vector3.Distance(climbPoint.transform.position, hitPoint);

            if(distance < nearestClimbPointDistance)
            {
                nearestClimbPoint = climbPoint;
                nearestClimbPointDistance = distance;
            }
        }

        return nearestClimbPoint;
    }
}
