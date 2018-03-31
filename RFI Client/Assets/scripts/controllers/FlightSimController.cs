//----------------------------------------------------------------------------------------------------------------------
// A Mouse look "Freelancer" style controller, "borrowed" from the Unity Asset store, and cleaned up.
//----------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------------------------------------------------

[Serializable]
public class FlightSimController : MonoBehaviour
{
    // "Objects", "For the main ship Game Object and weapons";
    public GameObject actualModel;            // "Ship GameObject", "Point this to the Game Object that actually contains the mesh for the ship. Generally, this is the first child of the empty container object this controller is placed in."
    public Transform weaponHardpoint1;        // "Weapon Hardpoint", "Transform for the barrel of the weapon"
    public GameObject bullet;                 // "Projectile GameObject", "Projectile that will be fired from the weapon hardpoint."

    // "Core Movement", "Controls for the various speeds for different operations."
    public float speed = 20.0f;               // "Base Speed", "Primary flight speed, without afterburners or brakes"
    public float afterburnerSpeed = 40f;      // Afterburner Speed", "Speed when the button for positive thrust is being held down"
    public float slowSpeed = 4f;              // "Brake Speed", "Speed when the button for negative thrust is being held down"
    public float thrustTransitionSpeed = 5f;  // Thrust Transition Speed", "How quickly afterburners/brakes will reach their maximum effect"
    public float turnspeed = 15.0f;           // "Turn/Roll Speed", "How fast turns and rolls will be executed "
    public float rollSpeedModifier = 7;       // "Roll Speed", "Multiplier for roll speed. Base roll is determined by turn speed"
    public float pitchYawStrength = 0.5f;     // "Pitch/Yaw Multiplier", "Controls the intensity of pitch and yaw inputs"

    // "Banking", "Visuals only--has no effect on actual movement"
    public bool useBanking = true;            // Will bank during turns. Disable for first-person mode, otherwise should generally be kept on because it looks cool. Your call, though.
    public float bankAngleClamp = 360;        // "Bank Angle Clamp", "Maximum angle the spacecraft can rotate along the Z axis."
    public float bankRotationSpeed = 3f;      // "Bank Rotation Speed", "Rotation speed along the Z axis when yaw is applied. Higher values will result in snappier banking."
    public float bankRotationMultiplier = 1f; // "Bank Rotation Multiplier", "Bank amount along the Z axis when yaw is applied."
    public float screenClamp = 500;           // "Screen Clamp (Pixels)", "Once the pointer is more than this many pixels from the center, the input in that direction(s) will be treated as the maximum value."

    // Inputs for roll, yaw, and pitch, taken from Unity's input system.
    [HideInInspector]
    public float roll, yaw, pitch;

    // True if afterburners are on.
    [HideInInspector]
    public bool afterburnerActive = false;

    // True if brakes are on
    [HideInInspector]
    public bool slowActive = false;

    // Distance in pixels from the vertical center of the screen.
    private float _distFromVertical;

    // Distance in pixel from the horizontal center of the screen.
    private float _distFromHorizontal;

    // Pointer position from TargetPointer
    private Vector2 _mousePos = new Vector2(0, 0);

    // Deadzone, taken from TargetPointer.
    private float _dz = 0;

    // Current speed/magnitude
    private float _currentMag = 0f;

    private bool _thrustExists = true;
    private bool _rollExists = true;

    // UI Crap
    public Slider ThrottleSlider;
    public Text ThrottleText;

    //------------------------------------------------------------------------------------------------------------------
    // Internal
    //------------------------------------------------------------------------------------------------------------------

