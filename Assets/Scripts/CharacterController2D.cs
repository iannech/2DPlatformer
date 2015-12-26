using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {

    public LayerMask collisionMask; //helps us determine which objects we are collide with.

    const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;


	void Start () {

        collider = GetComponent<BoxCollider2D>();
        calculateRaySpacing();
    }

    void Update()
    {
        // test if rayspacing is calculated correctly
        
       

        
    }

    public void Move(Vector3 velocity)
    {
        updateRayCastOrigins();
        collisions.Reset(); // we want a blank slate each time


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
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance; // once we hit something, we want to set the rayLength at that collision point

                // set collisions depending on how we've collided with something
                collisions.left = directionX == -1; // if we hit something and we're going left, then collision is set to left
                collisions.right = directionX == 1;

                
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

                // set collisions depending on how we've collided with something
                collisions.below  = directionY == -1; 
                collisions.above = directionY == 1;
            }
        }
    }

    //
    void updateRayCastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void calculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // calculate ray spacing
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    // To know exactly where our collisions are taking place
    public struct CollisionInfo
    {
        public bool above, below;
        public bool right, left;

        public void Reset()
        {
            above = below = false;
            right = left = false;
        }
    }
	
	
}
