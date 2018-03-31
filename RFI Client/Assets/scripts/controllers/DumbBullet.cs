//----------------------------------------------------------------------------------------------------------------------
// A dumb bullet that just coasts along until it hits something, then explodes.
//----------------------------------------------------------------------------------------------------------------------

using UnityEngine;

//----------------------------------------------------------------------------------------------------------------------

public class DumbBullet : MonoBehaviour
{
    public GameObject explosion;

    //------------------------------------------------------------------------------------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        // Spawn an explosion
        Instantiate(this.explosion, collision.contacts[0].point, Quaternion.identity);

        // TODO: Apply damage to whatever we hit.

        // Destroy ourselves
        Destroy(this.gameObject);
    } // end OnCollisionEnter
} // end DumbBullet

//----------------------------------------------------------------------------------------------------------------------

