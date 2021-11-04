using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpringController : MonoBehaviour
{
    public float jumpForceUp;
    public float jumpForceSideways;
    public float jumpChargeTime;
    public float tiltStrength;
    public float moveHopDelay;
    public float moveForceSideways;
    public int springCount;
    public float jointScale;
    public float linearDrag;
    public float linearDragWhileCharging;
    public float dampening; // dampening ratio for spring joints
    public float dampeningWhileCharging; // dampening ratio for spring joints
    public float bottomJointMass;
    public float bottomJointGravityScale;

    public float balancingJointMass;
    public float balancingJointGravityScale;

    public float relaxedDistance;
    public float chargedDistance;
    public float oscillatingFrequency;


    public LayerMask groundLayers;
    [Tooltip("How long the player has to 'lay' on the ground before they are considered grounded and able to jump (in seconds)")]
    public float groundCheckDuration;
    public float rescueSpasmDelay;
    public float rescueSpasmIntensity;
    public float rescueSpasmDuration;

    public Mesh debugMesh;

    private Joint[] _joints;
    private float _jumpCharge = 0;
    private float _moveHopDelay = 0;
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
            _joints[index].SpringJoint2D.distance = relaxedDistance/springCount;
            _joints[index].SpringJoint2D.frequency = oscillatingFrequency;
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
        _moveHopDelay -= Time.deltaTime;
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
                    _joints[index].SpringJoint2D.distance = Mathf.Lerp(relaxedDistance/springCount, chargedDistance/springCount, _jumpCharge);
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
                else if (Input.GetAxis("Horizontal") != 0 && _moveHopDelay <= 0)
                {
                    // Bewegung nach links und rechts
                    Jump(0, moveForceSideways, ForceMode2D.Impulse);
                    _moveHopDelay = moveHopDelay;
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
            _joints[index].SpringJoint2D.distance = relaxedDistance / springCount;
            _joints[index].SpringJoint2D.dampingRatio = dampening;
            _joints[index].Rigidbody2D.drag = linearDrag;
        }
    }

    private void Jump(float jumpCharge, float jumpForceSideways, ForceMode2D forceMode = ForceMode2D.Force)
    {
        var upForce = jumpForceUp * jumpCharge;
        _joints[_topJointIndex].Rigidbody2D.mass = bottomJointMass; // quick fix for making the character actually jump instead of spiralling out of control
        _joints[_topJointIndex].Rigidbody2D.AddForce(new Vector2(Random.value*0.01f + jumpForceSideways*Input.GetAxis("Horizontal"), upForce), forceMode);
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
        }

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
    
    #endregion Steuerung
}
