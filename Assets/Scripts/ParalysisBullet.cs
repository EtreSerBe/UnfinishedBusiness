using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalysisBullet : MonoBehaviour {

    public float m_fInitialImpulse = 1.0f;
    public float m_fMaxSpeed = 20.0f;
    public float m_fMinSpeed = 1.0f;
    public float m_fTimeToLive = 5.0f;
    //public float m_f
    private Rigidbody m_pRigidbodyRef;
    private MultiGravityController m_pMultiGravityControllerRef;

	// Use this for initialization
	void Start () {
        m_pRigidbodyRef = GetComponent<Rigidbody>();
	}

    private void OnCollisionEnter(Collision collision)
    {
        CancelInvoke();
        //Maybe I should also disappear it, so it doesn't look weird.
        m_pMultiGravityControllerRef.EraseBullet(gameObject); //Erase this game object from the Bullet manager.
    }

    void EndTimeToLive()
    {
        m_pMultiGravityControllerRef.EraseBullet(gameObject); //Erase this game object from the Bullet manager.
    }

    public void Shoot( MultiGravityController in_pMGCref )
    {
        m_pMultiGravityControllerRef = in_pMGCref;
        m_pRigidbodyRef.AddForce(transform.forward*m_fInitialImpulse, ForceMode.Impulse);
        Invoke("EndTimeToLive", m_fTimeToLive);//Invoked only if it does not collide with something, so it doesn't live forever.
    }

	// Update is called once per frame
	void Update () {
		//SHOULD THIS HAVE AN UPDATE? I don't think so.
        //Maybe I could add different funny behaviors, like bullet which stop in motion for a while, and can be used for crazy designs. :)
	}
}
