/*************************************************************************************
Manager for the turret test level, where I need to recognize if things have died.
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LVL_Man : MonoBehaviour
{
    // Ideally we would be spawning things in.
    public EN_PlasmaTurret                      PF_PlasmaTurret;
    public EN_GrenadeTurret                     PF_GrenadeTurret;
    public EN_NeedlerTurret                     PF_NeedlerTurret;
    public EN_Sniper                            PF_Sniper;

    public GameObject                           rPlasmaTurretSpawnLoc;
    public GameObject                           rGrenadeTurretSpawnLoc;
    public GameObject                           rNeedlerTurretSpawnLoc;
    public GameObject                           rSniperSpawnLoc;

    public List<GameObject>                     rSpawnedEnemies;


    // For now, just flat out spawn everything in on Start.
    void Start()
    {
        rSpawnedEnemies = new List<GameObject>();
        {
            EN_PlasmaTurret p = Instantiate(PF_PlasmaTurret, rPlasmaTurretSpawnLoc.transform.position, transform.rotation);
            rSpawnedEnemies.Add(p.gameObject);
        }
        {
            EN_GrenadeTurret p = Instantiate(PF_GrenadeTurret, rGrenadeTurretSpawnLoc.transform.position, transform.rotation);
            rSpawnedEnemies.Add(p.gameObject);
        }
        {
            EN_NeedlerTurret p = Instantiate(PF_NeedlerTurret, rNeedlerTurretSpawnLoc.transform.position, transform.rotation);
            rSpawnedEnemies.Add(p.gameObject);
        }
        {
            EN_Sniper p = Instantiate(PF_Sniper, rSniperSpawnLoc.transform.position, transform.rotation);
            rSpawnedEnemies.Add(p.gameObject);
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.K))
        {
            SceneManager.LoadScene("SN_MN_Main");
        }
    }

    public void FHandleEnemyKilled(GameObject p)
    {
        if(p == null){
            Debug.Log("Null game object");
            return;
        }

        rSpawnedEnemies.Remove(p);

        Debug.Log("Enemy killed, " + rSpawnedEnemies.Count + " remaining");

        if(rSpawnedEnemies.Count == 0){
            Debug.Log("Congrats, you Win!");
            SceneManager.LoadScene("SN_MN_Main");
        }
    }
}
