using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress {
    public static event EventHandler OnAnyCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProgress;

    new public static void ClearStaticData() {
        OnAnyCut = null;
    }

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // there is no KitchenObject here
            if (player.HasKitchenObject()) {
                // player holding something 
                if (HasRecipeWithInput(player.GetKichenObject().GetKitchenObjectSO())) {
                    // player carrying something that can be cut
                    KitchenObject kitchenObject = player.GetKichenObject();
                    kitchenObject.SetkitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            } else {
                // player not have anything
            }
        } else {
            // there is KitchenObject here
            if (!player.HasKitchenObject()) {
                // player not have anything
                GetKichenObject().SetkitchenObjectParent(player);

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
                    });
            } else {
                // player carrying something
                if (player.GetKichenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    // player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKichenObject().GetKitchenObjectSO())) {
                        GetKichenObject().DestroySelf();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc() {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc() {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });
    }

    public override void InteractAlternate(Player player) {
        CutObjectServerRpc();
        TestCuttingProgressDoneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc() {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc() {
        if (HasKitchenObject() && HasRecipeWithInput(GetKichenObject().GetKitchenObjectSO())) {
            // there is KitchenObject here && can be cut
            cuttingProgress++;

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKichenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });

        } 
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc() {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKichenObject().GetKitchenObjectSO());

        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
            KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKichenObject().GetKitchenObjectSO());

            KitchenObject.DestroyKitchenObject(GetKichenObject());

            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        return cuttingRecipeSO != null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) { 
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}

