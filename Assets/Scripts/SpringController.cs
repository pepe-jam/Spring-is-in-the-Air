using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringController : MonoBehaviour
{
    [SerializeField] private float jumpForceUp;
    [SerializeField] private float jumpForceSideways;
    [SerializeField] private float jumpChargeTime;
    [SerializeField] private int springCount;
    [SerializeField] private float jointScale;
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

    public float tiltStrength;

    public Mesh debugMesh;

    private Joint[] _joints;
    private float _jumpCharge = 0;
    private int _topJointIndex;

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
    }

    private void CreateSpringJointObject(int index)
    {
        _joints[index] = new Joint
        {
            GameObject = new GameObject("SpringJoint " + index, typeof(MeshFilter), typeof(MeshRenderer),
                typeof(Rigidbody2D), typeof(CircleCollider2D))
        };
        _joints[index].GameObject.transform.localScale = Vector3.one*jointScale;
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
            _joints[index].SpringJoint2D.distance = relaxedDistance;
            _joints[index].SpringJoint2D.frequency = oscillatingFrequency;
            SetBalancingJoint(index);
        }
        _joints[index].GameObject.transform.parent = gameObject.transform;   // macht das aktuelle gameObject zum Elternteil der neu erstellten SpringJoints
    }

    private void SetBottomJoint(int index)
    {
        _joints[index].Rigidbody2D.mass = bottomJointMass;
        _joints[index].Rigidbody2D.gravityScale = bottomJointGravityScale;
    }

    private void SetBalancingJoint(int index)
    {
        _joints[index].Rigidbody2D.mass = balancingJointMass;
        _joints[index].Rigidbody2D.gravityScale = balancingJointGravityScale;
    }
    
    // Update is called once per frame
    void Update()
    {
        // makes the spring tilt depending on the direction pressed
        _joints[_topJointIndex].Rigidbody2D.AddForce(Vector2.right * (Input.GetAxis("Horizontal") * tiltStrength));
        if (Input.GetKey(KeyCode.Space))
        {
            _jumpCharge = Mathf.Min(_jumpCharge + Time.deltaTime*jumpChargeTime, 1);
            _joints[0].Rigidbody2D.drag = linearDragWhileCharging;
            // makes the spring visibly charge by contracting its joints
            for (int index = 1; index < springCount; index++)
            {
                _joints[index].SpringJoint2D.distance = Mathf.Lerp(relaxedDistance, chargedDistance, _jumpCharge);
                _joints[index].SpringJoint2D.dampingRatio = dampeningWhileCharging;
                _joints[index].Rigidbody2D.drag = linearDragWhileCharging;
            }
        }
        else
        {
            if (_jumpCharge > 0)
            {
                Jump();
                _jumpCharge = 0;
                _joints[0].Rigidbody2D.drag = linearDrag;
                for (int index = 1; index < springCount; index++)
                {
                    _joints[index].SpringJoint2D.distance = relaxedDistance;
                    _joints[index].SpringJoint2D.dampingRatio = dampening;
                    _joints[index].Rigidbody2D.drag = linearDrag;
                }
            }
        }
    }

    private void Jump()
    {
        var upForce = jumpForceUp * _jumpCharge;
        _joints[_topJointIndex].Rigidbody2D.mass = bottomJointMass; // quick fix for making the character actually jump instead of spiralling out of control
        _joints[_topJointIndex].Rigidbody2D.AddForce(new Vector2(Random.value*0.01f + jumpForceSideways*Input.GetAxis("Horizontal"), upForce));
        Debug.Log("Applied jumping force to the "+_topJointIndex+". joint");
        TurnUpsideDown();

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

    private class Joint
    {
        public GameObject GameObject { get; set; }
        // So we don't have to call GetComponent() every time the spring turns around, we can save a reference to important components it as a property
        public SpringJoint2D SpringJoint2D { get; set; }    
        public Rigidbody2D Rigidbody2D;
    }
}
