using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Player : NetworkBehaviour
{

    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get
        {
            return _isDead;
        }

        protected set
        {
            _isDead = value;
        }
    }



    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectOnDeath;

    [SerializeField]
    private GameObject deathEffect;

    [SerializeField]
    private GameObject SpawnEffect;

    public void PlayerSetup()
    {
         
        CmdBroadCastNewPlayerSetup();
    }
    
    [Command]
    private void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerForAllClients();
    }

    [ClientRpc]

    private void RpcSetupPlayerForAllClients()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }

    // void update()
    //{
    //    if (!islocalplayer)
    //        return;

    //    if (input.getkeydown(keycode.k))
    //    {
    //        rpctakedamage(99999);

    //    }
    //}

    [ClientRpc]
    public void RpcTakeDamage(int amount)
    {
        if (_isDead)
            return;


        currentHealth -= amount;

        Debug.Log(transform.name + " now has " + currentHealth  + "health");    
        if(currentHealth <= 0)
        {
            Die();
        }


    }

    private void Die()
    {
        isDead = true;
        
        //Diable components 

        for(int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        //Disable gameobject 
        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(false);
        }


        //Disable the colider 
        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = false;

        //Spawn a death effect
        GameObject _gfxIns= (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);
       
        Debug.Log(transform.name + "is Dead");

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSetting.respawnTime);

        
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        
        SetDefaults();
        
        Debug.Log(transform.name + " respawnd.");

        //Creat SwapnEffect
        GameObject _gfxIn = (GameObject)Instantiate(SpawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIn, 3f);


    }


    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;


        //Enable the components 
        for(int i = 0; i < disableOnDeath.Length; i++ )
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        
        }

        //Enable the GameObjects
        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(true);
        }


        //Enable the collider
        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = true;

     
    }

   
   


}
