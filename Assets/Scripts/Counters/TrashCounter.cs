using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCounter : BaseCounter {
    public override void Interact(Player player) {
        if (player.HasKitchenObject()) {
            // player is holding KitchenObject
            player.GetKichenObject().DestroySelf();
        }
    }
}

