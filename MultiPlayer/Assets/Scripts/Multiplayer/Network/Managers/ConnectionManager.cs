using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public struct Client
{
    public enum State
    {
        ConnectionPending = 0,
        Connected = 1,
    }

    public uint id;
    public ulong clientSalt;
    public ulong serverSalt;
    public IPEndPoint ipEndPoint;
    public State state;
    public float timeStamp;

    public Client(IPEndPoint _ipEndPoint, uint _id, float _timeStamp)
    {
        ipEndPoint = _ipEndPoint;
        id = _id;
        clientSalt = 0;
        serverSalt = 0;
        timeStamp = _timeStamp;
        state = State.ConnectionPending;
    }
}


public class ConnectionManager : MBSingleton<ConnectionManager>
{
    private readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
    private readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();
    public Dictionary<uint, Client>.Enumerator ClientIterator
    {
        get { return clients.GetEnumerator(); }
    }
    private ulong clientSalt;
    private ulong serverSalt;

    private State currState;

    private const float SEND_RATE = 0.01f;
    private float timer = 0.0f;
    private uint clientId;
    public bool isServer { get; private set; }
    public IPAddress ipAddress { get; private set; }

    public int port { get; private set; }

    public enum State
    {
        SendingConnectionRequest,
        RequestingChallenge,
        RespondingChallenged,
        Connected,
    }

    protected override void Awake()
    {
        base.Awake();
        enabled = false;
    }

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        NetworkManager.Instance.StartConnection(port);
    }

    public void StartClient(IPAddress _ipAddress, int _port)
    {
        isServer = false;
        port = _port;
        ipAddress = _ipAddress;
        NetworkManager.Instance.StartConnection(ipAddress, port);
        currState = State.SendingConnectionRequest;
        enabled = true;
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            clients.Remove(ipToId[ip]);
        }
    }

    private void Update()
    {
        if (!isServer)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= SEND_RATE)
            {
                switch (currState)
                {
                    case State.SendingConnectionRequest:
                        SendConnectionRequest();
                        break;
                    case State.RespondingChallenged:
                        SendChallengeResponse(clientSalt, serverSalt);
                        break;
                }
                timer = 0.0f;
            }
        }
    }

    private void SendToServer<T>(NetworkPacket<T> packet)
    {
        PacketManager.Instance.SendPacket<T>(packet);
    }

    private void SendToClient<T>(NetworkPacket<T> packet, IPEndPoint ipEndPoint)
    {
        PacketManager.Instance.SendPacket<T>(packet, ipEndPoint);
    }

    private void SendConnectionRequest()
    {
        ConnectionRequestPacket request = new ConnectionRequestPacket();
        request.payload.clientSalt = Utilities.GetRandomUlong();
        SendToServer(request);
    }

    private void CheckAndSendChallengeRequest(IPEndPoint ipEndpoint, ConnectionRequestData connectionRequestData)
    {
        if (isServer)
        {
            if (!ipToId.ContainsKey(ipEndpoint))
            {
                Client newClient = new Client(ipEndpoint, clientId++, DateTime.Now.Ticks);
                newClient.clientSalt = connectionRequestData.clientSalt;
                newClient.serverSalt = Utilities.GetRandomUlong();
                clients.Add(newClient.id, newClient);
                ipToId.Add(ipEndpoint, newClient.id);
            }
            SendChallengeRequest(clients[ipToId[ipEndpoint]]);
        }
    }

    private void SendChallengeRequest(Client client)
    {
        ChallengeRequestPacket request = new ChallengeRequestPacket();
        request.payload.clientId = client.id;
        request.payload.clientSalt = client.clientSalt;
        request.payload.serverSalt = client.serverSalt;
        SendToClient(request, client.ipEndPoint);
    }

    private void CheckAndSendChallengeResponse(IPEndPoint ipEndpoint, ChallengeRequestData challengeRequestData)
    {
        if (!isServer && currState == State.SendingConnectionRequest)
        {
            clientSalt = challengeRequestData.clientSalt;
            serverSalt = challengeRequestData.serverSalt;
            currState = State.RespondingChallenged;
            SendChallengeResponse(clientSalt, serverSalt);
        }
    }


    private void SendChallengeResponse(ulong clientSalt, ulong serverSalt)
    {
        ChallengeResponsePacket request = new ChallengeResponsePacket();
        request.payload.result = clientSalt ^ serverSalt;
        SendToServer(request);
    }

    private void CheckResult(IPEndPoint ipEndPoint, ChallengeResponseData challengeResponseData)
    {
        if (isServer)
        {
            Client client = clients[ipToId[ipEndPoint]];
            ulong result = client.clientSalt ^ client.serverSalt;
            if (challengeResponseData.result == result)
                SendToClient(new ConnectedPacket(), ipEndPoint);
        }
    }

    public void OnReceivePacket(IPEndPoint ipEndpoint, PacketType packetType, Stream stream)
    {
        switch (packetType)
        {
            case PacketType.ConnectionRequest:
                ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
                connectionRequestPacket.Deserialize(stream);
                CheckAndSendChallengeRequest(ipEndpoint, connectionRequestPacket.payload);
                break;
            case PacketType.ChallengeRequest:
                ChallengeRequestPacket challengeRequestPacket = new ChallengeRequestPacket();
                challengeRequestPacket.Deserialize(stream);
                CheckAndSendChallengeResponse(ipEndpoint, challengeRequestPacket.payload);
                break;
            case PacketType.ChallengeResponse:
                ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
                challengeResponsePacket.Deserialize(stream);
                CheckResult(ipEndpoint, challengeResponsePacket.payload);
                break;
            case PacketType.Connected:
                if (!isServer && currState == State.RespondingChallenged)
                {
                    currState = State.Connected;
                    enabled = false;
                }
                break;
        }
    }

}