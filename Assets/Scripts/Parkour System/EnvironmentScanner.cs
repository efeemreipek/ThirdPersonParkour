using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] private Vector3 forwardRayOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] private float forwardRayLength = 0.8f;
    [SerializeField] private float heightRayLength = 5f;
    [SerializeField] private float ledgeRayLength = 10f;
    [SerializeField] private float ledgeHeightThreshold = 0.75f;
    [SerializeField] private float climbLedgeRayLength = 1.5f;
    [SerializeField] private float standRayLength = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask climbLedgeLayer;
    [SerializeField] private LayerMask groundLayer;

    public ObstacleHitData ObstacleCheck()
    {
        ObstacleHitData hitData = new ObstacleHitData();

        Vector3 forwardOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit, forwardRayLength, obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, hitData.forwardHitFound ? Color.red : Color.white);

        if(hitData.forwardHitFound)
        {
            Vector3 heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit, heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, hitData.heightHitFound ? Color.red : Color.white);
        }

        return hitData;
    }

    public bool ObstacleLedgeCheck(Vector3 moveDirection, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if(moveDirection == Vector3.zero) return false;

        float originOffset = 0.4f;
        Vector3 origin = transform.position + moveDirection * originOffset + Vector3.up;

        if(PhysicsUtils.ThreeRaycast(origin, Vector3.down, 0.25f, transform, out List<RaycastHit> hits, ledgeRayLength, obstacleLayer, true))
        {
            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold).ToList();

            if(validHits.Count > 0)
            {
                Vector3 surfaceRayOrigin = transform.position + moveDirection + Vector3.down * 0.1f;
                surfaceRayOrigin.y = transform.position.y - 0.1f;

                if(Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit surfaceHit, 2f, obstacleLayer))
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.cyan);

                    float height = transform.position.y - validHits[0].point.y;

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }
        }

        return false;
    }

    public bool ClimbLedgeCheck(Vector3 direction, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        if(direction == Vector3.zero) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 offset = Vector3.up * 0.18f;

        for(int i = 0; i < 10; i++)
        {
            Debug.DrawRay(origin + offset * i, direction, Color.white);

            if(Physics.Raycast(origin + offset * i, transform.forward, out RaycastHit hit, climbLedgeRayLength, climbLedgeLayer))
            {
                ledgeHit = hit;
                return true;
            }
        }

        return false;
    }

    public bool DropLedgeCheck(out RaycastHit dropLedgeHit)
    {
        dropLedgeHit = new RaycastHit();

        Vector3 origin = transform.position + Vector3.up * -0.1f + transform.forward * 2f;

        if(Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3f, climbLedgeLayer))
        {
            dropLedgeHit = hit;
            return true;
        }

        return false;
    }

    public bool DropCheck(out RaycastHit dropHit)
    {
        dropHit = new RaycastHit();

        Vector3 origin = transform.position;

        if(Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            dropHit = hit;
            return true;
        }

        return false;
    }

    public bool CanStandCheck(out RaycastHit standHit)
    {
        standHit = new RaycastHit();

        Vector3 origin = transform.position;

        if(Physics.Raycast(origin, Vector3.up, out RaycastHit hit, standRayLength, obstacleLayer))
        {
            Debug.DrawRay(origin, Vector3.up * 1.75f, Color.yellow);

            standHit = hit;
            return false;
        }

        return true;
    }
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}
