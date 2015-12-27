using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour {
    /*
        It's good practice not to assign abstract values to gravity and jumpVelocity. Therefore we'll define two other vars
        jumpHeight and timeToJumpApex which will be translated to the previously defined variables
     */

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;


    float gravity;
    float moveSpeed = 6;
    float jumpVelocity;
    float velocityXSmoothing;

    Vector3 velocity;

    CharacterController2D controller;


	void Start () {
        controller = GetComponent<CharacterController2D>();

        // calculate gravity and jumpVelocity
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print("Gravity" + gravity + "JumpVelocity" + jumpVelocity);
	}
	
	
	void Update () {

        if(controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = jumpVelocity;
        }
        float targetVelocityX = input.x * moveSpeed;

        // smoothen player movement
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
	}
}
