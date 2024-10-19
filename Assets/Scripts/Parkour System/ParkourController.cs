using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] private List<ParkourAction> parkourActionList = new List<ParkourAction>();
    [SerializeField] private ParkourAction jumpDownParkourAction;
    [SerializeField] private float autoDropHeightLimit = 1f;

    private EnvironmentScanner environmentScanner;
    private Animator animator;
    private PlayerController playerController;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        ObstacleHitData hitData = environmentScanner.ObstacleCheck();

        if(InputManager.Instance.JumpInput && !playerController.InAction && !playerController.IsHanging)
        {
            if(hitData.forwardHitFound)
            {
                foreach(ParkourAction action in parkourActionList)
                {
                    if(action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
            }
        }

        if(playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            bool shouldJump = true;
            if(playerController.LedgeData.height > autoDropHeightLimit && !InputManager.Instance.JumpInput)
            {
                shouldJump = false;
            }

            if(shouldJump && playerController.LedgeData.angle <= 50f)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownParkourAction));
            }
        }
    }

    private IEnumerator DoParkourAction(ParkourAction parkourAction)
    {
        playerController.SetControl(false);

        MatchTargetParams matchTargetParams = null;
        if(parkourAction.EnableTargetMatching)
        {
            matchTargetParams = new MatchTargetParams()
            {
                position = parkourAction.MatchPosition,
                bodyPart = parkourAction.MatchBodyPart,
                positionWeight = parkourAction.MatchPositionWeight,
                startTime = parkourAction.MatchStartTime,
                targetTime = parkourAction.MatchTargetTime
            };
        }
        yield return playerController.DoAction(parkourAction.AnimationName, matchTargetParams, parkourAction.TargetRotation, 
                                                 parkourAction.RotateToObstacle, parkourAction.PostActionDelay, parkourAction.Mirror);

        playerController.SetControl(true);
    }

    private void MatchTarget(ParkourAction parkourAction)
    {
        if(animator.IsInTransition(0)) return;
        if(animator.isMatchingTarget) return;

        animator.MatchTarget(parkourAction.MatchPosition,
                             transform.rotation,
                             parkourAction.MatchBodyPart,
                             new MatchTargetWeightMask(parkourAction.MatchPositionWeight, 0f),
                             parkourAction.MatchStartTime,
                             parkourAction.MatchTargetTime);
    }
}
