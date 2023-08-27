using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************
I've realized that I need to have a very basic enemy with little AI to figure things out 
so I can test level concepts, design, and pathfinding.

I'm wondering whether the rejection force concept is a good one. Sure, it works for
pathfinding, but ultimately we want the enemies to have a concept of the rocks. This
conception includes potentially line of sight blockage, and maneuvering to fire projectiles.

For instance, what if the hunter leaps into the rocks?

Hold that thought. What if we just built the combat maps as a grid of things. The enemies
can move with less space taken, but when they need to pathfind they easily use A* to get to 
the right place. This also lets us trivially use A*, since we never have to place grid spots,
because it can be assumed that anyplace without a wall, or rock emplacement is valid to move
to.
*********************************************/

public class TEST_BasicEnemy : MonoBehaviour
{
    public PC_Cont                  rPC;

    public float                    _movSpd = 3f;
    public bool                     mMoveTo = false;
    public Vector2                  mSpot;

    public float                    _rejectionForce = 1f;

    public Rigidbody2D              cRigid;
    

    // Start is called before the first frame update
    void Start()
    {
        rPC = FindObjectOfType<PC_Cont>();
        mSpot = new Vector2(0f,0f);
        cRigid = GetComponent<Rigidbody2D>();
        cRigid.velocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Hit something");
        if(col.GetComponent<ENV_Wall>()){
            Debug.Log("That something was a wall");
        }
    }

}
