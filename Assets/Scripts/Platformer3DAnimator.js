#pragma strict

// Minimum distance to ground used to determine whether to play jump/fall animation.
var groundCheckDistance : float = 0.6;

// Called every frame (about 60 times per second).
function Update()
{
	// Detect how the player is moving.
	var motion : Vector3 = this.GetComponent(Rigidbody).velocity;

	// Convert motion to be relative to the player's rotation.
	var motionRelativeToPlayerRotation : Vector3 = this.GetComponent(Transform).InverseTransformDirection(motion);

	// Notify animator how the player is moving.
	this.GetComponentInChildren(Animator).SetFloat("ForwardBack", motionRelativeToPlayerRotation.z);
	this.GetComponentInChildren(Animator).SetFloat("LeftRight", this.GetComponent(Rigidbody).angularVelocity.y);

	// Detect the center of the player's capsule collider.
	var colliderCenter : Vector3 = this.GetComponent(CapsuleCollider).center;

	// Detect the height of the collider.
	var colliderHeight : float = this.GetComponent(CapsuleCollider).height;

	// Detect the radius of the rounded ends of the player's capsule collider.
	var colliderRadius : float = this.GetComponent(CapsuleCollider).radius;

	// Calculate a position just below the bottom of the player's capsule collider.
	var groundCheckSphereCenter : Vector3 = this.transform.position + colliderCenter + Vector3.down * (colliderHeight / 2 - colliderRadius + groundCheckDistance);

	// Creates an invisible sphere just below the player's feet.
	// If the sphere overlaps anything other than the player, returns true.
	var isTouchingGround : boolean = Physics.CheckSphere(groundCheckSphereCenter,	// sphere center
		colliderRadius - groundCheckDistance);										// sphere radius

	// Notify the animator whether the player is touching the ground.
	this.GetComponentInChildren(Animator).SetBool("TouchingGround", isTouchingGround);
}