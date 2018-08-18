using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGravityController : MonoBehaviour {

    protected Animator m_pCharacterAnimatorRef;
    public bool m_bGrounded;
    public float m_fGroundCheckRadius = 0.2f;
    public LayerMask m_GroundCheckLayerMask;


    public Vector3 m_vActualGravityDirection;
    public float m_fButtonDelay = 0.2f; //in seconds
    public float m_fCurrentElapsedTime = 0.0f;
    public float m_fHorizontalSensitivity = 20.0f;
    public float m_fVerticalSensitivity = 10.0f;
    public float m_fMovementAccelerationFactor = 1.0f;

    public GameObject m_pParalysisGunRef;
    public GameObject m_pParalysisBulletRef;

    public HashSet<ParalyzableObject> m_hsParalyzableObjects = new HashSet<ParalyzableObject>();
    protected HashSet<GameObject> m_hsParalysisBullets = new HashSet<GameObject>();
    protected HashSet<GameObject> m_hsParalysisBulletsToDestroy = new HashSet<GameObject>();
    protected int m_iBulletActualID = 0;

    //* Variables for the main character control, such as movement and jump. *//
    protected CharacterController m_pMainCharacterRef;
    public float m_fMaxSpeed = 10.0f;
    //public float m_fMaxJumpHeight = 5.0f;
    public float m_fMaxJumpingSpeed = 2.0f; //Affects how much can you change your movement while jumping.
    public float m_fGravityFallingMultiplier = 1.5f;
    public float m_fSmallJumpMultiplier = 1.0f;
    public bool m_bCanJump = false;
    protected Rigidbody m_pRigidbodyMainCharacterRef = null;
    protected bool m_bTouchingWalkable = false;

    protected void UpdateParalyzableObjects()
    {
        foreach (ParalyzableObject PO in m_hsParalyzableObjects)
        {
            PO.DecreaseParalysis();
            //Also, if it is no longer paralyzed, then update the Walkable part of the object.
        }
    }

    //void RotateObject()
    //{
    //    float fAngle = Vector3.Angle(transform.up, Physics.gravity);
    //    transform.right = Vector3.Cross(-Physics.gravity.normalized, transform.forward);
    //    transform.up = -Physics.gravity.normalized;
    //    //gameObject.transform.Rotate(transform.forward, fAngle);
    //}

    public void EraseBullet(GameObject in_pBulletToErase)
    {
        if (m_hsParalysisBullets.Contains(in_pBulletToErase))
        {
            m_hsParalysisBulletsToDestroy.Add(in_pBulletToErase); //Queue it so it is destroyed in the Fixed update.
        }
        else
        {
            Debug.LogWarning("Somehow, a bullet tried to be erased twice or more. It had the ID: " + in_pBulletToErase.name);
        }
    }

    //Should be called after the FixedUpdate function.
    protected void DestroyBullets()
    {
        //Bullets that were queued to destroy are definitely destroyed here.
        foreach (GameObject Bullet in m_hsParalysisBulletsToDestroy)
        {
            m_hsParalysisBullets.Remove(Bullet);
            Destroy(Bullet);
        }
    }



    // Use this for initialization
    protected virtual void Start () {
        m_vActualGravityDirection = Physics.gravity;

        m_pCharacterAnimatorRef = GetComponent<Animator>();
        if (m_pCharacterAnimatorRef == null)
        {
            //then it has not found the right component.
            Debug.LogError("Error, this MultigravityController has not found the corresponding Animator component.");
        }

        //CAREFUL: This is not ready to react to NEWLY ADDED PARALYZABLE OBJECTS, so be careful.
        m_hsParalyzableObjects.UnionWith( FindObjectsOfType<ParalyzableObject>()); //Add all the found Paralyzable objects.
        m_pRigidbodyMainCharacterRef = GetComponent<Rigidbody>();

        if (m_pRigidbodyMainCharacterRef == null)
        { Debug.LogError("No rigidbody component was attached to the MultiGravityController game object. please check."); }

        if (m_pParalysisBulletRef == null)
        {
            Debug.LogError("Error, no paralysisbullet reference provided.");
        }
    }

    protected void CharacterUpdate(float in_fVerticalInput, float in_fHorizontalInput)
    {
        if (m_bCanJump && Input.GetButtonDown("Jump"))
        {
            m_pRigidbodyMainCharacterRef.AddForce(m_pRigidbodyMainCharacterRef.transform.up * m_fMaxJumpingSpeed, ForceMode.Impulse);
            Debug.Log("entered the jump.");
        }
        Vector3 pMovementVector = in_fHorizontalInput * Vector3.right;// + fVerticalInput * transform.forward;
        m_pRigidbodyMainCharacterRef.AddForce(pMovementVector * m_fMovementAccelerationFactor, ForceMode.Force);
    }

    // Update is called once per frame
    protected virtual void Update() {
        float fHorizontalInput = Input.GetAxis("Horizontal");
        float fVerticalInput = Input.GetAxis("Vertical");
        m_fCurrentElapsedTime += Time.deltaTime;
        if ((Mathf.Abs(fHorizontalInput) > 0.0f || Mathf.Abs(fVerticalInput) > 0.0f))
        {
            if (Input.GetButtonDown("ChangeGravity"))
            {
                if (m_fCurrentElapsedTime > m_fButtonDelay)
                {
                    //If the vertical axis value is greater than the horizontal, then invert in vertical.
                    if (Mathf.Abs(fVerticalInput) > Mathf.Abs(fHorizontalInput))
                    {
                        //Then check if the axis is positive or negative
                        if (fVerticalInput > 0.0f)
                        {
                            m_vActualGravityDirection = (Vector3.up * 9.81f);
                        }
                        else
                        {
                            m_vActualGravityDirection = (Vector3.up * -9.81f);
                        }
                    }
                    else
                    {
                        //Then check if the Horizontal axis is positive or negative
                        if (fHorizontalInput > 0.0f)
                        {
                            m_vActualGravityDirection = (Vector3.right * 9.81f);
                        }
                        else
                        {
                            m_vActualGravityDirection = (Vector3.right * -9.81f);
                        }
                    }

                    //m_vActualGravityDirection = (transform.up * fVerticalInput + transform.right * fHorizontalInput).normalized * 9.81f;
                    Physics.gravity = m_vActualGravityDirection;
                    Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
                    m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
                }
            }
        }


        /********************** CAMERA MOVEMENT **************** IN 2D THERE'S NO NEED TO ROTATE THE CAMERA/
        /*float fRightHorizontal = Input.GetAxis("RightHorizontal");
        float fRightVertical = Input.GetAxis("RightVertical");

        if (Mathf.Abs(fRightHorizontal) > 0.0f || Mathf.Abs(fRightVertical) > 0.0f)
        {
            Quaternion tOtherQuat = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(fRightHorizontal * m_fHorizontalSensitivity, fRightVertical * m_fVerticalSensitivity, 0.0f));
            //How can one get the Acceleration?
            transform.rotation = Quaternion.RotateTowards(transform.rotation, tOtherQuat, 180.0f); //180????
        }*/
        UpdateParalyzableObjects();

        if (Input.GetButtonDown("Shoot"))
        {
            //Create a new bullet with the position and rotation of the Gun which fires it.
            //NOTE: The gunRef must facing similar direction as the player camera (but adjusted like an FPS weapon.).
            GameObject pNewBullet = Instantiate<GameObject>(m_pParalysisBulletRef, m_pParalysisGunRef.transform.position, m_pParalysisGunRef.transform.rotation);
            pNewBullet.GetComponent<ParalysisBullet>().Shoot(this);//Need to pass this so it can let me know when to destroy it.
            pNewBullet.name = "Bullet " + m_iBulletActualID.ToString();
            m_iBulletActualID++;
            m_hsParalysisBullets.Add(pNewBullet);
        }
        
    }


    protected void FixedUpdate()
    {
        float fHorizontalInput = Input.GetAxis("Horizontal");
        float fVerticalInput = Input.GetAxis("Vertical");

        CharacterUpdate(fVerticalInput, fHorizontalInput); //Call to update the other possible commands for the main character.

        m_pCharacterAnimatorRef.SetFloat("HorizontalSpeed", Mathf.Abs(m_pRigidbodyMainCharacterRef.velocity.x)); // we set it for the animator to use.
        m_pCharacterAnimatorRef.SetFloat("VerticalSpeed", m_pRigidbodyMainCharacterRef.velocity.y);

        DestroyBullets();
    }
}
