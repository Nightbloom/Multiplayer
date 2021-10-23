using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(ConfigurableJoint))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;

    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [SerializeField]
    private LayerMask enviromentMask;

    [Header("Spring settings: ")]
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;


    //COmponent Caching
    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSetting(jointSpring);
    }

    void Update()
    {
        //Setting target postion for spring
        //This makes the physics act right when it comes to
        //applying gravity when flying ovet the obejects
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, enviromentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
             else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);

        }
                
        // calculate movemoment velocity as a 3d vector
        float xMov = Input.GetAxis("Horizontal");
        float zMov = Input.GetAxis("Vertical");

        Vector3 moveHorizontal = transform.right * xMov;
        Vector3 moveVertical = transform.forward * zMov;

        
        //Final movement vector
        Vector3 velocity = (moveHorizontal + moveVertical).normalized * speed;

        // Animate momement
        animator.SetFloat("ForwardVelocity", zMov );

        //Apply movement

        motor.Move(velocity);


        // Calculate rotation as a 3D vector (turning around)

        float yRot = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        // Apply rotation
        motor.Rotate(rotation);

        // Calculate camera rotation as a 3D vector (turning around)

        float xRot = Input.GetAxisRaw("Mouse Y");

        float cameraRotationX = xRot * lookSensitivity;

        // Apply Camera rotation
        motor.cameraRotate(cameraRotationX);

       
        
        // Calculate  the thrusterforce based on player input
        Vector3 _thrusterForce = Vector3.zero;
        if(Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {

            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime; 
            
            _thrusterForce = Vector3.up * thrusterForce;
            SetJointSetting(0f);

        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime; 
               
            SetJointSetting(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        //Apply the thruster force
        motor.ApplyThruster(_thrusterForce);

    }

    private void  SetJointSetting(float _jointSpring)
    {
        joint.yDrive = new JointDrive { positionSpring = jointSpring, maximumForce = jointMaxForce };

    }

} // class
