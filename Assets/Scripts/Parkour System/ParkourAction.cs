using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animationName;
    [SerializeField] private string obstacleTag;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private bool rotateToObstacle;
    [SerializeField] private float postActionDelay;

    [Header("Target Matching")]
    [SerializeField] private bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] private float matchStartTime;
    [SerializeField] private float matchTargetTime;
    [SerializeField] private Vector3 matchPositionWeight = Vector3.up;

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPosition { get; set; }
    public bool Mirror { get; set; }


    public string AnimationName => animationName;
    public bool RotateToObstacle => rotateToObstacle;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPositionWeight => matchPositionWeight;
    public float PostActionDelay => postActionDelay;


    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform playerTransform)
    {
        if(!string.IsNullOrEmpty(obstacleTag) && !hitData.forwardHit.transform.CompareTag(obstacleTag)) return false;

        float height = hitData.heightHit.point.y - playerTransform.position.y;

        if(height < minHeight || height > maxHeight)
        {
            return false;
        }

        if(rotateToObstacle)
        {
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
        }

        if(enableTargetMatching)
        {
            MatchPosition = hitData.heightHit.point;
        }

        return true;
    }
}
