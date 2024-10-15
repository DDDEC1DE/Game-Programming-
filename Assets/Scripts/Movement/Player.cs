using UnityEngine;

public class Player : MonoBehaviour
{
    public float InAirMoveAccel = 30.0f;
    public float InAirMaxHorizSpeed = 20.0f;
    public float InAirMaxVertSpeed = 50.0f;

    public float OnGroundMoveAccel = 10.0f;
    public float OnGroundMaxSpeed = 10.0f;
    public float OnGroundStopEaseSpeed = 10.0f;

    public bool InstantStepUp = false;
    public float StepUpEaseSpeed = 10.0f;
    public float MinAllowedSurfaceAngle = 15.0f;

    public float GravityAccel = -10.0f;
    public float JumpSpeed = 10.0f;
    public float MaxJumpHoldTime = 0.5f;

    public float GroundCheckStartOffsetY = 0.5f;
    public float CheckForGroundRadius = 0.5f;
    public float GroundResolutionOverlap = 0.05f;

    public float MaxMidAirJumpTime = 0.3f;

    public float JumpPushOutOfGroundAmount = 0.5f;

    public GameObject FootLocationObj;

    public float VectorVisualizeScale = 2.0f;

    public float JiggleFrequency = 0.0f;
    public float MaxJiggleOffset = 3.0f;
    //Properties  very similar to getters and setters in C++ lingvo
	//Allow us to obtain acces to private variable
	public PlayerController Controller { get; set; }
	public Vector3 GroundVelocity { get; private set; }
	public Vector3 GroundAngularVelocity { get; private set; }
	public Vector3 GroundNormal { get; private set; }
    void Start ()
    {
        if(!SetupHumanPlayer())
        {
            return;
        }
        
        //Initialing miscellaneous values
        m_GroundCheckMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        m_RigidBody = GetComponent<Rigidbody>();

        m_Velocity = Vector3.zero;

        m_AllowJump = true;
    }
   
    // Update is called once per frame
    public void Update()
    {
     	UpdateJiggles();
    }

    void FixedUpdate()
    {
        //Update velocity from physics system
        m_Velocity = m_RigidBody.velocity;

        //Update ground info
        UpdateGroundInfo();      

        //Get input
        Controller.UpdateControls();

        Vector3 localMoveDir = Controller.GetMoveInput();
      
        localMoveDir.Normalize();

        bool isJumping = Controller.IsJumping();

        // Update movement
        switch (m_MovementState)
        {
            case MovementState.OnGround:
                UpdateOnGround(localMoveDir, isJumping);
                break;
            case MovementState.InAir:
                UpdateInAir(localMoveDir, isJumping);
                break;

            case MovementState.Disable:
                break;

            default:
                DebugUtils.LogError("Invalid movement state: {0}", m_MovementState);
                break;
        }
    }

    public void UpdateStopping(float stopEaseSpeed)
    {
        //Ease down to the ground velocity to stop relative to the ground
        m_Velocity = MathUtils.LerpTo(stopEaseSpeed, m_Velocity, GroundVelocity, Time.fixedDeltaTime);
    }

    void UpdateGroundInfo()
    {
        //Clear ground info.  Doing this here can simplify the code a bit since we deal with cases where the
        //ground isn't found more easily
        GroundAngularVelocity = Vector3.zero;
        GroundVelocity = Vector3.zero;
        GroundNormal.Set(0.0f, 0.0f, 1.0f);

        //Check for the ground below the player
        m_CenterHeight = transform.position.y;

        float footHeight = FootLocationObj.transform.position.y;

        float halfCapsuleHeight = m_CenterHeight - footHeight;

        Vector3 rayStart = transform.position;
        rayStart.y += GroundCheckStartOffsetY;
        
        Vector3 rayDir = Vector3.down;

        float rayDist = halfCapsuleHeight + GroundCheckStartOffsetY - CheckForGroundRadius;

        //Find all of the surfaces overlapping the sphere cast
        RaycastHit[] hitInfos = Physics.SphereCastAll(rayStart, CheckForGroundRadius, rayDir, rayDist, m_GroundCheckMask);

        //Get the closest surface that is acceptable to walk on.  The order of the 
        RaycastHit groundHitInfo = new RaycastHit();
        bool validGroundFound = false;
        float minGroundDist = float.MaxValue;

        foreach (RaycastHit hitInfo in hitInfos)
        {
            //Check the surface angle to see if it's acceptable to walk on.  
            //Also checking if the distance is zero I ran into a case where the sphere cast was hitting a wall and
            //returning weird resuls in the hit info.  Checking if the distance is greater than 0 eliminates this 
            //case. 
            float surfaceAngle = MathUtils.CalcVerticalAngle(hitInfo.normal);
            if (surfaceAngle < MinAllowedSurfaceAngle || hitInfo.distance <= 0.0f)
            {
                continue;
            }

            if (hitInfo.distance < minGroundDist)
            {
                minGroundDist = hitInfo.distance;

                groundHitInfo = hitInfo;

                validGroundFound = true;
            }
        }

         if(!validGroundFound)
         {
             if (m_MovementState != MovementState.Disable)
             {
                 SetMovementState(MovementState.InAir);
             }
             return;
         }
         
        //Step up
         Vector3 bottomAtHitPoint = MathUtils.ProjectToBottomOfCapsule(
           groundHitInfo.point,
           transform.position,
           halfCapsuleHeight * 2.0f,
           CheckForGroundRadius
           );
         
         float stepUpAmount = groundHitInfo.point.y - bottomAtHitPoint.y;
         m_CenterHeight += stepUpAmount - GroundResolutionOverlap;
         
         //Setting Ground Normal based on object normal we cought trough rayCast.
         GroundNormal = groundHitInfo.normal;
         
        //Set the movement state to be on ground
         if(m_MovementState != MovementState.Disable)
         {
             SetMovementState(MovementState.OnGround);
         }
    }

