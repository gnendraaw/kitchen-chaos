using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter {
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO platesKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    private int platesSpawnAmount;
    private int platesSpawnAmountMax = 4;

    private void Update() {
        if (!IsServer) {
            return;
        }

        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax) {
            spawnPlateTimer = 0f;

            if (KitchenGameManager.Instance.IsGamePlaying() && platesSpawnAmount < platesSpawnAmountMax) {
                SpawnNewPlateServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnNewPlateServerRpc() {
        SpawnNewPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnNewPlateClientRpc() {
        platesSpawnAmount++;

        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player) {
        if (!player.HasKitchenObject()) {
            // player is empty handed
            if (platesSpawnAmount > 0) {
                // there is at least 1 plate here
                KitchenObject.SpawnKitchenObject(platesKitchenObjectSO, player);

                InteractLogicServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc() {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc() {
        platesSpawnAmount--;

        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
    
}

