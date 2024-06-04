/*****************
Has at least two shot points, then fires in the direction of the player, except biased so the shots 
cross paths a certain distance from the player. 
*****************/
using UnityEngine;

public class SPL_CrossAiming : MonoBehaviour
{
    public float                    _spacing = 0.5f;
    public float                    _shotCrossDistance = 2f;
    public float                    _fireInterval = 1f;
    public float                    mFireTmStmp;
    public PJ_EN_Plasmoid           PF_Bullet;

    public void Update()
    {
        PC_Cont rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null) return;

        transform.up = (rPC.transform.position - transform.position).normalized;

        if(Time.time - mFireTmStmp > _fireInterval){

            void ShootBullet(Vector3 vShiftDir, float spacingMultiplier = 1f)
            {
                PJ_EN_Plasmoid p = Instantiate(PF_Bullet, transform.position, transform.rotation);
                // shift p over by a certain distance. 
                p.transform.position += vShiftDir.normalized * _spacing * spacingMultiplier;

                Vector2 aimPos = transform.position + (rPC.transform.position - transform.position).normalized * _shotCrossDistance;
                p.FShootAt(aimPos, p.transform.position, gameObject);
            }

            ShootBullet(transform.right, 4f);
            ShootBullet(transform.right, 3f);
            ShootBullet(transform.right, 2f);
            ShootBullet(transform.right);
            ShootBullet(transform.right*-1f);
            ShootBullet(transform.right*-1f, 2f);
            ShootBullet(transform.right*-1f, 3f);
            ShootBullet(transform.right*-1f, 4f);

            mFireTmStmp = Time.time;
        }
    }
}