    void UpdateOnGround(Vector3 localMoveDir, bool isJumping)
    {
        //if movement is close to zero just stop
        if (localMoveDir.sqrMagnitude > MathUtils.CompareEpsilon)
        {
           
            //Since the movement calculations are easier to do with out taking the ground velocity into account
            //we are calculating the velocity relative to the ground
            Vector3 localVelocity = m_Velocity - GroundVelocity;

            //The world movement accelration
            Vector3 moveAccel = CalcMoveAccel(localMoveDir);
         	Debug.DrawLine(transform.position, transform.position + moveAccel * VectorVisualizeScale, Color.green);
            //Adjust acceleration to follow slope
            Vector3 groundTangent = moveAccel - Vector3.Project(moveAccel, GroundNormal);
            groundTangent.Normalize();
            moveAccel = groundTangent;
          
            //The velocity along the movement direction
            Vector3 velAlongMoveDir =  Vector3.Project(localVelocity, moveAccel);
           
            //If we are changing direction, come to a stop first.  This makes the movement more responsive
            //since the stopping happens a lot faster than the acceleration typically allows
            if (Vector3.Dot(velAlongMoveDir, moveAccel) > 0.0f) // if dot product is positive than we are going forward
            {
                //Use a similar method to stopping to ease the movement to just be in the desired move direction
                //This makes turning more responsive
                localVelocity = MathUtils.LerpTo(OnGroundStopEaseSpeed, localVelocity, velAlongMoveDir, Time.fixedDeltaTime);
            }
            else //slow down when direction is changed
            {
                localVelocity = MathUtils.LerpTo(OnGroundStopEaseSpeed, localVelocity, Vector3.zero, Time.fixedDeltaTime);
            }

            //Apply acceleration to velocity
            moveAccel *= OnGroundMoveAccel;

            localVelocity += moveAccel * Time.fixedDeltaTime;

            localVelocity = Vector3.ClampMagnitude(localVelocity, OnGroundMaxSpeed);

            //Update the world velocity
            m_Velocity = localVelocity + GroundVelocity;
        }
        else 
        {
            UpdateStopping(OnGroundStopEaseSpeed);
        }

        //Handle jump input
        if (isJumping)
        {
            ActivateJump();
        }
        else
        {
            m_AllowJump = true;
        }

        //Move the character controller
        ApplyVelocity(m_Velocity);

        //Ease the height up to the step up height
        Vector3 playerCenter = transform.position;

        if (InstantStepUp) //adjust capsule vertical position instantly, depending on raycast returned surface
        {
            playerCenter.y = m_CenterHeight;
        }
        else
        {
            playerCenter.y = MathUtils.LerpTo(StepUpEaseSpeed, playerCenter.y, m_CenterHeight, Time.deltaTime);
        }

        transform.position = playerCenter;

        //Reset time in air
        m_TimeInAir = 0.0f;
    }

    void UpdateInAir(Vector3 localMoveDir, bool isJumping)
    {
        //Check if move direction is large enough before applying acceleration
        if (localMoveDir.sqrMagnitude > MathUtils.CompareEpsilon)// if we are curently moving
        {
            //The world movement accelration
            Vector3 moveAccel = CalcMoveAccel(localMoveDir);

            moveAccel *= InAirMoveAccel;

            m_Velocity += moveAccel * Time.fixedDeltaTime;

            //Clamp velocity
            m_Velocity = MathUtils.HorizontalClamp(m_Velocity, InAirMaxHorizSpeed);

            m_Velocity.y = Mathf.Clamp(m_Velocity.y, -InAirMaxVertSpeed, InAirMaxVertSpeed);
        }

        //Update mid air jump timer and related jump.  This timer is to make jump timing a little more forgiving 
        //by letting you still jump a short time after falling off a ledge.
        if (m_JumpTimeLeft <= 0.0f)
        {
            if (m_TimeLeftToAllowMidAirJump > 0.0f)
            {
                m_TimeLeftToAllowMidAirJump -= Time.fixedDeltaTime;

                if (isJumping)
                {
                    ActivateJump();
                }
                else
                {
                    m_AllowJump = true;
                }
            }
        }
        else
        {
            m_TimeLeftToAllowMidAirJump = 0.0f;
        }

        //Update gravity and jump height control
        if (m_JumpTimeLeft > 0.0f && isJumping)
        {
            m_JumpTimeLeft -= Time.fixedDeltaTime;
        }
        else
        {
            m_Velocity.y += GravityAccel * Time.fixedDeltaTime;

            m_JumpTimeLeft = 0.0f;
        }

        //Move the character controller
        ApplyVelocity(m_Velocity);

        //Increment time in air
        m_TimeInAir += Time.deltaTime;
    }

