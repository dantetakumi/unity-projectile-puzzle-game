using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    // fields set in the Unity Inspector pane 
    [Header("Inscribed")]                                                      
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    // fields set dynamically
    [Header("Dynamic")]                                                      
    public GameObject launchPoint;
    public Vector3 launchPos;                                   
    public GameObject projectile;                                     
    public bool aimingMode;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
    }
    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        // The player has pressed the mouse button while over Slingshot
        aimingMode = true;
        // Instantiate a Projectile
        projectile = Instantiate(projectilePrefab) as GameObject;
        // Start it at the launchPoint
        projectile.transform.position = launchPos;
        // Set it to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        if (!aimingMode) return;

        // Mouse position in world space
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Delta from launch position
        Vector3 mouseDelta = mousePos3D - launchPos;

        // Clamp to the slingshot "aura" radius
        float maxMagnitude = GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta = mouseDelta.normalized * maxMagnitude;
        }

        // Make the projectile follow the mouse (but limited to the aura)
        projectile.transform.position = launchPos + mouseDelta;

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;

            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Fire opposite the pull direction, using the CLAMPED mouseDelta
            projRB.velocity = -mouseDelta * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;

            Instantiate<GameObject>(projLinePrefab, projectile.transform);

            projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }
}
