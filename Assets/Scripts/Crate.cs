using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour {

    private readonly float 
       RAYCAST_X_OFFSET = 0.85f, // 0.7f Half the length of the side of the player collision box parallel to the x axis.
       RAYCAST_Y_OFFSET = 0.85f; // Half the lenght of side parallel to the y axis.

    private bool falling = false;
    private BoxCollider2D thisCollider;
    private SpriteRenderer thisSR;
       
    // Use this for initialization
    void Start () {
        thisCollider = GetComponent<BoxCollider2D>();
        thisSR = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D pitsRayCast = Physics2D.Raycast(transform.position, Vector3.back, 0.0f, Player.pitsLayerMask);
        if (pitsRayCast.collider != null && pitsRayCast.collider.tag == "Pit")
        {
            falling = true;
        }

        if (falling & transform.localScale.x > 0.1f)
        {
            thisCollider.enabled = false;
            transform.localScale -= new Vector3(0.15f, 0.15f, 0.0f);
        } else if (falling)
        {
            thisSR.enabled = falling = false;
        }

    }

    public bool CanBeMoved(int playerRotation)
    {
        bool canBeMoved = true;
        RaycastHit2D[] playerHitInfo = new RaycastHit2D[3];

        switch (playerRotation)
        {
            case 0:
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, Player.obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, Player.obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(0, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, Player.obstaclesLayerMask);
                break;
            case 90:
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.left, 0.1f, Player.obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.left, 0.1f, Player.obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, 0), Vector2.left, 0.1f, Player.obstaclesLayerMask);
                break;
            case 180:
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, Player.obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, Player.obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(0, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, Player.obstaclesLayerMask);
                break;
            case -90:
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.right, 0.1f, Player.obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.right, 0.1f, Player.obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, 0), Vector2.right, 0.1f, Player.obstaclesLayerMask);
                break;
        }

        for (int i = 0; i < 3; i++)
        {
            if (playerHitInfo[i].collider != null)
            {
                canBeMoved &= false;                
            }
        }

        return canBeMoved;
    }
}
