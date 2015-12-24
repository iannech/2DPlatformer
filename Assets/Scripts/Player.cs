using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour {

    public float gravity = -20f;
    public float moveSpeed = 6;

    Vector3 velocity;

    CharacterController2D controller;


	void Start () {
        controller = GetComponent<CharacterController2D>();

	}
	
	
	void Update () {

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        velocity.x = input.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
	}
}
