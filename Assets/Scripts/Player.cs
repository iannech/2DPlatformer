using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController2D))]
public class Player : MonoBehaviour {

    CharacterController2D controller;

	void Start () {
        controller = GetComponent<CharacterController2D>();

	}
	
	
	void Update () {
	
	}
}
