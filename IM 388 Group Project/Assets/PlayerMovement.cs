/*****************************************************************************
// File Name :         PlayerMovement.cs
// Author :            Jacob Welch
// Creation Date :     28 August 2021
//
// Brief Description : Handles the movement of the player.
*****************************************************************************/
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    #region Shoot Forces
    [Header("Shoot forces")]
    [SerializeField]
    [Tooltip("How much force cannons shoot out objects")]
    [Range(0, 5000)]
    private float shootForce = 1000;

    [SerializeField]
    [Tooltip("How much force pushes the cannon back when it shoots")]
    [Range(100, 1000)]
    private float shootPushBackForce = 100;

    [SerializeField]
    [Tooltip("How much angular momentum is added to the cannon when it is shot")]
    [Range(200, 4000)]
    private float shootTorque = 800;
    #endregion

    #region Speed Limits
    [Header("Speed Limits")]
    [SerializeField]
    [Tooltip("The max velocity of cannons")]
    [Range(5, 30)]
    private float maxSpeed = 10;

    [SerializeField]
    [Tooltip("The max amount of spin a cannon can have")]
    [Range(300, 1000)]
    private float maxAngularVelocity = 600;
    #endregion

    #region Shoot Positions
    [Header("Shoot positions")]
    [SerializeField]
    [Tooltip("The position the cannon is shot from")]
    private GameObject shootPos;

    [SerializeField]
    [Tooltip("The position the cannon pivots from")]
    private GameObject pivotPos;
    #endregion

    #region Extras
    [Header("Extras")]
    [SerializeField]
    [Tooltip("The cannon part of the heirarchy")]
    private GameObject cannon;
    #endregion

    #region Components
    /// <summary>
    /// Current Cinemachine being used.
    /// </summary>
    [HideInInspector]
    public CinemachineVirtualCamera currentVCam;

    /// <summary>
    /// The Rigidbody2D component of this cannon.
    /// </summary>
    [HideInInspector]
    public Rigidbody2D rb2d;

    /// <summary>
    /// Reference to the main camera in this scene.
    /// </summary>
    private Camera mainCam;
    #endregion

    #region Bools
    /// <summary>
    /// Holds true if the player is allowed to aim the cannon.
    /// </summary>
    private static bool canAim = true;

    /// <summary>
    /// Allows or disables the cannons ability to shoot.
    /// </summary>
    private bool canShoot = true;
    #endregion
    #endregion

    #region Functions
    /// <summary>
    /// Initializes components.
    /// </summary>
    private void Awake()
    {
        currentVCam = transform.parent.gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
        mainCam = Camera.main;
        rb2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Resets the speed and angular velocity.
    /// </summary>
    private void FixedUpdate()
    {
        LimitSpeed();
        LimitAngularVelocity();
    }

    /// <summary>
    /// Limits how fast the player can move.
    /// </summary>
    private void LimitSpeed()
    {
        if (rb2d.velocity.magnitude > maxSpeed)
        {
            Vector2 newVel = rb2d.velocity.normalized * maxSpeed;
            rb2d.velocity = newVel;
        }
    }

    /// <summary>
    /// Limits the amount of rotation on the player.
    /// </summary>
    private void LimitAngularVelocity()
    {
        if (rb2d.angularVelocity > maxAngularVelocity)
        {
            rb2d.angularVelocity = maxAngularVelocity;
        }
    }

    /// <summary>
    /// Aims the cannon.
    /// </summary>
    /// <param name="input">The mouse position.</param>
    public void OnLook(InputValue input)
    {
        if (canAim)
        {
            // Calculates the angle the cannon should be
            Vector2 inputVec = input.Get<Vector2>();
            Vector3 aimDir = (mainCam.ScreenToWorldPoint(inputVec) - cannon.transform.position).normalized;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            cannon.transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    /// <summary>
    /// If the shoot key is pressed then a new character is shot out.
    /// </summary>
    public void OnShootPlayer()
    {
        if (canShoot)
        {
            // Disables cannons ability to shoot and aim
            canShoot = false;
            canAim = false;

            //Spawns cannon
            GameObject tempCannon = Instantiate(Resources.Load("Prefabs/Cannon and Camera", typeof(GameObject)), (Vector2)shootPos.transform.position, Quaternion.identity) as GameObject;
            tempCannon.transform.localScale = transform.parent.transform.localScale / 1.2f;

            // Gets direction cannon should be shot
            Vector2 shootDir = shootPos.transform.position - pivotPos.transform.position;
            shootDir.Normalize();

            // Adds forces to both cannons and changes the current camera
            rb2d.AddForce(-shootDir * shootPushBackForce);
            PlayerMovement tempPM = tempCannon.GetComponentInChildren<PlayerMovement>();
            tempPM.rb2d.velocity = shootDir * shootForce;
            tempPM.rb2d.AddTorque(shootTorque);
            tempPM.currentVCam.Priority = currentVCam.Priority + 1;
        }
    }

    /// <summary>
    /// Destroys objects once they go off of the screen.
    /// </summary>
    private void OnBecameInvisible()
    {
        Destroy(transform.parent.gameObject);
    }
    #endregion
}
