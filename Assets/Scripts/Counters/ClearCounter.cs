using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // there is no KitchenObject here
            if (player.HasKitchenObject()) {
                // player holding something 
                player.GetKichenObject().SetkitchenObjectParent(this);
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
                if (player.GetKichenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    // player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKichenObject().GetKitchenObjectSO())) {
                        GetKichenObject().DestroySelf();
                    }
                } else {
                    // player is not carrying Plate but something else 
                    if (GetKichenObject().TryGetPlate(out plateKitchenObject)) {
                        // counter is holding a Plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKichenObject().GetKitchenObjectSO())) {
                            player.GetKichenObject().DestroySelf();
                        }
                    }
                }
            }
        }
    }
}
