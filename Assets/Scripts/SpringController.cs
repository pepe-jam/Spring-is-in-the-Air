using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpringController : MonoBehaviour
{
    [Header("Spring Shape")]
    public int springCount;
    public float height;
    public float jointScale;
    public Mesh debugMesh;

    [Header("Physical Properties")]
    public float linearDrag;
    public float dampening; // dampening ratio for spring joints
    public float bottomJointMass;
    public float bottomJointGravityScale;
    public float balancingJointMass;
    public float balancingJointGravityScale;
    public float oscillatingFrequency;

    
    [Header("Jumping")]
    public float jumpForceUp;
    public float jumpForceSideways;
    public float jumpChargeTime;
    public float linearDragWhileCharging;
    public float dampeningWhileCharging; // dampening ratio for spring joints
    public float chargingContraction;
    [Tooltip("How far the player leans into the direction they are aiming towards")]
    public float tiltStrength;

    [Header("Left-Right-Movement")]
    public float moveDuration;
    public float moveForceSideways;
    [Tooltip("How far the player will move relative to their height")]
    [Range(0, 1)]
    public float moveDistance;
    public float moveAnimationSpeed;
    
   
    [Header("Ground Check")]
    public LayerMask groundLayers;
    [Tooltip("How long the player has to 'lay' on the ground before they are considered grounded and able to jump (in seconds)")]
    public float groundCheckDuration;
    
    [Header("Rescue Spasm for when the groundcheck fails and the player gets stuck")]
    public float rescueSpasmDelay;
    public float rescueSpasmIntensity;
    public float rescueSpasmDuration;


    private Joint[] _joints;
    private float _jumpCharge = 0;
    private float _lastTimeMoved = 0;
    private float _secondsGrounded = 0;
    private int _topJointIndex;
    private int _bottomJointIndex;
    
    #region Initialisierung

    // Start is called before the first frame update
    void Start()
    {
        _joints = new Joint[springCount];   // initialize the array containing all joints the character has
        // create the joints of the character
        for (int index = 0; index < springCount; index++)
        {
            CreateSpringJointObject(index);
        }

        _topJointIndex = springCount - 1;
        _bottomJointIndex = 0;
    }

    private void CreateSpringJointObject(int index)
    {
        _joints[index] = new Joint
        {
            GameObject = new GameObject("SpringJoint " + index, typeof(MeshFilter), typeof(MeshRenderer),
                typeof(Rigidbody2D), typeof(BoxCollider2D))
        };
        _joints[index].GameObject.layer = LayerMask.NameToLayer("Player");  // Add all joints to a separate layer to make ground collision checks possible
        _joints[index].GameObject.transform.localScale = Vector3.one*jointScale/springCount;
        _joints[index].Rigidbody2D = _joints[index].GameObject.GetComponent<Rigidbody2D>();
        _joints[index].Rigidbody2D.drag = linearDrag;
        _joints[index].Rigidbody2D.angularDrag = 1000000f;
        _joints[index].GameObject.GetComponent<MeshFilter>().mesh = debugMesh;  // just to make the joints visible for debug purposes 
        if (index == 0)
        {
            SetBottomJoint(index);
        }
        else
        {
            _joints[index].SpringJoint2D = _joints[index].GameObject.AddComponent<SpringJoint2D>();
            _joints[index].SpringJoint2D.connectedBody = _joints[index - 1].GameObject.GetComponent<Rigidbody2D>(); // attach this SpringJoint to the joint before it
            _joints[index].SpringJoint2D.dampingRatio = dampening;
            _joints[index].SpringJoint2D.autoConfigureDistance = false;
            _joints[index].SpringJoint2D.distance = height/springCount;
            _joints[index].SpringJoint2D.frequency = oscillatingFrequency;
            _joints[index].SpringJoint2D.enableCollision = false;
            SetBalancingJoint(index);
        }
        _joints[index].GameObject.transform.parent = gameObject.transform;   // macht das aktuelle gameObject zum Elternteil der neu erstellten SpringJoints
    }

    private void SetBottomJoint(int index)
    {
        _joints[index].Rigidbody2D.mass = bottomJointMass;
        _joints[index].Rigidbody2D.gravityScale = bottomJointGravityScale;
        _bottomJointIndex = index;
    }

    private void SetBalancingJoint(int index)
    {
        _joints[index].Rigidbody2D.mass = balancingJointMass/springCount;
        _joints[index].Rigidbody2D.gravityScale = balancingJointGravityScale;
    }
    
    private class Joint
    {
        public GameObject GameObject { get; set; }
        // So we don't have to call GetComponent() every time the spring turns around, we can save a reference to important components it as a property
        public SpringJoint2D SpringJoint2D { get; set; }    
        public Rigidbody2D Rigidbody2D;
    }
    
    # endregion Initialisierung
    
    # region Steuerung

    private float _lastFloorCollisionTime;
    
    // Update is called once per frame
    void Update()
    {
        if (GroundCheck())
        {
            BalancingJointGravity();
            _lastFloorCollisionTime = Time.time;
            
            if (Input.GetKey(KeyCode.Space))
            {
                // makes the spring tilt into the direction of the jump
                _joints[_topJointIndex].Rigidbody2D.AddForce(Vector2.right * (Input.GetAxis("Horizontal") * tiltStrength));
                _jumpCharge = Mathf.Min(_jumpCharge + Time.deltaTime*jumpChargeTime, 1);    // gradually charge a jump while the jump button is held down
                _joints[0].Rigidbody2D.drag = linearDragWhileCharging;
                // makes the spring visibly charge by contracting its joints
                for (int index = 1; index < springCount; index++)
                {
                    _joints[index].SpringJoint2D.distance = Mathf.Lerp(height/springCount, (height*chargingContraction)/springCount, _jumpCharge);
                    _joints[index].SpringJoint2D.dampingRatio = dampeningWhileCharging;
                    _joints[index].Rigidbody2D.drag = linearDragWhileCharging;
                }
            }
            else
            {
                if (_jumpCharge > 0)
                {
                    // Jump high into the air (or not, depending on the value of _jumpCharge)
                    Jump(_jumpCharge, jumpForceSideways);
                    _jumpCharge = 0;
                    ResetPhysicalProperties();
                }
                else if (Input.GetAxis("Horizontal") != 0 && Time.time - _lastTimeMoved > moveDuration )
                {
                    // Bewegung nach links und rechts
                    //Jump(0, moveForceSideways, ForceMode2D.Impulse);
                    StartCoroutine(nameof(Move));
                    _lastTimeMoved = Time.time;
                }
            }
        }
        else // if not on ground
        {
            // If the player leaves the ground while charging for a jump, that charging state must be reversed 
            ResetPhysicalProperties();
            if (Time.time - _lastFloorCollisionTime > rescueSpasmDelay)
            {
               RescueSpasm();
            }
        }
    }

    
    private void ResetPhysicalProperties()
    {
        // instantly discharge the character's spring joints and reset their physics properties back to normal
        _joints[0].Rigidbody2D.drag = linearDrag;
        for (int index = 1; index < springCount; index++)
        {
            _joints[index].SpringJoint2D.distance = height / springCount;
            _joints[index].SpringJoint2D.dampingRatio = dampening;
            _joints[index].Rigidbody2D.drag = linearDrag;
        }
    }

    private void Jump(float jumpCharge, float jumpForceSideways, ForceMode2D forceMode = ForceMode2D.Force)
    {
        var jumpForce = jumpCharge * new Vector2(jumpForceSideways * Input.GetAxis("Horizontal"),
            jumpForceUp);
        _joints[_topJointIndex].Rigidbody2D.mass = bottomJointMass; // quick fix for making the character actually jump instead of spiralling out of control
        _joints[_topJointIndex].Rigidbody2D.AddForce(jumpForce, forceMode);
        TurnUpsideDown();
        BalancingJointGravity(true);
    }

    private void TurnUpsideDown()
    {
        if (_topJointIndex==0)
        {
            _topJointIndex = springCount - 1;
            SetBalancingJoint(springCount-1);
            SetBottomJoint(0);
        }
        else
        {
            _topJointIndex = 0;
            SetBalancingJoint(0);
            SetBottomJoint(springCount-1);
        }
    }

    private void BalancingJointGravity(bool weightless = false)
    {
        for (int index = 0; index < springCount; index++)
        {
            if (index != _bottomJointIndex)
            {
                _joints[index].Rigidbody2D.gravityScale = weightless ? 0 : balancingJointGravityScale;
            }
        }
    }

    private void RescueSpasm()
    {
        // rescue spasm
        for (int index = 1; index < springCount; index++)
        {
            _joints[index].SpringJoint2D.distance = rescueSpasmIntensity / springCount;
            _joints[index].SpringJoint2D.dampingRatio = dampeningWhileCharging;
        }
        // end of rescue spasm
        if (Time.time - _lastFloorCollisionTime > rescueSpasmDelay + rescueSpasmDuration)
        {
            _lastFloorCollisionTime = Time.time;
            ResetPhysicalProperties();
        }
    }

    /**
     * Checks if the player (therefore the bottom joint of the player) is on the ground and ready to jump.
     * In order to avoid the player controller thinking it is on the ground whilst colliding with a wall, for instance,
     * the GroundCheck() only returns true after several consecutive frames of the bottom joint being in contact with "ground".
     * How long exactly is determined by the groundCheckDuration parameter. 
     */
    private bool GroundCheck()
    {
        if (Physics2D.OverlapCircle(_joints[_bottomJointIndex].Rigidbody2D.position, _joints[_bottomJointIndex].GameObject.transform.lossyScale.y + 0.0001f, groundLayers))
        {
            _secondsGrounded += Time.deltaTime;
            if (_secondsGrounded > groundCheckDuration)
            {
                return true;
            }

            return false;
        }

        _secondsGrounded = 0;
        return false;
    }

    /*
     * Gradually moves the top joint next to the bottom joint before switching roles. Velocity is copied from the previous bottom joint to the new one.
     */
    private IEnumerator Move()
    {
        _lastTimeMoved = Time.time;
        // direction will be 1 or -1, depending on whether the player wanted to go left or right
        float direction = Input.GetAxis("Horizontal") / Mathf.Abs(Input.GetAxis("Horizontal"));
        Vector2 positionOffset = new Vector2(direction * moveDistance * height, 0);
        _joints[_bottomJointIndex].Rigidbody2D.gravityScale = bottomJointGravityScale;
        Vector2 bottomJointVelocity = _joints[_bottomJointIndex].Rigidbody2D.velocity;  // copy
        do
        {
            // funtkioniert nicht
            var current_position = _joints[_topJointIndex].Rigidbody2D.position;
            var target_position = _joints[_bottomJointIndex].Rigidbody2D.position + positionOffset;
            //_joints[_topJointIndex].Rigidbody2D.velocity = (current_position - target_position).normalized * moveAnimationSpeed;
            _joints[_topJointIndex].Rigidbody2D.position = Vector2.MoveTowards(current_position, target_position, moveAnimationSpeed);
            yield return null;
        } while (Time.time - _lastTimeMoved > moveDuration);

        _joints[_topJointIndex].Rigidbody2D.velocity = bottomJointVelocity; // paste
        TurnUpsideDown();
        BalancingJointGravity();
    }

    /*
     primitive approach to moving where perpendicular forces are applied to the top and bottom joint whilst the top joint is falling down.
     Causes the player to turn into a woolen ball and get stuck when trying to move back and forth.
    private IEnumerator Move()
    {
        // direction will be 1 or -1, depending on whether the player wanted to go left or right
        float direction = Input.GetAxis("Horizontal") / Mathf.Abs(Input.GetAxis("Horizontal"));
        var moveForce = new Vector2(direction * moveForceSideways, 0);
        _lastTimeMoved = Time.time;
        _joints[_topJointIndex].Rigidbody2D.gravityScale = bottomJointGravityScale;
        _joints[_topJointIndex].Rigidbody2D.mass = bottomJointMass;
        do
        {
            _joints[_topJointIndex].Rigidbody2D.AddForce(moveForce);
            _joints[_bottomJointIndex].Rigidbody2D.AddForce(-moveForce);
            yield return null;
        } while (Time.time - _lastTimeMoved > moveDuration);
        TurnUpsideDown();
        BalancingJointGravity();
    }
    */
    
    
    #endregion Steuerung
}
