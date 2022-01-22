using System;
using System.Collections;
using FMODUnity;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class SpringController : MonoBehaviour
{
    [Header("Appearance")]
    public int segmentCount;
    public float height;
    public float segmentScale;
    //public Mesh segmentMesh;
    //public Material segmentMaterial;
    public Sprite face;
    [Tooltip("Size of the face's segment and therefore the face itself")]
    public float faceScale;
    public Material jointMaterial;
    public float jointWidth;

    [Header("Physical Properties")]
    public float linearDrag;
    public float angularDrag;
    public float dampening; // dampening ratio for spring joints
    public float bottomJointMass;
    public float bottomJointGravityScale;
    public float balancingJointMass;
    public float balancingJointGravityScale;
    public float oscillatingFrequency;
    public PhysicsMaterial2D physicsMaterial2D;

    
    [Header("Jumping")]
    public float jumpForceUp;
    public float jumpForceSideways;
    public float jumpChargeTime;
    public float linearDragWhileCharging;
    public float dampeningWhileCharging; // dampening ratio for spring joints
    public float chargingContraction;
    [Tooltip("Controls for how many seconds after a jump the GroundCheck always fails to prevent weird things from happening.")]
    public float groundCheckSkipDuration;
    [Tooltip("How far the player leans into the direction they are aiming towards")]
    public float tiltStrength;
    

    [Header("Walking (Left-Right-Movement)")]
    [Tooltip("How long it takes for a single move cycle to complete")]
    public float moveDuration;

    [Tooltip("The time the player waits between two move cycles. Must be a positive number.")]
    public float moveDelay;
    //public float moveForceSideways;
    [Tooltip("How far the player will move relative to their height")]
    public float moveDistance;
    public float moveAnimationSpeed;
    [Tooltip("0 means the player will bash their head into the ground when walking, 1 means they will merely lean over before falling over")]
    [Range(0f, 1f)]
    public float walkingHeight;
    
    
   
    [Header("Ground Check")]
    public LayerMask groundLayers;
    [Tooltip("How long the player has to 'lay' on the ground before they are considered grounded and able to jump (in seconds)")]
    public float groundCheckDuration;
    
    [Header("Rescue Spasm for when the groundcheck fails and the player gets stuck")]
    public float rescueSpasmDelay;
    public float rescueSpasmIntensity;
    public float rescueSpasmDuration;

    [Header("Audio")] 
    public CollisionSoundEvents collisionSoundEvents;
    public StudioEventEmitter jumpSound;
    public StudioEventEmitter startWalkSound;
    public  StudioEventEmitter stopWalkSound;

    private bool _canMove = true;
    private Segment[] _segments;
    private float _jumpCharge = 0;
    private float _lastTimeMoved = 0;
    private float _lastTimeJumped = 0;
    private float _secondsGrounded = 0;
    private int _topJointIndex;
    private int _bottomJointIndex;
    private float _lastFloorCollisionTime;

    private LineRenderer _lineRenderer;
    
    
    #region public Getters

    public GameObject GetFaceSegment()
    {
        try
        {
            return _segments[segmentCount - 1].GameObject;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    #endregion

    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
    }
    
    #region Initialisierung

    // Start is called before the first frame update
    void Start()
    {
        _segments = new Segment[segmentCount];   // initialize the array containing all joints the character has
        // create the joints of the character
        for (int index = 0; index < segmentCount; index++)
        {
            CreateSpringJointObject(index);
        }
        _topJointIndex = segmentCount - 1;
        _bottomJointIndex = 0;
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        jointMaterial.mainTextureScale = new Vector2(1 / jointWidth, 1);    // to avoid texture stretching on the linerenderer 
        _lineRenderer.material = jointMaterial;
        _lineRenderer.positionCount = segmentCount;
        _lineRenderer.widthMultiplier = jointWidth;
        _lineRenderer.textureMode = LineTextureMode.Tile;
        
    }

    private class Segment
    {
        public GameObject GameObject { get; set; }
        // So we don't have to call GetComponent() every time the spring turns around, we can save a reference to important components it as a property
        public SpringJoint2D SpringJoint2D { get; set; }    
        public Rigidbody2D Rigidbody2D;
        public CollisionAudioPlayer CollisionAudioPlayer;
    }
    private void CreateSpringJointObject(int index)
    {
        _segments[index] = new Segment
        {
            GameObject = new GameObject("SpringJoint " + index,
                typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(CollisionAudioPlayer)) 
        };
        _segments[index].GameObject.transform.position = gameObject.transform.position; // makes the Player spawn at the Player Object's position instead of at the world's origin
        _segments[index].GameObject.layer = LayerMask.NameToLayer("Player");  // Add all joints to a separate layer to make ground collision checks possible
        _segments[index].GameObject.transform.localScale = Vector3.one*segmentScale/segmentCount;
        _segments[index].Rigidbody2D = _segments[index].GameObject.GetComponent<Rigidbody2D>();
        _segments[index].Rigidbody2D.drag = linearDrag;
        _segments[index].Rigidbody2D.angularDrag = angularDrag;
        _segments[index].Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // prevents the player from partially phasing through walls
        _segments[index].Rigidbody2D.sharedMaterial = physicsMaterial2D;
        _segments[index].GameObject.GetComponent<Collider2D>().sharedMaterial = physicsMaterial2D;
        _segments[index].CollisionAudioPlayer = _segments[index].GameObject.GetComponent<CollisionAudioPlayer>();
        _segments[index].CollisionAudioPlayer.collisionSoundEvents = collisionSoundEvents;
        _segments[index].CollisionAudioPlayer.ignoredPhysicsMaterials = new[] { physicsMaterial2D };
        if (index == 0)
        {
            SetBottomJoint(index);
        }
        else
        {
            _segments[index].SpringJoint2D = _segments[index].GameObject.AddComponent<SpringJoint2D>();
            _segments[index].SpringJoint2D.connectedBody = _segments[index - 1].GameObject.GetComponent<Rigidbody2D>(); // attach this SpringJoint to the joint before it
            _segments[index].SpringJoint2D.anchor = new Vector2(0, -0.5f);
            _segments[index].SpringJoint2D.connectedAnchor = new Vector2(0, 0);
            _segments[index].SpringJoint2D.dampingRatio = dampening;
            _segments[index].SpringJoint2D.autoConfigureDistance = false;
            _segments[index].SpringJoint2D.distance = height/segmentCount;
            _segments[index].SpringJoint2D.frequency = oscillatingFrequency;
            _segments[index].SpringJoint2D.enableCollision = false;
            SetBalancingJoint(index);
        }
        _segments[index].GameObject.transform.parent = gameObject.transform;   // macht das aktuelle gameObject zum Elternteil der neu erstellten SpringJoints
        if (index == Mathf.Round((segmentCount-1)/2f))
        {
            var faceRenderer = _segments[index].GameObject.AddComponent<SpriteRenderer>();
            faceRenderer.sprite = face;
            _segments[index].GameObject.transform.localScale = Vector3.one*faceScale/segmentCount;
            faceRenderer.sortingLayerName = "Player";
            faceRenderer.sortingOrder = 99; // render King Spring's face above everything else
        }
    }

    private void SetBottomJoint(int index)
    {
        _segments[index].Rigidbody2D.mass = bottomJointMass;
        _segments[index].Rigidbody2D.gravityScale = bottomJointGravityScale;
        _bottomJointIndex = index;
    }

    private void SetBalancingJoint(int index)
    {
        _segments[index].Rigidbody2D.mass = balancingJointMass/segmentCount;
        _segments[index].Rigidbody2D.gravityScale = balancingJointGravityScale;
    }
    

    
    # endregion Initialisierung
    

    
    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            Movement();
        }
        UpdateLineRenderer();
    }
    
    # region Steuerung


    private void Movement()
    {
        if (GroundCheck())
        {
            BalancingJointGravity();
            _lastFloorCollisionTime = Time.time;

            if (Input.GetKey(KeyCode.Space))
            {
                // makes the spring tilt into the direction of the jump
                _segments[_topJointIndex].Rigidbody2D
                    .AddForce(Vector2.right * (Input.GetAxis("Horizontal") * tiltStrength));
                _jumpCharge =
                    Mathf.Min(_jumpCharge + Time.deltaTime * jumpChargeTime,
                        1); // gradually charge a jump while the jump button is held down
                _segments[0].Rigidbody2D.drag = linearDragWhileCharging;
                // makes the spring visibly charge by contracting its joints
                for (int index = 1; index < segmentCount; index++)
                {
                    _segments[index].SpringJoint2D.distance = Mathf.Lerp(height / segmentCount,
                        (height * chargingContraction) / segmentCount, _jumpCharge);
                    _segments[index].SpringJoint2D.dampingRatio = dampeningWhileCharging;
                    _segments[index].Rigidbody2D.drag = linearDragWhileCharging;
                }
                jointMaterial.mainTextureScale = new Vector2(Mathf.Lerp(1, 1/chargingContraction, _jumpCharge) / jointWidth, 1);
            }
            else
            {
                if (_jumpCharge > 0)
                {
                    // Jump high into the air (or not, depending on the value of _jumpCharge)
                    Jump(_jumpCharge, jumpForceSideways);
                    _jumpCharge = 0;
                    ResetPhysicalProperties();
                    jointMaterial.mainTextureScale = new Vector2(1 / jointWidth, 1);
                }
                else if (Input.GetAxis("Horizontal") != 0 && Time.time - _lastTimeMoved > moveDelay)
                {
                    // Bewegung nach links und rechts
                    //Jump(0, moveForceSideways, ForceMode2D.Impulse);
                    StartCoroutine(nameof(Walk));
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
        _segments[0].Rigidbody2D.drag = linearDrag;
        for (int index = 1; index < segmentCount; index++)
        {
            _segments[index].SpringJoint2D.distance = height / segmentCount;
            _segments[index].SpringJoint2D.dampingRatio = dampening;
            _segments[index].Rigidbody2D.drag = linearDrag;
        }
    }

    private void Jump(float jumpCharge, float jumpForceSideways, ForceMode2D forceMode = ForceMode2D.Force)
    {
        var jumpForce = jumpCharge * new Vector2(jumpForceSideways * Input.GetAxis("Horizontal"),
            jumpForceUp);
        _segments[_topJointIndex].Rigidbody2D.mass = bottomJointMass; // quick fix for making the character actually jump instead of spiralling out of control
        _segments[_topJointIndex].Rigidbody2D.AddForce(jumpForce, forceMode);
        TurnUpsideDown();
        BalancingJointGravity(true);
        _lastTimeJumped = Time.time;
        var parameters = jumpSound.Params;
        jumpSound.Play();
        jumpSound.SetParameter("jumpCharge", jumpCharge);   // in that order.
    }
    private void TurnUpsideDown()
    {
        if (_topJointIndex==0)
        {
            _topJointIndex = segmentCount - 1;
            SetBalancingJoint(segmentCount-1);
            SetBottomJoint(0);
        }
        else
        {
            _topJointIndex = 0;
            SetBalancingJoint(0);
            SetBottomJoint(segmentCount-1);
        }
    }

    private void BalancingJointGravity(bool weightless = false)
    {
        for (int index = 0; index < segmentCount; index++)
        {
            if (index != _bottomJointIndex)
            {
                _segments[index].Rigidbody2D.gravityScale = weightless ? 0 : balancingJointGravityScale;
            }
        }
    }

    private void RescueSpasm()
    {
        // rescue spasm
        for (int index = 1; index < segmentCount; index++)
        {
            _segments[index].SpringJoint2D.distance = rescueSpasmIntensity * height / segmentCount;
            _segments[index].SpringJoint2D.dampingRatio = dampeningWhileCharging;
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
        if (Time.time - _lastTimeJumped < groundCheckSkipDuration)
        {
            return false;
        }
        if (Physics2D.OverlapCircle(_segments[_bottomJointIndex].Rigidbody2D.position, _segments[_bottomJointIndex].GameObject.transform.lossyScale.y + 0.0001f, groundLayers))
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
    private IEnumerator Walk()
    {
        _lastTimeMoved = Time.time;
        // direction will be 1 or -1, depending on whether the player wanted to go left or right
        float direction = Input.GetAxis("Horizontal") / Mathf.Abs(Input.GetAxis("Horizontal"));
        Vector2 positionOffset = new Vector2(
            direction * moveDistance * height, 
            walkingHeight/height);
        _segments[_bottomJointIndex].Rigidbody2D.gravityScale = bottomJointGravityScale;
        Vector2 bottomJointVelocity = _segments[_bottomJointIndex].Rigidbody2D.velocity;  // copy
        startWalkSound.Play();
        
        while (Time.time - _lastTimeMoved < moveDuration)
        {
            var currentPosition = _segments[_topJointIndex].Rigidbody2D.position;
            var targetPosition = _segments[_bottomJointIndex].Rigidbody2D.position + positionOffset;
            _segments[_topJointIndex].Rigidbody2D.velocity = (targetPosition - currentPosition) * moveAnimationSpeed;
            yield return null;
        }

        _segments[_topJointIndex].Rigidbody2D.velocity = bottomJointVelocity; // paste
        TurnUpsideDown();
        BalancingJointGravity();
        stopWalkSound.Play();
        
    }
    
    
    #endregion Steuerung

    private void UpdateLineRenderer()
    {
        for (int index = 0; index < _segments.Length; index++)
        {
            _lineRenderer.SetPosition(index, _segments[index].GameObject.transform.position);
        }
    }

}
