using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalyzableObject : MonoBehaviour {

    public bool m_bIsParalyzed = false;
    public int m_iMaxParalysis = 1; //The max number of "RemainingParalysis" this object can stack.
    public int m_iRemainingParalysis = 0; //The number of times it will remain static before responding to gravity again.


    //public Collider m_pCollider

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Paralysis"))
        {
            m_iRemainingParalysis = m_iRemainingParalysis + 1 > m_iMaxParalysis ? m_iMaxParalysis : m_iRemainingParalysis + 1; //limit it to the max
        }
    }

    //This function has to be called from the Multi gravity controller, so it decreases all the paralyzable objects' paralysis by 1.
    public void DecreaseParalysis()
    {
        m_iRemainingParalysis = m_iRemainingParalysis < 0 ? m_iRemainingParalysis -1 :0; //Limit it from below to 0.
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
