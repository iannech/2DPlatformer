using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {

    BoxCollider2D collider;
	void Start () {

        collider = GetComponent<BoxCollider2D>();
	}
	
	
	void Update () {
	
	}
}
