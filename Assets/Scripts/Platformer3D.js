#pragma strict

// Creates a section header in the Inspector!
@Header("--- Movement ---")

// Speed when not running.
var walkSpeed : float = 3;

// Speed when holding left shift. 
var runSpeed : float = 5;

// Speed of jumping.
var jumpSpeed : float = 10;

// How quickly player turns when the cam is moved.
var turnSpeed : float = 3;

// How close to the ground the player must be to jump or change direction.
var groundCheckDistance : float = 0.1;

// How much direction can be changed in the air. Range of 0-1.
@Range(0, 1)
var airSteerAmount : float = 1;

// Creates a section header in the Inspector!
@Header("--- Camera ---")

// This player's camera.
// It will be moved forward if line of sight is ever broken.
var cam : GameObject = null;

// The parent of the camera.
// Think of it as the camera's pivot and focal point.
var camPivot : GameObject = null;

// Whether to invert the camera movement direction when the mouse is moved up or down.
var invertMouseY : boolean = true;

// How fast the cam moves, in degrees per second.
var camMoveSpeed : float = 360;

// The lowest the camera can be tilted.
var mincamTilt : float = -75;

// The highest the camera can be tilted.
var maxcamTilt : float = 75;

// The closest the camera can get to a wall before it is moved.
var camColliderRadius : float = 0.2;

// *** Private variables don't show in the inspector. ***

// The current rotation of the camera pivot.
private var camRotation : Vector3 = Vector3.zero;

// The position, relative to the camera pivot, where the cam wants to be.
private var idealLocalCamPosition : Vector3 = Vector3.zero;

// The distance between cam and pivot.
private var idealLocalCamDistance : float = 0;

// Whether the player was running the last time he/she touched the ground.
private var didRun : boolean = false;

// Called once at the beginning.
function Start()
{
    // Gets the initial camera rotation.
    camRotation = camPivot.transform.eulerAngles;

    // Gets the ideal camera position relative to the pivot based on its initial position.
    idealLocalCamPosition = cam.transform.localPosition;

    // Calculates the distance from the cam to the pivot at full extension.
    idealLocalCamDistance = idealLocalCamPosition.magnitude;

    // Configure physics checks to ignore triggers.
    Physics.queriesHitTriggers = false;
}

