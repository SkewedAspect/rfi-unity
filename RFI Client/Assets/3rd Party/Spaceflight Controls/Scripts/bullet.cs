using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour {
    public GameObject explo;

    void Start() {
    }

    void Update() {
    }

    void OnCollisionEnter(Collision coll) {
        // Spawn an explosion
        Instantiate(this.explo, coll.contacts[0].point, Quaternion.identity);

        // Destroy ourselves
        GameObject.Destroy(gameObject);
    } // end OnCollisionEnter
} // end DumbBullet