    void ApplyVelocity(Vector3 velocity)
    {
        Vector3 velocityDiff = velocity - m_RigidBody.velocity;

        m_RigidBody.AddForce(velocityDiff, ForceMode.VelocityChange);
    }

    void ActivateJump()
    {
        //The allowJump bool is to prevent the player from holding down the jump button to bounce up and down
        //Instead they will have to release the button first.
        if (m_AllowJump)
        {
            //Set the vertical speed to be the jump speed + the ground velocity
            m_Velocity.y = JumpSpeed + GroundVelocity.y;

            //This is to ensure that the player wont still be touching the ground after the jump
            transform.position += new Vector3(0.0f, JumpPushOutOfGroundAmount, 0.0f);

            //Set the jump timer
            m_JumpTimeLeft = MaxJumpHoldTime;

            m_AllowJump = false;

        }
    }

    Vector3 CalcMoveAccel(Vector3 localMoveDir)
    {
        Vector3 controlRotation = Controller.GetControlRotation();
        controlRotation.x = 0;

        Vector3 moveAccel = Quaternion.Euler(controlRotation) * localMoveDir;

		return moveAccel;
    }

    void SetMovementState(MovementState movementState)
    {
        switch (movementState)
        {
            case MovementState.OnGround:
                m_TimeLeftToAllowMidAirJump = MaxMidAirJumpTime;
                break;

            case MovementState.InAir:
                break;

            case MovementState.Disable:
                m_Velocity = Vector3.zero;
                ApplyVelocity(m_Velocity);
                break;

            default:
                DebugUtils.LogError("Invalid movement state: {0}", movementState);
                break;
        }
        
        m_MovementState = movementState;
    }

	void UpdateJiggles()
	{
		if (JiggleFrequency <= 0.0f)
		{
			return;
		}

		//Update timer
		m_TimeTillNextJiggle -= Time.deltaTime;

		if (m_TimeTillNextJiggle <= 0.0f)
		{
			m_TimeTillNextJiggle = 1.0f / JiggleFrequency;

			//Approximate normal distribution
			float minRange = -1.0f;
			float maxRange = 1.0f;
			float offsetAmount = UnityEngine.Random.Range(minRange, maxRange);
			offsetAmount += UnityEngine.Random.Range(minRange, maxRange);
			offsetAmount += UnityEngine.Random.Range(minRange, maxRange);
			offsetAmount += UnityEngine.Random.Range(minRange, maxRange);
			offsetAmount /= 4.0f;

			offsetAmount *= MaxJiggleOffset;

			//Offset the player position
			Vector3 offset = UnityEngine.Random.onUnitSphere * offsetAmount;
			offset.y = Mathf.Abs(offset.y);

			transform.position += offset;
		}
	}

	//This function is called when the script is loaded or a value is changed in the inspector.
	//Note that this will only called in the editor.
	void OnValidate()
	{
		m_TimeTillNextJiggle = 0.0f;
	}
	
	bool SetupHumanPlayer()
	{
		if (LevelManager.Instance.GetPlayer() == null)
		{
			//When new level is loaded all objects are getting destroyed.
			//Setting a flag which prevents object distruction.
			DontDestroyOnLoad(gameObject);
			
			// Saving our player handle
			LevelManager.Instance.RegisterPlayer(this);
			
			// Assigning Controlls
			Controller = new MouseKeyPlayerController();
            Controller.Init(this);
            return true;
		}
		else 
		{
			//Do not allow creation of second player object.
			Destroy(gameObject);
			return false;
		}
	}

    enum MovementState
    {
        OnGround,
        InAir,
        Disable
    }

    MovementState m_MovementState;

    Rigidbody m_RigidBody;

    Vector3 m_Velocity;
    float m_CenterHeight;

    int m_GroundCheckMask;

    float m_JumpTimeLeft;
    bool m_AllowJump;

    float m_TimeLeftToAllowMidAirJump;

    float m_TimeTillNextJiggle;



    float m_TimeInAir;

}
