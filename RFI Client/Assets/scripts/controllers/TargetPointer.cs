// ----------------------------------------------------------------------------------------------------------------------
// Custom Targeting Pointer
// ----------------------------------------------------------------------------------------------------------------------

using UnityEngine;

// ----------------------------------------------------------------------------------------------------------------------

[System.Serializable]
public class TargetPointer : MonoBehaviour
{
    public Texture pointerTexture;                 // The image for the pointer, generally a crosshair or dot.
    public bool useMouseInput = false;             // Pointer will be controlled by the mouse.
    public bool useGamepadInput = false;           // Pointer will be controlled by a joystick
    public bool pointerReturnsToCenter = false;    // Pointer will drift to the center of the screen (Use this for joysticks)
    public bool instantSnapping = false;           // If the pointer returns to the center, this will make it return to the center instantly when input is idle. Only works for joysticks
    public float centerSpeed = 5f;                 // How fast the pointer returns to the center.
    public bool centerLock = false;                // Pointer graphic will be locked to the center. Also affects shooting raycast (always shoots to the center of the screen)
    public bool invertYAxis = false;               // Inverts the y axis.

    public float deadzoneRadius = 0f;              // Deadzone in the center of the screen where the pointer can move without affecting the ship's movement.

    public float thumbstickSpeedModifier = 1f;     // Speed multiplier for joysticks.
    public float mouseSensitivityModifier = 15f;   // Speed multiplier for the mouse.

    public static Vector2 pointerPosition;         // Position of the pointer in screen coordinates.

    [HideInInspector]
    public Rect deadzoneRect;                      // Rect representation of the deadzone.

    public static TargetPointer instance;          // The instance of this class (Should only be one)

    // ------------------------------------------------------------------------------------------------------------------
    // Unity API
    // ------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        // Set pointer position to center of screen
        pointerPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        instance = this;
    } // end Awake

    private void Start()
    {
        // Lock the Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.deadzoneRect = new Rect(
            (Screen.width / 2f) - this.deadzoneRadius,
            (Screen.height / 2f) - this.deadzoneRadius,
            this.deadzoneRadius * 2,
            this.deadzoneRadius * 2
        );

        if(this.pointerTexture == null)
        {
            Debug.LogWarning("(FlightControls) Warning: No texture set for the custom pointer!");
        } // end if

        if(!this.useMouseInput && !this.useGamepadInput)
        {
            Debug.LogError("(FlightControls) No input method selected! See the Custom Pointer script on the Main Camera and select either mouse or gamepad.");
        } // end if
    } // end Start

    private void Update()
    {
        if(this.useMouseInput)
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } // end if

            if(Input.GetKey(KeyCode.C))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } // end if

            var xAxis = Input.GetAxis("Mouse X");
            var yAxis = Input.GetAxis("Mouse Y");

            // If the Cursor is visible, we reset the target to 0, 0.
            if(Cursor.visible)
            {
                xAxis = 0f;
                yAxis = 0f;
            } // end if

            if(this.invertYAxis)
            {
                yAxis = -yAxis;
            } // end if

            // Add the input to the pointer's position
            pointerPosition += new Vector2(
                xAxis * this.mouseSensitivityModifier,
                yAxis * this.mouseSensitivityModifier
            );
        }
        else if(this.useGamepadInput)
        {
            var xAxis = Input.GetAxis("Horizontal");
            var yAxis = Input.GetAxis("Vertical");

            if(this.invertYAxis)
            {
                yAxis = -yAxis;
            } // end if

            pointerPosition += new Vector2(
                xAxis * this.thumbstickSpeedModifier * Mathf.Pow(Input.GetAxis("Horizontal"), 2),
                yAxis * this.thumbstickSpeedModifier * Mathf.Pow(Input.GetAxis("Vertical"), 2)
            );
        } // end if

        // If the pointer returns to the center of the screen and it's not in the deadzone...
        if(this.pointerReturnsToCenter && !this.deadzoneRect.Contains(pointerPosition))
        {
            // If there's no input and instant snapping is on...
            if(Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0 && this.instantSnapping)
            {
                pointerPosition = new Vector2(Screen.width / 2f, Screen.height / 2f); // Place pointer at the center.
            }
            else
            {
                // Move pointer to the center (Will stop when it hits the deadzone)
                pointerPosition.x = Mathf.Lerp(pointerPosition.x, Screen.width / 2f, this.centerSpeed * Time.deltaTime);
                pointerPosition.y = Mathf.Lerp(pointerPosition.y, Screen.height / 2f, this.centerSpeed * Time.deltaTime);
            } // end if
        } // end if

        // Keep the pointer within the bounds of the screen.
        pointerPosition.x = Mathf.Clamp(pointerPosition.x, 0, Screen.width);
        pointerPosition.y = Mathf.Clamp(pointerPosition.y, 0, Screen.height);
    } // end Update

    private void OnGUI()
    {
        // Draw the pointer texture.
        if(this.pointerTexture != null && !this.centerLock)
        {
            GUI.DrawTexture(
                new Rect(
                    pointerPosition.x - (this.pointerTexture.width / 2f),
                    Screen.height - pointerPosition.y - (this.pointerTexture.height / 2f),
                    this.pointerTexture.width,
                    this.pointerTexture.height
                ),
                this.pointerTexture
            );
        }
        else
        {
            GUI.DrawTexture(
                new Rect(
                    (Screen.width / 2f) - (this.pointerTexture.width / 2f),
                    (Screen.height / 2f) - (this.pointerTexture.height / 2f),
                    this.pointerTexture.width,
                    this.pointerTexture.height
                ),
                this.pointerTexture
            );
        } // end if
    } // end OnGUI
} // end TargetPointer
