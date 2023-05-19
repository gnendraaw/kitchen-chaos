using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour {
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f;
    private bool onGamePaused = false;
    private bool isLocalPlayerReady;
    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake() {
        Instance = this;

        state.Value = State.WaitingToStart;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Start() {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
        if (state.Value == State.WaitingToStart) {
            isLocalPlayerReady = true;

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                // this player is not ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPauseAction(object sender, System.EventArgs e) {
        TogglePauseGame();
    }

    private void Update() {
        if (!IsServer) {
            return;
        }

        switch(state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f) {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f) {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }
    }

    public void TogglePauseGame() {
        onGamePaused = !onGamePaused;

        if (onGamePaused) {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsLocalPlayerReady() {
        return isLocalPlayerReady;
    }

    public bool IsGamePlaying() {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public bool IsGameOver() {
        return state.Value == State.GameOver;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer.Value;
    }

    public float GetGamePlayingTimer() {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }
}
