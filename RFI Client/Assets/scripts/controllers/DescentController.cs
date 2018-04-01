//----------------------------------------------------------------------------------------------------------------------
// A "Descent" Style FlightSim Controller, liberally "borrowed" from Reddit.
//----------------------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;

//----------------------------------------------------------------------------------------------------------------------

namespace controllers
{
    public class DescentController : MonoBehaviour
    {
        //--------------------------------------------------------------------------------------------------------------

        public float DragCoef = 0.5f;

        // Sensitivity values by axis
        public float PitchAxisSens = 1.0f;
        public float YawAxisSens = 1.0f;
        public float RollAxisSens = 1.0f;

        // Maximum values to apply
        public float PitchMax = 45.0f;
        public float YawMax = 45.0f;
        public float RollMax = 45.0f;

        // Axis is between -1 and 1, then multiplied by sensitivity.
        private float _pitchAxis = 0.0f;
        private float _yawAxis = 0.0f;
        private float _rollAxis = 0.0f;

        private Rigidbody _body;

        //--------------------------------------------------------------------------------------------------------------
        // Unity API
        //--------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            this._body = this.GetComponent<Rigidbody>();
        } // end start

        private void FixedUpdate()
        {
            // Reset per frame
            this._pitchAxis = 0.0f;
            this._yawAxis = 0.0f;
            this._rollAxis = 0.0f;

            var pitchInput = false;
            var yawInput = false;
            var rollInput = false;

//            var localAngularVelocity = this.transform.InverseTransformDirection(this._body.angularVelocity);

            //----------------------------------------------------------------------------------------------------------
            // Handle Input
            //----------------------------------------------------------------------------------------------------------

            if(Input.GetKey(KeyCode.UpArrow))
            {
                this._pitchAxis = 0.75f;
                pitchInput = true;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                this._pitchAxis = -0.75f;
                pitchInput = true;
            }
            else
            {
//                this._pitchAxis = -localAngularVelocity.y * this.PitchAxisSens * this.DragCoef;
            } // end if

            if(Input.GetKey(KeyCode.LeftArrow))
            {
                this._yawAxis = -0.75f;
                yawInput = true;
            }
            else if(Input.GetKey(KeyCode.RightArrow))
            {
                this._yawAxis = 0.75f;
                yawInput = true;
            }
            else
            {
//                this._yawAxis = -localAngularVelocity.z * this.YawAxisSens * this.DragCoef;
            } // end if

            if(Input.GetKey(KeyCode.A))
            {
                this._rollAxis = 0.75f;
                rollInput = true;
            }
            else if(Input.GetKey(KeyCode.D))
            {
                this._rollAxis = -0.75f;
                rollInput = true;
            }
            else
            {
//                this._rollAxis = -localAngularVelocity.x * this.RollAxisSens * this.DragCoef;
            } // end if

            //----------------------------------------------------------------------------------------------------------
            // Zero out velocity if we're getting close
            //----------------------------------------------------------------------------------------------------------

            var angularVelX = this._body.angularVelocity.x;
            if(!rollInput && Math.Abs(angularVelX) < 0.01)
            {
                this._rollAxis = 0;
                angularVelX = 0;
            } // end if

            var angularVelY = this._body.angularVelocity.y;
            if(!pitchInput && Math.Abs(angularVelY) < 0.01)
            {
                this._pitchAxis = 0;
                angularVelY = 0;
            } // end if

            var angularVelZ = this._body.angularVelocity.z;
            if(!yawInput && Math.Abs(angularVelZ) < 0.01)
            {
                this._yawAxis = 0;
                angularVelZ = 0;
            } // end if

            this._body.angularVelocity = new Vector3(angularVelX, angularVelY, angularVelZ);

            //----------------------------------------------------------------------------------------------------------

            var pitch = this._pitchAxis * this.PitchAxisSens;
            var yaw = this._yawAxis * this.YawAxisSens;
            var roll = this._rollAxis * this.RollAxisSens;

            // Apply Rotation forces
            this._body.AddRelativeTorque(
                roll * this.RollMax * Time.deltaTime,
                pitch * this.PitchMax * Time.deltaTime,
                yaw * this.YawMax * Time.deltaTime
            );
        } // end Update
    } // end DescentController
} // end controllers

//----------------------------------------------------------------------------------------------------------------------
