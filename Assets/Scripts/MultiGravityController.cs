using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGravityController : MonoBehaviour {

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

    private int m_iBulletActualID = 0;
    protected Rigidbody m_pRigidbodyRef;


    void UpdateParalyzableObjects()
    {
        foreach ( ParalyzableObject PO in m_hsParalyzableObjects )
        {
            PO.DecreaseParalysis(); 
            //Also, if it is no longer paralyzed, then update the Walkable part of the object.
        }
    }

    void RotateObject()
    {
        float fAngle = Vector3.Angle(transform.up, Physics.gravity);
        transform.right = Vector3.Cross(-Physics.gravity.normalized, transform.forward);
        transform.up = -Physics.gravity.normalized;
        //gameObject.transform.Rotate(transform.forward, fAngle);
    }

    public void EraseBullet(GameObject in_pBulletToErase)
    {
        if (m_hsParalysisBullets.Contains(in_pBulletToErase))
        {
            m_hsParalysisBullets.Remove(in_pBulletToErase);
        }
        else
        {
            Debug.LogWarning("Somehow, a bullet tried to be erased twice or more. It had the ID: " + in_pBulletToErase.name);
        }

    }

    // Use this for initialization
    void Start () {
        m_vActualGravityDirection = Physics.gravity;

        //CAREFUL: This is not ready to react to NEWLY ADDED PARALYZABLE OBJECTS, so be careful.
        m_hsParalyzableObjects.UnionWith( FindObjectsOfType<ParalyzableObject>()); //Add all the found Paralyzable objects.
        m_pRigidbodyRef = GetComponentInChildren<Rigidbody>();

        if (m_pParalysisBulletRef == null)
        {
            Debug.LogError("Error, no paralysisbullet reference provided.");
        }
    }

    // Update is called once per frame
    void Update() {
        float fHorizontalInput = Input.GetAxis("Horizontal");
        float fVerticalInput = Input.GetAxis("Vertical");
        m_fCurrentElapsedTime += Time.deltaTime;
        if ((Mathf.Abs(fHorizontalInput) > 0.0f || Mathf.Abs(fVerticalInput) > 0.0f))
        {
            if (Input.GetButtonDown("ChangeGravity"))
            {
                if (m_fCurrentElapsedTime > m_fButtonDelay)
                {
                    m_vActualGravityDirection = (transform.up * fVerticalInput + transform.right * fHorizontalInput).normalized * 9.81f;
                    Physics.gravity = m_vActualGravityDirection;
                    Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
                    m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
                }
            }
            else
            {
                //Then, move normally.
                //NOTE: this should take a FORWARD ignoring the facing of the camera (stripping it from the Y axis), 
                Vector3 pMovementVector = fHorizontalInput * transform.right + fVerticalInput * transform.forward;
                //pMovementVector.y = 0.0f;
                m_pRigidbodyRef.AddForce(pMovementVector * m_fMovementAccelerationFactor, ForceMode.Force);
            }
        }

        /********************** CAMERA MOVEMENT ****************/
        float fRightHorizontal = Input.GetAxis("RightHorizontal");
        float fRightVertical = Input.GetAxis("RightVertical");

        if (Mathf.Abs(fRightHorizontal) > 0.0f || Mathf.Abs(fRightVertical) > 0.0f)
        {
            Quaternion tOtherQuat = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(fRightHorizontal * m_fHorizontalSensitivity, fRightVertical * m_fVerticalSensitivity, 0.0f));
            //How can one get the Acceleration?
            transform.rotation = Quaternion.RotateTowards(transform.rotation, tOtherQuat, 180.0f); //180????
        }

        if (Input.GetButtonDown("Shoot"))
        {
            //Create a new bullet with the position and rotation of the Gun which fires it.
            //NOTE: The gunRef must facing similar direction as the player camera (but adjusted like an FPS weapon.).
            GameObject pNewBullet = Instantiate<GameObject>(m_pParalysisBulletRef, m_pParalysisGunRef.transform.position, m_pParalysisGunRef.transform.rotation);
            pNewBullet.name = "Bullet " + m_iBulletActualID.ToString();
            m_hsParalysisBullets.Add(pNewBullet);
        }
    }
}