    private void _updateCursorPosition()
    {
        this._mousePos = TargetPointer.pointerPosition;

        // Calculate distances from the center of the screen.
        float distV = Vector2.Distance(this._mousePos, new Vector2(this._mousePos.x, Screen.height / 2f));
        float distH = Vector2.Distance(this._mousePos, new Vector2(Screen.width / 2f, this._mousePos.y));

        // If the distances are less than the deadzone, then we want it to default to 0 so that no movements will occur.
        if(Mathf.Abs(distV) < this._dz)
        {
            distV = 0;
        }
        else
        {
            distV -= this._dz;
        } // end if

        // Subtracting the deadzone from the distance. If we didn't do this, there would be a snap as it tries to go to from 0 to the end of the deadzone, resulting in jerky movement.
        if(Mathf.Abs(distH) < this._dz)
        {
            distH = 0;
        }
        else
        {
            distH -= this._dz;
        } // end if

        // Clamping distances to the screen bounds.
        this._distFromVertical = Mathf.Clamp(distV, 0, Screen.height);
        this._distFromHorizontal = Mathf.Clamp(distH, 0, Screen.width);

        // If the mouse position is to the left, then we want the distance to go negative so it'll move left.
        if(this._mousePos.x < Screen.width / 2f && this._distFromHorizontal != 0)
        {
            this._distFromHorizontal *= -1;
        } // end if

        // If the mouse position is above the center, then we want the distance to go negative so it'll move upwards.
        if(this._mousePos.y >= Screen.height / 2f && this._distFromVertical != 0)
        {
            this._distFromVertical *= -1;
        } // end if
    } // end updateCursorPosition

    private void _updateBanking()
    {
        // Load rotation information.
        Quaternion newRotation = this.transform.rotation;
        Vector3 newEulerAngles = newRotation.eulerAngles;

        // Basically, we're just making it bank a little in the direction that it's turning.
        newEulerAngles.z += Mathf.Clamp(
            -this.yaw * this.turnspeed * Time.deltaTime * this.bankRotationMultiplier,
            -this.bankAngleClamp,
             this.bankAngleClamp
        );

        newRotation.eulerAngles = newEulerAngles;

        // Apply the rotation to the gameobject that contains the model.
        this.actualModel.transform.rotation = Quaternion.Slerp(this.actualModel.transform.rotation,
            newRotation, this.bankRotationSpeed * Time.deltaTime
        );
    } // end updateBraking

    public void FireShot()
    {
        if(this.weaponHardpoint1 == null)
        {
            Debug.LogError("(FlightControls) Trying to fire weapon, but no hardpoint set up!");
            return;
        } // end if

        if(this.bullet == null)
        {
            Debug.LogError("(FlightControls) Bullet GameObject is null!");
            return;
        } // end if

        // Shoots it in the direction that the pointer is pointing. Might want to take note of this line for when you
        // upgrade the shooting system.
        if(Camera.main == null)
        {
            Debug.LogError(
                "(FlightControls) Main camera is null! Make sure the flight camera has the tag of MainCamera!");
            return;
        } // end if

        GameObject shot1 = Instantiate(this.bullet, this.weaponHardpoint1.position, Quaternion.identity);

        Ray vRay;
        if(!TargetPointer.instance.centerLock)
        {
            vRay = Camera.main.ScreenPointToRay(TargetPointer.pointerPosition);
        }
        else
        {
            vRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        } // end if

        // If we make contact with something in the world, we'll make the shot actually go to that point.
        RaycastHit hit;
        if(Physics.Raycast(vRay, out hit))
        {
            shot1.transform.LookAt(hit.point);
            shot1.GetComponent<Rigidbody>().AddForce(shot1.transform.forward * 90000f);

        }
        else
        {
            // Otherwise, since the ray didn't hit anything, we're just going to guess and shoot the projectile in the
            // general direction.
            shot1.GetComponent<Rigidbody>().AddForce(vRay.direction * 90000f);
        } // end if
    } // end fireShot

    //------------------------------------------------------------------------------------------------------------------
    // Unity API
    //------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        this._mousePos = new Vector2(0, 0);
        this._dz = TargetPointer.instance.deadzoneRadius;

        // Setting this equal to 0 here as a failsafe in case the roll axis is not set up.
        this.roll = 0;

