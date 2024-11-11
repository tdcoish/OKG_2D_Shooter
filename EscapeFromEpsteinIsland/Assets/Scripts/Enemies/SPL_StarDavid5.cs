/*******
Classic enclosing pattern. All six emitters fire at the player, but since they shoot out four streams,
the player doesn't need to move at all to dodge them, but rather stay in the pocket for a while.

Not sure this should be aimed at all. I kind of like this as something for an open fight, where the 
player can figure out where the pockets of safety are.

Ideally, this should flash before it fires.
*******/
using UnityEngine;
using System.Collections.Generic;

public class SPL_StarDavid5 : MonoBehaviour
{
    public SpriteRenderer           rSprite;

    public List<WP_DavidOrb>        rEmitters;
    public float                    _salvoRate = 1f;
    public float                    mSalvoTmStmp;
    public float                    _fireRate = 0.05f;
    public float                    mFireTmStmp;
    public int                      _shotsPerSalvo = 10;
    public int                      mShotCounter = 0;
    public float                    _shotSpread = 2f;
    
    public PJ_EN_Plasmoid           PF_Bullet;

    void Update()
    {
        PC_Cont rPC = FindObjectOfType<PC_Cont>();
        if(rPC == null) return;

        void ShootBullet(Vector2 emitterPos, Vector2 pcPos, float angleOffset)
        {
            Vector2 vDirToPC = (pcPos - emitterPos).normalized;
            PJ_EN_Plasmoid p = Instantiate(PF_Bullet, emitterPos, transform.rotation);
            float radiansOfTurn = angleOffset*Mathf.Deg2Rad;
            Vector2 vDir = vDirToPC;
            vDir.x = vDirToPC.x * Mathf.Cos(radiansOfTurn) - vDir.y * Mathf.Sin(radiansOfTurn);
            vDir.y = vDirToPC.x * Mathf.Sin(radiansOfTurn) + vDir.y * Mathf.Cos(radiansOfTurn);
            p.cRigid.velocity = vDir.normalized * p.mProjD._spd;
            p.transform.up = p.cRigid.velocity.normalized;
            p.mProjD.rOwner = gameObject;
        }

        if(Time.time - mSalvoTmStmp > _salvoRate * 0.6f){
            rSprite.color = new Color(1f, 0f, 0f, 1f);
        }else{
            rSprite.color = new Color(1f, 1f, 1f, 1f);
        }

        if(Time.time - mSalvoTmStmp > _salvoRate){
            if(Time.time - mFireTmStmp > _fireRate){
                for(int i=0; i<rEmitters.Count; i++){
                    ShootBullet(rEmitters[i].transform.position, rPC.transform.position, _shotSpread * 2f);
                    ShootBullet(rEmitters[i].transform.position, rPC.transform.position, _shotSpread);
                    ShootBullet(rEmitters[i].transform.position, rPC.transform.position, -_shotSpread);
                    ShootBullet(rEmitters[i].transform.position, rPC.transform.position, -_shotSpread * 2f);
                }
                mShotCounter++;
                mFireTmStmp = Time.time;
                if(mShotCounter >= _shotsPerSalvo){
                    mSalvoTmStmp = Time.time;
                    mShotCounter = 0;
                }
            }
        }
    }
}
