using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";
    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;
    private GameObject playerUIInstance;


    Camera sceneCamera;

    void Start()
    {
        // Disable components that should only be
        // activate on the player we control 
        if(!isLocalPlayer)
        {
            DisableCompnents();
            AssingnRemoteLayer();
        }
        else
        {
            // we are the local player: Disable the scene camera
            sceneCamera = Camera.main;
            if(sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }

            // Disable player graphics for local player
            setLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            //Create PlayerUI
            playerUIInstance =  Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;


            // Configure PlayerUI

            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui == null)
                Debug.LogError("No PlayerUI component on PlayerUI prefab.");
            ui.SetController(GetComponent<PlayerController>());

            GetComponent<Player>().PlayerSetup();
        }

        
    }

    void setLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            setLayerRecursively(child.gameObject, newLayer);
        }


    }


    public override void OnStartClient()
    {
        base.OnStartClient();

        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player player = GetComponent<Player>();

        GameManager.ReisterPlayer(netID, player);
    }



    void AssingnRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);


    }

    void DisableCompnents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }

    }

    // When we are destored
    void OnDisable()
    {

        Destroy(playerUIInstance);
        // Re-enable the scene camera
        if(sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(false);
        }

        GameManager.UnRegisterPlayer(transform.name);

    }
}
