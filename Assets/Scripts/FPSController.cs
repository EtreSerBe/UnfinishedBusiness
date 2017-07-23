using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {

    public float m_fMaxSpeed = 10.0f;
    public float m_fMaxJumpHeight = 5.0f;
    public float m_fMaxJumpingSpeed = 2.0f; //Affects how much can you change your movement while jumping.
    public bool m_bCanJump = false;

    protected Rigidbody m_pRigidbodyRef = null;
    private bool m_bTouchingWalkable = false;
    
    HashSet<GameObject> m_hsWalkableObjects = new HashSet<GameObject>();

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Walkable"))
        {
            m_bCanJump = true;
            m_hsWalkableObjects.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Walkable"))
        {
            m_hsWalkableObjects.Remove(collision.gameObject); //Maybe check if it has the item, anyway it will be removed.
            if (m_hsWalkableObjects.Count == 0) //Then, it is touching nothing so it cannot jump again.
            {
                m_bCanJump = false;
                Debug.Log("Not touching any walkable, it cannot jump now.");
            }
        }
        
    }

    

    // Use this for initialization
    void Start () {
        m_pRigidbodyRef = GetComponent<Rigidbody>();
        if ( m_pRigidbodyRef == null )
        { Debug.LogError("No rigidbody component was attached to the FPS controller game object. please check."); }

	}
	
	// Update is called once per frame
	void Update () {

        if ( m_bCanJump && Input.GetButtonDown("Jump") )
        {
            m_pRigidbodyRef.AddForce(Vector3.up * m_fMaxJumpingSpeed, ForceMode.Impulse);
        }

	}
}
