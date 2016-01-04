using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RayCastController {
    public LayerMask passengerMask;
   

    public Vector3[] localWayPoints;
    Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range (0,2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints; // this percentage is between 0 - 1
    float nextMoveTime;

    //list to store passenger movement info
    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, CharacterController2D> passengerDictionary = new Dictionary<Transform, CharacterController2D>();


    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWayPoints.Length];
        for(int i=0; i < localWayPoints.Length; i++)
        {
            globalWaypoints[i] = localWayPoints[i] + transform.position;
        }
    }

    
    void Update () {
        updateRayCastOrigins();
        Vector3 velocity =CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);

        movePassengers(true);
        transform.Translate(velocity);
        movePassengers(false);
	}

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement()
    {
        if(Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length; // reset globalWaypoints each time it reaches end of array                                         
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentWaypoints);

        if(percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;
            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
            
        }

        return newPos - transform.position;
    }
    void movePassengers(bool beforeMovePlatform)
    {
        foreach(PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<CharacterController2D>());
            }
            if(passenger.moveBeforePlatform == beforeMovePlatform)
            {
                // this is not Optimal, since its calling GetComponent for each passengerAdd. So to avoid this, we use a Dictionary 
                // passenger.transform.GetComponent<CharacterController2D>().Move(passenger.velocity, passenger.standingOnPlatform);
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);

            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity)
    {
        /**
            Store all the passengers moved in a single frame in a HashSet
            HashSet is fast in storing things and checking whether they contain certain things
        **/
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if(velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

              
                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        // Each passenger will only be moved 1 time per frame
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        // add each passenger movement to the passenger list
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength,passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        // add each passenger movement to the passenger list
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),  false, true));

                    }
                }
            }
        }

        // If passenger is on top on Horizontally or Vertically moving Platform
        if(directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;
            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin =  raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        // add each passenger movement to the passenger list
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }

        }
    }

    // struct to hold all movement info
    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos()
    {
        if(localWayPoints != null)
        {
            Gizmos.color = Color.blue;
            float size = .3f;

            for(int i = 0; i < localWayPoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying)? globalWaypoints[i]: localWayPoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
