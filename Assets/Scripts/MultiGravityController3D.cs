using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGravityController3D : MultiGravityController {

    /*MAPPING FOR THE XBOX ONE CONTROLLER*/
    /* https://answers.unity.com/questions/1350081/xbox-one-controller-mapping-solved.html */

    /*By default, this is the direction. This variable must be equal to the inverse of the normal of the wall in front (raycast) of the camera.*/
    Vector3 m_vActualForwardDirection = new Vector3(0.0f, 0.0f, 1.0f);
    Vector3 m_vActualRightDirection = new Vector3(1.0f, 0.0f, 0.0f); //Used to hold the value for the right direction of the orthonormal base formed. So we don't need to calculate it again.
    GameObject m_pActualForwardWall;

    Vector3 vCameraWithoutCurrentVertical = new Vector3(0.0f, 0.0f, 0.0f);
    Camera m_CurrentCamera = null;

    //void RotateObject()
    //{
    //    float fAngle = Vector3.Angle(transform.up, Physics.gravity);
    //    transform.right = Vector3.Cross(-Physics.gravity.normalized, transform.forward);
    //    transform.up = -Physics.gravity.normalized;
    //    //gameObject.transform.Rotate(transform.forward, fAngle);
    //}



    void UpdateActualDirections()
    {

        //We raycast from the center of the camera to any of the walls (not ceiling or floor).
        //But this raycast must be against infinite walls.
        vCameraWithoutCurrentVertical = m_CurrentCamera.gameObject.transform.forward;
        //Now, we check which axis to remove from the camera direction, based on which is the current gravity direction.
        if (m_vActualGravityDirection.x != 0.0f)
        {
            vCameraWithoutCurrentVertical.x = 0.0f;
        }
        else if (m_vActualGravityDirection.y != 0.0f)
        {
            vCameraWithoutCurrentVertical.y = 0.0f;
        }
        else if (m_vActualGravityDirection.z != 0.0f)
        {
            vCameraWithoutCurrentVertical.z = 0.0f;
        }
        else
        {
            //In case none of the previous happens, we have a serious problem here!
            Debug.LogError("Grave error, no direction set for  m_vActualGravityDirection in 3D gravity controller.");
        }

        /*EXAMPLE OF THE LAYER MASK USAGE-
         * // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;*/

        int iLayerMaskWalls = 1 << LayerMask.NameToLayer("Walls"); //This can be hastened by using the final layer id given to walls.
        RaycastHit RHInfo;
        //Now, we use that direction to make a raycast. 
        if (Physics.Raycast(m_CurrentCamera.transform.position, vCameraWithoutCurrentVertical, out RHInfo, 1000.0f, iLayerMaskWalls))
        {
            //First, check if this wall is different from the previously hit one.
            if (RHInfo.collider.gameObject == m_pActualForwardWall)
                return; //If they're equal, just exit this functions, no update is needed here.

            if (m_pActualForwardWall != null)
            {   //NOTE: If we could set up this actualForwardWall from the start, we could avoid this "if" each time it enters this method.
                m_pActualForwardWall.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f); // reset the material of the former front wall.
            }

            //Then, Check which wall it hit and get its normal. The opposite of that direction will be the new Forward.
            m_pActualForwardWall = RHInfo.collider.gameObject; //Save the object so no overwriting is done if wall is repeated.
            m_vActualForwardDirection = -RHInfo.normal;
            m_vActualRightDirection = Vector3.Cross(m_vActualGravityDirection, m_vActualForwardDirection);
            Debug.Log("The value obtained from Cross(m_vActualGravityDirection, m_vActualForwardDirection) was: " + m_vActualRightDirection);
            Vector3.OrthoNormalize(ref m_vActualGravityDirection, ref m_vActualForwardDirection, ref m_vActualRightDirection);
            Debug.Log("The orthonormalized values obtained in comparison to the cross product, were: " + m_vActualGravityDirection + ", " + m_vActualForwardDirection + ", " + m_vActualRightDirection);

            //Now, highlight that wall's appearance, to make it obvious to the player that's the one in forward right now.
            Renderer WallRenderer =  m_pActualForwardWall.GetComponent<Renderer>();
            if (WallRenderer == null)
            { Debug.LogError("The hit wall has no renderer component, please check that so we can highlight it as the forward one. Exiting this function for safety."); return; }

            //Change the color of the material.
            WallRenderer.material.color = new Color(1.0f, 0.0f, 1.0f); //By default, this is the highlight color for the front wall.
            //WallRenderer.material. TO DO: Change the shader flags or some textures.

        }
        //Otherwise, no wall has been hit, which is wrong, so a message must be displayed.
        else { Debug.LogError("No wall hit by the raycast, please check the walls and the directions, as well as the walls' normals, or the max distance."); }

    }

    // Use this for initialization
    protected override void Start()
    {
        m_vActualGravityDirection = Physics.gravity;

        m_pCharacterAnimatorRef = GetComponent<Animator>();
        if (m_pCharacterAnimatorRef == null)
        {
            //then it has not found the right component.
            Debug.LogError("Error, this MultigravityController has not found the corresponding Animator component.");
        }

        //CAREFUL: This is not ready to react to NEWLY ADDED PARALYZABLE OBJECTS, so be careful.
        m_hsParalyzableObjects.UnionWith(FindObjectsOfType<ParalyzableObject>()); //Add all the found Paralyzable objects.
        m_pRigidbodyMainCharacterRef = GetComponent<Rigidbody>();

        if (m_pRigidbodyMainCharacterRef == null)
        { Debug.LogError("No rigidbody component was attached to the MultiGravityController game object. please check."); }

        if (m_pParalysisBulletRef == null)
        {
            Debug.LogError("Error, no paralysisbullet reference provided.");
        }

        m_CurrentCamera = Camera.main;
    }



    // Update is called once per frame
    protected override void Update()
    {
        float fHorizontalInput = Input.GetAxis("Horizontal");
        float fVerticalInput = Input.GetAxis("Vertical");

        //These float handle the direction of the gravity change given by the user. 
        float fGravForwardBackward = Input.GetAxis("GravityForwardBackward");
        float fGravRightLeft = Input.GetAxis("GravityRightLeft");
        bool bGravInvertActual = Input.GetButton("GravityInvertActual");


        m_fCurrentElapsedTime += Time.deltaTime;

        //We need to check for changes in the forward direction here, as the camera could 've changed.
        UpdateActualDirections();
        if(m_fCurrentElapsedTime >= m_fButtonDelay )
        if (bGravInvertActual)
        {
            //Then, invert the gravity to the one active right now. This one has priority over all the others.
            m_vActualGravityDirection = Physics.gravity = -Physics.gravity; //invert it, the one active right now. Also set it in this class's variable.
            Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
            m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
        }
        else if (Mathf.Abs(fGravForwardBackward) > 0.0f)
        {
            float fDirection = fGravForwardBackward > 0.0f ? 1.0f : -1.0f;
            //Then we go to the forward or the backwards wall. The forward direction must be displayed constantly so the player knows it.
            m_vActualGravityDirection = m_vActualForwardDirection * fDirection; //After this, you need to recalculate the actual directions of the player. WARNING.
            Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
            m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
        }
        else if (Mathf.Abs(fGravRightLeft) > 0.0f)
        {
            float fDirection = fGravRightLeft > 0.0f ? 1.0f : -1.0f;
            //Then we go to the forward or the backwards wall. The forward direction must be displayed constantly so the player knows it.
            m_vActualGravityDirection = m_vActualRightDirection * fDirection; //After this, you need to recalculate the actual directions of the player. WARNING.
            Debug.Log("Changing gravity to: " + m_vActualGravityDirection.ToString());
            m_fCurrentElapsedTime = 0.0f; //reset it so the player has to wait a while before re-changing the gravity.
        }

        if ((Mathf.Abs(fHorizontalInput) > 0.0f || Mathf.Abs(fVerticalInput) > 0.0f))
        {
            //Now, to handle the character's movement.
            //First, we get current forward of the camera. Which is in vCameraWithoutCurrentVertical = Camera.current.transform.forward;
            //Next, we multiply it for the horizontal and vertical inputs.
            Vector3 vAddedForce = new Vector3(vCameraWithoutCurrentVertical.x * fHorizontalInput, 0.0f, vCameraWithoutCurrentVertical.z * fVerticalInput);
            m_pRigidbodyMainCharacterRef.AddForce(vAddedForce, ForceMode.Force);
            //Also, we must limit the character's max speed.
            //TO DO:
            //Supposedly, the character must cease to move by the friction with the floor, right?
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


}
