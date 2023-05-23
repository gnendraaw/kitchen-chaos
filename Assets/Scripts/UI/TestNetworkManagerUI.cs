using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkManagerUI : MonoBehaviour {
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake() {
        startHostButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.Instance.StartHost();

            gameObject.SetActive(false);
        });

        startClientButton.onClick.AddListener(() => {
            KitchenGameMultiplayer.Instance.StartClient();

            gameObject.SetActive(false);
        });
    }

    private void Start() {
        gameObject.SetActive(true);
    }
}

