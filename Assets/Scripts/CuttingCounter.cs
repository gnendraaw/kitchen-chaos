using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter {
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // there is no KitchenObject here
            if (player.HasKitchenObject()) {
                // player holding something 
                if (HasRecipeWithInput(player.GetKichenObject().GetKitchenObjectSO())) {
                    // player carrying something that can be cut
                    player.GetKichenObject().SetkitchenObjectParent(this);
                }
            } else {
                // player not have anything
            }
        } else {
            // there is KitchenObject here
            if (!player.HasKitchenObject()) {
                // player not have anything
                GetKichenObject().SetkitchenObjectParent(player);
            } else {
                // player carrying something
            }
        }
    }

    public override void InteractAlternate(Player player) {
        if (HasKitchenObject() && HasRecipeWithInput(GetKichenObject().GetKitchenObjectSO())) {
            // there is KitchenObject here && can be cut
            KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKichenObject().GetKitchenObjectSO());

            GetKichenObject().DestroySelf();

            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        } 
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) { 
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO.output;
            }
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) { 
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return true;
            }
        }
        return false;
    }
}

