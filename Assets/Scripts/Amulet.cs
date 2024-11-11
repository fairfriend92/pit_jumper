using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amulet : MonoBehaviour {

    private LayerMask pitsLayerMask;

	// Use this for initialization
	void Start () {
        pitsLayerMask = LayerMask.GetMask("Pit");
    }

    // Update is called once per frame
    void Update () {
        RaycastHit2D pitsRayCast = Physics2D.Raycast(transform.position, Vector3.back, 0.0f, pitsLayerMask);

        if (pitsRayCast.collider != null && pitsRayCast.collider.tag != "Trap")
        {
            if (transform.localScale.x > 0.1f)
            {
                transform.localScale -= new Vector3(0.15f, 0.15f, 0.0f);
            } else
            {
                this.enabled = false;
            }
        }
    }
}
