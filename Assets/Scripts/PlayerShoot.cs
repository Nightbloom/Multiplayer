using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";
    

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;
  
    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;


     void Start()
    {
        if(cam == null)
        {
            Debug.LogError("PlayerShoot: No camera reference!");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

     void Update()
    {

        currentWeapon = weaponManager.GetCurrentWeapon();
        
        
        if(currentWeapon.fireRate <= 0f)
        {
           if(Input.GetButtonDown("Fire1"))
           {
            Shoot();
           }

        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);

            }
            else if(Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");

            }
        }


    }

    //Id called on the server when  a player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }


    //Is called on all clients when we need to a 
    //a shoot effect
    [ClientRpc]
    void RpcDoShootEffect()
    {
       
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    //Is called on the server when we hit something 
    //Takes in the hit point ans the normal of the surface
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }
    
    //Is called on a;; the clients 
    //Here we can spawn in cool effects
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 1f);
    }

    [Client]
    void Shoot()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        // We are shotting, call the OnShoot methode  on the server 
        CmdOnShoot();

        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.range, mask))
        {
            if(hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(hit.collider.name, currentWeapon.damage);
            }
            //We hit something, call the Onhit method on the server
            CmdOnHit(hit.point, hit.normal);
        }

    }
    
    [Command]
    void CmdPlayerShot(string playerID, int _damage)
    {
        Debug.Log(playerID + "Has been shot");

        Player player = GameManager.GetPlayer(playerID);
        player.RpcTakeDamage(_damage);
    }


}