// Called every frame (about 60 times per second).
function Update()
{
    // *** SIDEWAYS CAMERA ROTATION ***

    // Detects the sideways movement of the mouse.
    var leftRightMouseInput : float = Input.GetAxis("Mouse X");

    // Calculates how far the cam will be moved.
    var leftRightcamMovement : float = leftRightMouseInput * camMoveSpeed * Time.deltaTime;

    // Applies horizontal camera rotation.
    camRotation.y += leftRightcamMovement;


    // *** UP/DOWN CAMERA ROTATION ***

    // Detects the up/down movement of the mouse.
    var upDownMouseInput : float = Input.GetAxis("Mouse Y");

    // If mouse input is set to be inverted...
    if(invertMouseY == true)
    {
        // Flips the movement direction.
        upDownMouseInput *= -1;
    }

    // Calculates how far the camera will be moved.
    var upDowncamMovement : float = upDownMouseInput * camMoveSpeed * Time.deltaTime;

    // Applies vertical camera rotation.
    // This time, ensures that the rotation never goes outside the clamped range.
    camRotation.x = Mathf.Clamp(camRotation.x + upDowncamMovement,	// desired value
							mincamTilt,								// minimum
							maxcamTilt);							// maximum

    // *** All camera rotations are applied. ***
    camPivot.transform.eulerAngles = camRotation;


    // *** CAMERA LINE OF SIGHT CHECKING ***

    // Gets current position of the cam pivot.
    var camPivotPosition : Vector3 = camPivot.transform.position;

    // Gets the direction from the pivot to the cam's ideal position.
    // This takes into account the pivot's rotation.
    var directionFromPivotToCam : Vector3 = camPivot.transform.TransformDirection(idealLocalCamPosition);

    // Creates a place to put the results of the check.
    var camLineOfSightCheckInfo : RaycastHit;

    // Casts an invisible sphere from the cam pivot to the ideal cam position.
    // If anything was hit, returns true and stores details in camLineOfSightCheckInfo.
    var camIsBlocked : boolean = Physics.SphereCast(camPivotPosition,	// start point
		camColliderRadius,			// sphere radius
		directionFromPivotToCam,	// direction to check
		camLineOfSightCheckInfo,	// where results go
		idealLocalCamDistance);		// distance to check

    // If the camera is blocked by something...
    if(camIsBlocked == true)
    {
        // Finds the normal of the point where the camera was blocked.
        var hitNormal : Vector3 = camLineOfSightCheckInfo.normal;

        // Calculates how far the camera will need to be moved to not overlap the collider.
        var offsetToKeepCamOutOfWall : Vector3 = hitNormal * camColliderRadius;

        // Finds the exact position where the camera was blocked.
        var pointWherecamHitWall : Vector3 = camLineOfSightCheckInfo.point;

        // Calculates the exact position where the camera should be to completely avoid blockage.
        var newCamPosition : Vector3 = pointWherecamHitWall + offsetToKeepCamOutOfWall;

        // Applies the modified camera position.
        cam.transform.position = newCamPosition;
    }
        // If the camera is not blocked...
    else
    {
        // Place the camera at its ideal offset from the pivot.
        cam.transform.localPosition = idealLocalCamPosition;
    }


    // *** PLAYER ROTATION TO MATCH CAMERA ***

    // Detect the forward direction of the camera.
    var currentcamForwardDirection : Vector3 = camPivot.transform.forward;

    // Discard vertical direction because we don't care about that.
    currentcamForwardDirection.y = 0;

    // Reset the vector's direction to a length of 1.
    currentcamForwardDirection.Normalize();

    // Detect the direction the player is currently facing.
    var playerForward : Vector3 = this.transform.forward;

    // Calculate the rotation that is needed to make the player look where the camera is facing.
    var rotationNeededSoPlayerFacescamForward : Quaternion = Quaternion.FromToRotation(playerForward,	// current rotation
		currentcamForwardDirection);																	// target rotation

    // Convert the rotation to a vector so we can apply it as angular velocity.
    var rotationAsVector : Vector3 = rotationNeededSoPlayerFacescamForward.eulerAngles;

    // The result of the previous operation is always positive.
    // This may give us the long way around to rotate.
    // So, we subtract 360 from any value over 180 so that we always get the shortest direction to rotate.
    if(rotationAsVector.y > 180)
    {
        rotationAsVector.y -= 360;
    }

    // Applies calculated rotation in the form of angular velocity to the Rigidbody component.
    this.GetComponent(Rigidbody).angularVelocity = rotationAsVector * turnSpeed * Time.deltaTime;


    // *** DETECTING THE GROUND ***

    // Default the strength of the input to the air steer variable.
    var inputStrength : float = airSteerAmount;

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

    // If the player is standing on something solid...
    if(isTouchingGround == true)
    {
        // Allow full input fidelity.
        inputStrength = 1;

        // Update whether the player is holding the run key.
        didRun = Input.GetKey(KeyCode.LeftShift);


        // *** JUMPING ***

        // Detect whether the player pressed the jump key.
        var didJump : boolean = Input.GetButton("Jump");

        // If the player did press the key...
        if(didJump == true)
        {
            // Applies upward velocity.
            this.GetComponent(Rigidbody).velocity.y = jumpSpeed;
        }
    }


    // *** MOVEMENT ***

    // Remember the current velocity from the Rigidbody.
    var previousVelocity : Vector3 = this.GetComponent(Rigidbody).velocity;

    // Create a place to store the new target velocity.
    var targetVelocity : Vector3 = Vector3.zero;

    // Detect left/right keys pressed.
    var leftRightKeyboardInput : float = Input.GetAxis("Horizontal");

    // Detect forward/back keys pressed.
    var forwardBackKeyboardInput : float = Input.GetAxis("Vertical");

    // Combine input into a vector for simplicity.
    var combinedInput : Vector3 = new Vector3(leftRightKeyboardInput, 0, forwardBackKeyboardInput);

    // Converts the input vector so that +Z is in the direction the player is facing.
    var inputRotatedToMatchPlayer : Vector3 = this.transform.TransformDirection(combinedInput);

    // Gets the east/west component of the movement.
    var eastWestMovement : float = inputRotatedToMatchPlayer.x;

    // Applies east/west to targetVelocity.
    targetVelocity.x = eastWestMovement;

    // Gets the north/south component of the movement.
    var northSouthMovement : float = inputRotatedToMatchPlayer.z;

    // Applies north/south to targetVelocity.
    targetVelocity.z = northSouthMovement;

    // If the player was running last time he/she was on the ground...
    if(didRun == true)
    {
        // Multiply targetVelocity by runSpeed.
        targetVelocity*= runSpeed;
    }
        // If the player was not running...
    else
    {
        // Multiply targetVelocity by walkSpeed instead.
        targetVelocity *= walkSpeed;
    }

    // *** NON-VERTICAL MOVEMENT IS APPLIED ***

    // X and Z velocity are applied using inputStrength to dampen if needed.
    // This applies the effects of scalable air steer.

    this.GetComponent(Rigidbody).velocity.x = Mathf.Lerp(previousVelocity.x,	// starting velocity
		targetVelocity.x,	// target velocity
		inputStrength);		// how much to apply (0 = none, 1 = all)

    this.GetComponent(Rigidbody).velocity.z = Mathf.Lerp(previousVelocity.z,	// starting velocity
		targetVelocity.z,	// target velocity
		inputStrength);		// how much to apply (0 = none, 1 = all)
}