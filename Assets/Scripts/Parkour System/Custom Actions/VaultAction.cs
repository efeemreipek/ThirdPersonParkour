using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom Actions/New Vault Action")]
public class VaultAction : ParkourAction
{
    public override bool CheckIfPossible(ObstacleHitData hitData, Transform playerTransform)
    {
        if(!base.CheckIfPossible(hitData, playerTransform))
        {
            return false;
        }

        Vector3 hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        if((hitPoint.z < 0 && hitPoint.x < 0) || (hitPoint.z > 0 && hitPoint.x > 0))
        {
            // Mirror animation
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            // Dont mirror animation
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }

        return true;
    }
}
