using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGravityController : MonoBehaviour {

    public Vector3 m_vActualGravityDirection;
    public float m_fButtonDelay = 0.2f; //in seconds
    public float m_fCurrentElapsedTime = 0.0f;

    public HashSet<ParalyzableObject> m_hsParalyzableObjects = new HashSet<ParalyzableObject>();


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

	// Use this for initialization
	void Start () {
        m_vActualGravityDirection = Physics.gravity;

        //CAREFUL: This is not ready to react to NEWLY ADDED PARALYZABLE OBJECTS, so be careful.
        m_hsParalyzableObjects.UnionWith( FindObjectsOfType<ParalyzableObject>()); //Add all the found Paralyzable objects.

	}

    // Update is called once per frame
    void Update() {
        float fHorizontalInput = Input.GetAxis("Horizontal");
        float fVerticalInput = Input.GetAxis("Vertical");
        m_fCurrentElapsedTime += Time.deltaTime;
        if (Input.GetButtonDown("ChangeGravity") && m_fCurrentElapsedTime > m_fButtonDelay)
        {
            if (Mathf.Abs(fHorizontalInput) > 0.0f || Mathf.Abs(fVerticalInput) > 0.0f)
            {
                RotateObject();
                m_vActualGravityDirection = (transform.up * fVerticalInput + transform.right * fHorizontalInput).normalized * 9.81f;
                Physics.gravity = m_vActualGravityDirection;
                Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
                m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
            }
        }
        
	}
}