        // Error handling, in case one of the inputs aren't set up.
        try
        {
            Input.GetAxis("Thrust");
        }
        catch
        {
            this._thrustExists = false;
            Debug.LogError(
                "(Flight Controls) Thrust input axis not set up! Go to \"Edit > Project Settings > Input\" to create a new axis called 'Thrust' so the ship can change speeds.");
        } // end try/catch

        try
        {
            Input.GetAxis("Roll");
        }
        catch
        {
            this._rollExists = false;
            Debug.LogError(
                "(Flight Controls) Roll input axis not set up! Go to \"Edit > Project Settings > Input\" to create a new axis called 'Roll' so the ship can roll.");
        } // end try/catch
    } // end Start

    void FixedUpdate()
    {
        if(this.actualModel == null)
        {
            Debug.LogError("(FlightControls) Ship GameObject is null.");
            return;
        } // end if

        this._updateCursorPosition();

        // Clamping the pitch and yaw values, and taking in the roll input.
        this.pitch = Mathf.Clamp(this._distFromVertical, -this.screenClamp - this._dz, this.screenClamp + this._dz) * this.pitchYawStrength;
        this.yaw = Mathf.Clamp(this._distFromHorizontal, -this.screenClamp - this._dz, this.screenClamp + this._dz) * this.pitchYawStrength;
        if(this._rollExists) this.roll = (Input.GetAxis("Roll") * -this.rollSpeedModifier);

        // Getting the current speed.
        this._currentMag = this.GetComponent<Rigidbody>().velocity.magnitude;

        // If input on the thrust axis is positive, activate afterburners.
        if(this._thrustExists)
        {
            if(Input.GetAxis("Thrust") > 0)
            {
                this.afterburnerActive = true;
                this.slowActive = false;
                this._currentMag = Mathf.Lerp(this._currentMag, this.afterburnerSpeed, this.thrustTransitionSpeed * Time.deltaTime);
            }
            else if(Input.GetAxis("Thrust") < 0)
            {
                // If input on the thrust axis is negatve, activate brakes.
                this.slowActive = true;
                this.afterburnerActive = false;
                this._currentMag = Mathf.Lerp(this._currentMag, this.slowSpeed, this.thrustTransitionSpeed * Time.deltaTime);
            }
            else
            {
                // Otherwise, hold normal speed.
                this.slowActive = false;
                this.afterburnerActive = false;
                this._currentMag = Mathf.Lerp(this._currentMag, this.speed, this.thrustTransitionSpeed * Time.deltaTime);
            } // end if
        } // end if

        // Update throttle slider
        this.ThrottleSlider.maxValue = 1;
        this.ThrottleSlider.minValue = -1;
        this.ThrottleSlider.value = Input.GetAxis("Thrust");
        this.ThrottleText.text = string.Format("Throttle: {0}%", Math.Round((Input.GetAxis("Thrust") + 1) / 2 * 100));

        // Apply all these values to the rigidbody on the container.
        this.GetComponent<Rigidbody>().AddRelativeTorque(
            this.pitch * this.turnspeed * Time.deltaTime,
            this.yaw * this.turnspeed * Time.deltaTime,
            this.roll * this.turnspeed * (this.rollSpeedModifier / 2) * Time.deltaTime
        );

        // Apply speed
        this.GetComponent<Rigidbody>().velocity = this.transform.forward * this._currentMag;

        // Calculate banking.
        if(this.useBanking)
        {
            this._updateBanking();
        } // end if
    } // end FixedUpdate

    void Update()
    {
        if(Input.GetKey(KeyCode.F10))
        {
            Application.Quit();
        } // end if

        // Please remove this and replace it with a shooting system that works for your game, if you need one.
        if(Input.GetMouseButtonDown(0))
        {
            this.FireShot();
        } // end if
    } // end Update
} // end FlightSimController

//----------------------------------------------------------------------------------------------------------------------
