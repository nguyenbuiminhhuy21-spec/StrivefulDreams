using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGame.Scripts.Services.Multiplayer
{
    public interface ISteamMultiplayerService
    {
        bool IsInitialized { get; }
        bool IsInLobby { get; }
        CSteamID CurrentLobbyId { get; }
        List<CSteamID> LobbyMembers { get; }
        string PlayerName { get; }
        CSteamID PlayerSteamId { get; }

        event Action OnLobbyCreated;
        event Action OnLobbyJoined;
        event Action<CSteamID> OnPlayerJoined;
        event Action<CSteamID> OnPlayerLeft;
        event Action<byte[], CSteamID> OnMessageReceived;

        Task<bool> InitializeAsync();
        Task<bool> CreateLobbyAsync(int maxPlayers = 4);
        Task<bool> JoinLobbyAsync(CSteamID lobbyId);
        Task<bool> LeaveLobbyAsync();
        void SendMessageToAll(byte[] data, int channel = 0);
        void SendMessageToUser(CSteamID targetSteamId, byte[] data, int channel = 0);
        void Update();
        void Shutdown();
    }

    public class SteamMultiplayerService : ISteamMultiplayerService
    {
        private bool _isInitialized;
        private CSteamID _currentLobbyId;
        private List<CSteamID> _lobbyMembers = new();
        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
        private Callback<SteamNetworkingMessagesSessionRequest_t> _networkingMessagesSessionRequestCallback;
        private Callback<SteamNetworkingMessagesSessionFailed_t> _networkingMessagesSessionFailedCallback;

        public bool IsInitialized => _isInitialized;
        public bool IsInLobby => _currentLobbyId.IsValid();
        public CSteamID CurrentLobbyId => _currentLobbyId;
        public List<CSteamID> LobbyMembers => _lobbyMembers.ToList();
        public string PlayerName => SteamFriends.GetPersonaName();
        public CSteamID PlayerSteamId => SteamUser.GetSteamID();

        public event Action OnLobbyCreated;
        public event Action OnLobbyJoined;
        public event Action<CSteamID> OnPlayerJoined;
        public event Action<CSteamID> OnPlayerLeft;
        public event Action<byte[], CSteamID> OnMessageReceived;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Initialize Steam API with app ID (placeholder - replace with your actual Steam App ID)
                if (!SteamAPI.Init())
                {
                    Console.WriteLine("Failed to initialize Steam API");
                    return false;
                }

                // Register callbacks
                _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreatedCallback);
                _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnterCallback);
                _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdateCallback);
                _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdateCallback);
                _networkingMessagesSessionRequestCallback = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnNetworkingMessagesSessionRequest);
                _networkingMessagesSessionFailedCallback = Callback<SteamNetworkingMessagesSessionFailed_t>.Create(OnNetworkingMessagesSessionFailed);

                _isInitialized = true;
                Console.WriteLine($"Steam initialized successfully. Player: {PlayerName} ({PlayerSteamId})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Steam: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateLobbyAsync(int maxPlayers = 4)
        {
            if (!_isInitialized) return false;

            var call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxPlayers);
            if (call == SteamAPICall_t.Invalid)
            {
                Console.WriteLine("Failed to create lobby");
                return false;
            }

            // Wait for callback
            return true;
        }

        public async Task<bool> JoinLobbyAsync(CSteamID lobbyId)
        {
            if (!_isInitialized) return false;

            var call = SteamMatchmaking.JoinLobby(lobbyId);
            if (call == SteamAPICall_t.Invalid)
            {
                Console.WriteLine("Failed to join lobby");
                return false;
            }

            return true;
        }

        public async Task<bool> LeaveLobbyAsync()
        {
            if (!_isInitialized || !IsInLobby) return false;

            SteamMatchmaking.LeaveLobby(_currentLobbyId);
            _currentLobbyId = CSteamID.Nil;
            _lobbyMembers.Clear();

            return true;
        }

        public void SendMessageToAll(byte[] data, int channel = 0)
        {
            if (!_isInitialized || !IsInLobby) return;

            foreach (var memberId in _lobbyMembers)
            {
                if (memberId != PlayerSteamId)
                {
                    SendMessageToUser(memberId, data, channel);
                }
            }
        }

        public void SendMessageToUser(CSteamID targetSteamId, byte[] data, int channel = 0)
        {
            if (!_isInitialized) return;

            // TODO: Implement message sending
            Console.WriteLine($"Sending message to {targetSteamId}: {data.Length} bytes");
        }

        public void Update()
        {
            if (!_isInitialized) return;

            SteamAPI.RunCallbacks();

            // TODO: Poll for messages
        }

        public void Shutdown()
        {
            if (_isInitialized)
            {
                SteamAPI.Shutdown();
                _isInitialized = false;
            }
        }

        private void OnLobbyCreatedCallback(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                Console.WriteLine($"Lobby created: {_currentLobbyId}");
                OnLobbyCreated?.Invoke();
            }
            else
            {
                Console.WriteLine($"Failed to create lobby: {callback.m_eResult}");
            }
        }

        private void OnLobbyEnterCallback(LobbyEnter_t callback)
        {
            _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            Console.WriteLine($"Joined lobby: {_currentLobbyId}");
            UpdateLobbyMembers();
            OnLobbyJoined?.Invoke();
        }

        private void OnLobbyDataUpdateCallback(LobbyDataUpdate_t callback)
        {
            UpdateLobbyMembers();
        }

        private void OnLobbyChatUpdateCallback(LobbyChatUpdate_t callback)
        {
            var userChanged = new CSteamID(callback.m_ulSteamIDUserChanged);
            var stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

            if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                OnPlayerJoined?.Invoke(userChanged);
            }
            else if (stateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
                     stateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                OnPlayerLeft?.Invoke(userChanged);
            }

            UpdateLobbyMembers();
        }

        private void OnNetworkingMessagesSessionRequest(SteamNetworkingMessagesSessionRequest_t callback)
        {
            SteamNetworkingMessages.AcceptSessionWithUser(ref callback.m_identityRemote);
        }

        private void OnNetworkingMessagesSessionFailed(SteamNetworkingMessagesSessionFailed_t callback)
        {
            Console.WriteLine($"Networking session failed with {callback.m_info.m_identityRemote.GetSteamID()}: {callback.m_info.m_eEndReason}");
        }

        private void UpdateLobbyMembers()
        {
            if (!IsInLobby) return;

            _lobbyMembers.Clear();
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(_currentLobbyId);

            for (int i = 0; i < memberCount; i++)
            {
                var memberId = SteamMatchmaking.GetLobbyMemberByIndex(_currentLobbyId, i);
                _lobbyMembers.Add(memberId);
            }
        }
    }
}