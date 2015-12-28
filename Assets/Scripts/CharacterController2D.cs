using UnityEngine;
using System.Collections;


public class CharacterController2D :RayCastController {

    float maxClimbSlopeAngle = 80;
    float maxDescendingAngle = 80;

    public CollisionInfo collisions;

    public override void Start()
    {
        base.Start();
    }

    public void Move(Vector3 velocity)
    {
        updateRayCastOrigins();
        collisions.Reset(); // we want a blank slate each time
        collisions.velocityOld = velocity;

        if(velocity.y < 0)
        {
            descendSlope(ref velocity);

        }
        if(velocity.x !=0)
        {
            HorizontalCollisions(ref velocity);
        }
        if(velocity.y != 0)
        {
            verticalCollisions(ref velocity);
        }
       
        transform.Translate(velocity);
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        // get direction of y velocity
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;


        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;

            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                // get angle between player and slope. Use bottom-most ray to calculate.
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(i == 0 && slopeAngle <= maxClimbSlopeAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                 
                    }
                    // to ensure that our player starts climbing a slope right at the base of the slope
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    climbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (! collisions.climbingSlope || slopeAngle >maxClimbSlopeAngle )
                {
                    
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance; // Once we hit something, we want to set the rayLength at that collision point

                    /* Eliminate jittery-sideways collisons with obstacles on a slope.
                       WHy this happens? 
                       velocity.x is reduced while velocity.y remains the same.
                       Solution:
                       recalculate velocity.y
                    */
                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    // set collisions depending on how we've collided with something
                    collisions.left = directionX == -1; // if we hit something and we're going left, then collision is set to left
                    collisions.right = directionX == 1;
                }
          
            } 
        }
    }
    //
    void verticalCollisions(ref Vector3 velocity)
    {
        // get direction of y velocity
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin,Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance; // once we hit something, we want to set the rayLength at that collision point

                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x); 
                }
                // Set collisions depending on how we've collided with something
                collisions.below  = directionY == -1; 
                collisions.above = directionY == 1;
            }    
        }

        // Sometimes the player may seem to stick at an intersection of 2 slopes for a moment.
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }


            }
        }
    }

    void climbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        { 
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
        
    }

    void descendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle !=0 && slopeAngle <= maxDescendingAngle)
            {
                if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                {
                    float moveDistance = Mathf.Abs(velocity.x);
                    float descendingVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                    velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                    velocity.y -= descendingVelocityY;

                    collisions.slopeAngle = slopeAngle;
                    collisions.descendingSlope = true;
                    collisions.below = true;
                }
            }
        }
    }

   

    // To know exactly where our collisions are taking place
    public struct CollisionInfo
    {
        public bool above, below;
        public bool right, left;
        public bool climbingSlope;

        public Vector3 velocityOld;

        public float slopeAngle, slopeAngleOld;
        public bool descendingSlope;

        public void Reset()
        {
            above = below = false;
            right = left = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
	
	
}
