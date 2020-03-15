﻿
public class NetworkClient
{
    private enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
    }
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    // Sent from client to server when changed
    public class ClientConfig
    {
        public int serverUpdateRate;            // max bytes/sec
        public int serverUpdateInterval;        // requested tick / update
    }

    private INetworkTransport _transport;
    private ClientConfig _clientConfig;
    private ClientConnection _clientConnection;

    public bool IsConnected { get { return _connectionState == ConnectionState.Connected; } }

    public NetworkClient(INetworkTransport transport) {
        _transport = transport;
        _clientConfig = new ClientConfig();
    }

    public void Disconnect() {
        _transport.Disconnect();
    }

    public void Connect() {
        GameDebug.Assert(_connectionState == ConnectionState.Disconnected);
        GameDebug.Assert(_clientConnection == null);

        _transport.Connect();

        _connectionState = ConnectionState.Connecting;
    }

    public void Update() {
        _transport.Update();

        TransportEvent e = new TransportEvent();
        while(_transport.NextEvent(ref e)) {
            switch (e.type) {
                case TransportEvent.Type.Connect:
                OnConnect(e.ConnectionId);
                break;
                case TransportEvent.Type.Disconnect:
                OnDisconnect(e.ConnectionId);
                break;
                case TransportEvent.Type.Data:
                OnData(e.Data);
                break;
            }
        }
    }

    public void SendData() {
    }

    public void OnData(byte[] data) {
    }

    public void OnConnect(int connectionId) {
        GameDebug.Assert(_connectionState == ConnectionState.Connecting);
        Console.Write("Connected");
        _connectionState = ConnectionState.Connected;
        _clientConnection = new ClientConnection(connectionId, _clientConfig);
    }

    public void OnDisconnect(int connectionId) {
        if (_clientConnection == null) return;

        if(_clientConnection.ConnectionId != connectionId) {
            GameDebug.LogWarning("Receive disconnect event but not towards this player");
            return;
        }
        
        GameDebug.Log("Disconnected");
        _connectionState = ConnectionState.Disconnected;
        _clientConnection = null;
    }

    private class ClientConnection
    {
        private ClientConfig _clientConfig;
        public readonly int ConnectionId;

        public ClientConnection(int connectionId, ClientConfig clientConfig) {
            ConnectionId = connectionId;
        }
    }
}
