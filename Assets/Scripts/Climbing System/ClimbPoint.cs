using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] private bool mountPoint;
    [SerializeField] private bool dropPoint;
    [SerializeField] private List<Neighbour> neighbours = new List<Neighbour>();

    public bool MountPoint => mountPoint;
    public bool DropPoint => dropPoint;

    private void Awake()
    {
        var twoWayNeighbours = neighbours.Where(n => n.isTwoWay);

        foreach (Neighbour neighbour in twoWayNeighbours)
        {
            neighbour.climbPoint?.CreateConnection(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
        }
    }

    public void CreateConnection(ClimbPoint climbPoint, Vector2 direction, ConnectionType connectionType, bool isTwoWay = true)
    {
        Neighbour neighbour = new Neighbour()
        {
            climbPoint = climbPoint,
            direction = direction,
            connectionType = connectionType,
            isTwoWay = isTwoWay
        };

        neighbours.Add(neighbour);
    }
    public Neighbour GetNeighbour(Vector2 direction)
    {
        Neighbour neighbour = null;

        if(direction.y != 0f)
        {
            neighbour = neighbours.FirstOrDefault(n => n.direction.y == direction.y);
        }

        if(neighbour == null && direction.x != 0f)
        {
            neighbour = neighbours.FirstOrDefault(n => n.direction.x == direction.x);
        }

        return neighbour;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);


        foreach(Neighbour neighbour in neighbours)
        {
            if(neighbour.climbPoint != null)
            {
                Debug.DrawLine(transform.position, neighbour.climbPoint.transform.position, neighbour.isTwoWay ? Color.green : Color.grey);
            }
        }   
    }
}

[System.Serializable]
public class Neighbour
{
    public ClimbPoint climbPoint;
    public Vector2 direction;
    public ConnectionType connectionType;
    public bool isTwoWay = true;
}

public enum ConnectionType
{
    Jump,
    Move
}
