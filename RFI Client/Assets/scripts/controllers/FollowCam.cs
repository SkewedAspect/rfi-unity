// ----------------------------------------------------------------------------------------------------------------------
// Follow Camera script
// ----------------------------------------------------------------------------------------------------------------------

using UnityEngine;

// ----------------------------------------------------------------------------------------------------------------------

[System.Serializable]
public class FollowCam : MonoBehaviour
{
    public Transform target;                  // What the camera looks at. Generally the targeter.
    public FlightSimController control;       // The PlayerFlightControl script that is in play.

    public float followDistance = 3.0f;       // How far behind the camera will follow the targeter.
    public float cameraElevation = 3.0f;      // How high the camera will rise above the targeter's Z axis.

    public float followTightness = 5.0f;      // How closely the camera will follow the target. Higher values are snappier, lower results in a more lazy follow.

    public float rotationTightness = 10.0f;   // How closely the camera will react to rotations, similar to above.
    public float afterburnerShakeAmount = 2f; // How much the camera will shake when afterburners are active.

    public float yawMultiplier = 0.005f;      // Curbs the extremes of input. This should be a really small number. Might need to be tweaked, but do it as a last resort.
    public bool shakeOnAfterburn = true;      // The camera will shake when afterburners are active.

    public static FollowCam instance;         // The instance of this class. Should only be one.

    // -----------------------------------------------------------------------------------------------------------------
    // Unity API
    // -----------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        instance = this;
    } // end Awake

    private void FixedUpdate()
    {
        if(this.target == null)
        {
            Debug.LogError("(Flight Controls) Camera target is null!");
            return;
        } // end if

        if(this.control == null)
        {
            Debug.LogError("(Flight Controls) Flight controller is null on camera!");
            return;
        } // end if

        // Calculate where we want the camera to be.
        var newPosition = this.target.TransformPoint(
            this.control.yaw * this.yawMultiplier,
            this.cameraElevation, -this.followDistance
        );

        // Get the difference between the current location and the target's current location.
        var positionDifference = this.target.position - this.transform.position;

        // Move the camera towards the new position.
        this.transform.position = Vector3.Lerp(
            this.transform.position,
            newPosition,
            Time.deltaTime * this.followTightness
        );

        Quaternion newRotation;
        if(this.control.afterburnerActive && this.shakeOnAfterburn)
        {
            // Shake the camera while looking towards the targeter.
            newRotation = Quaternion.LookRotation(
                positionDifference + new Vector3(
                    Random.Range(-this.afterburnerShakeAmount, this.afterburnerShakeAmount),
                    Random.Range(-this.afterburnerShakeAmount, this.afterburnerShakeAmount),
                    Random.Range(-this.afterburnerShakeAmount,
                    this.afterburnerShakeAmount)
                ),
                this.target.up
            );
        }
        else
        {
            // Look towards the targeter
            newRotation = Quaternion.LookRotation(positionDifference, this.target.up);
        } // end if

        this.transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            newRotation,
            Time.deltaTime * this.rotationTightness
        );
    } // end FixedUpdate
} // end FollowCam
