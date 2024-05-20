/*******
Periodically radiate bullets from the center of the player.
********/
using UnityEngine;

public class PC_RadiatingShots : MonoBehaviour
{
    public PJ_PC_RadShot                PF_Projectile;
    public float                        _timeBetweenShots = 2f;
    public float                        mShotTmStmp;

    void Update()
    {
        void ShootProjectile(float xDir, float yDir)
        {
            Vector2 vDir = new Vector2(xDir, yDir);
            vDir = vDir.normalized;       // just in case.
            // Have an offset from the player.
            Vector2 startPos = transform.position;
            startPos += vDir*0.1f;
            PJ_PC_RadShot temp = Instantiate(PF_Projectile, startPos, transform.rotation);
            temp.FShootAt(vDir, gameObject);
            // Make the shots velocity track with the player.
            temp.cRigid.velocity += GetComponent<Rigidbody2D>().velocity;
            temp.mCreatedTmStmp = Time.time;
            temp.mProjD._spd = temp.cRigid.velocity.magnitude;
        }

        if(Time.time - mShotTmStmp > _timeBetweenShots){
            mShotTmStmp = Time.time;
            ShootProjectile(0f, 1f);
            ShootProjectile(1f, 1f);
            ShootProjectile(-1f, 1f);
            ShootProjectile(1f, 0f);
            ShootProjectile(-1f, 0f);
            ShootProjectile(0f, -1f);
            ShootProjectile(1f, -1f);
            ShootProjectile(-1f, -1f);
        }
    }
}
