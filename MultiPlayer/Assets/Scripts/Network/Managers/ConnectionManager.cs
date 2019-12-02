using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Client
{
    public enum State
    {
        ConnectionPending,
        Connected,
    }

    public uint id;
    public ulong clientSalt;
    public ulong serverSalt;
    public IPEndPoint ipEndPoint;
    public State state;
    public float timeStamp;
    public AcknowledgeChecker acknowledgeChecker;

    public Client(IPEndPoint ipEndPoint, uint id, float timeStamp)
    {
        this.ipEndPoint = ipEndPoint;
        this.id = id;
        this.clientSalt = 0;
        this.serverSalt = 0;
        this.timeStamp = timeStamp;
        this.state = State.ConnectionPending;
        this.acknowledgeChecker = new AcknowledgeChecker();
    }

    public Client()
    {
        this.ipEndPoint = null;
        this.id = 0;
        this.clientSalt = 0;
        this.serverSalt = 0;
        this.timeStamp = 0;
        this.state = State.ConnectionPending;
        this.acknowledgeChecker = new AcknowledgeChecker();
    }
}


public class ConnectionManager : MBSingleton<ConnectionManager>
{
    public enum State
    {
        SendingConnectionRequest,
        RequestingChallenge,
        RespondingChallenge,
        Connected,
    }

    public readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
    public Dictionary<uint, Client>.Enumerator ClientIterator
    {
        get
        {
            return clients.GetEnumerator();
        }
    }
    public readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();
    public Client OwnClient = new Client();
    private const float SEND_RATE = 0.01f;
    private float timer = 0f;
    public bool isServer { get; private set; }
    public IPAddress ipAddress { get; private set; }

    public int port { get; private set; }

    private State currState;
    private uint id = 0;

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
        enabled = true;
    }

    public void StartClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        NetworkManager.Instance.StartConnection(ipAddress, port);
        currState = State.SendingConnectionRequest;
        OwnClient.acknowledgeChecker = new AcknowledgeChecker();
        enabled = true;
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client  + ip.Address");
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
                        {
                            SendConnectionRequest();
                        }
                        break;
                    case State.RespondingChallenge:
                        {
                            SendChallengeResponse(OwnClient.clientSalt, OwnClient.serverSalt);
                        }
                        break;
                    case State.Connected:
                        {
                            OwnClient.acknowledgeChecker.SendPendingPackets();
                        }
                        break;
                }
                timer = 0f;
            }
            else
            {
                ConnectionManager.Instance.OwnClient.acknowledgeChecker.SendPendingPackets();
            }
        }
        else
        {
            for (uint i = 0; i < clients.Count; i++)
            {
                clients[i].acknowledgeChecker.SendPendingPackets();
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
                Client newClient = new Client(ipEndpoint, id++, DateTime.Now.Ticks);
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
            OwnClient.clientSalt = challengeRequestData.clientSalt;
            OwnClient.serverSalt = challengeRequestData.serverSalt;
            currState = State.RespondingChallenge;
            SendChallengeResponse(OwnClient.clientSalt, OwnClient.serverSalt);
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
            {
                client.state = Client.State.Connected;
                SendToClient(new ConnectedPacket(), ipEndPoint);
            }
        }
    }

    private void FinishHandShake()
    {
        if (!isServer && currState == State.RespondingChallenge)
        {
            currState = State.Connected;
        }
    }


    public void OnReceivePacket(IPEndPoint ipEndpoint, PacketType packetType, Stream stream)
    {
        switch (packetType)
        {
            case PacketType.ConnectionRequest:
                {
                    ConnectionRequestPacket connectionRequestPacket = new ConnectionRequestPacket();
                    connectionRequestPacket.Deserialize(stream);
                    CheckAndSendChallengeRequest(ipEndpoint, connectionRequestPacket.payload);
                }
                break;
            case PacketType.ChallengeRequest:
                {
                    ChallengeRequestPacket challengeRequestPacket = new ChallengeRequestPacket();
                    challengeRequestPacket.Deserialize(stream);
                    CheckAndSendChallengeResponse(ipEndpoint, challengeRequestPacket.payload);
                }
                break;
            case PacketType.ChallengeResponse:
                {
                    ChallengeResponsePacket challengeResponsePacket = new ChallengeResponsePacket();
                    challengeResponsePacket.Deserialize(stream);
                    CheckResult(ipEndpoint, challengeResponsePacket.payload);
                }
                break;
            case PacketType.Connected:
                {
                    FinishHandShake();
                }
                break;
        }
    }

    public void QueuePacket(byte[] packet, uint id, IPEndPoint ipEndPoint)
    {
        if (isServer)
        {
            clients[ipToId[ipEndPoint]].acknowledgeChecker.QueuePacket(packet, id);
        }
    }

    public void QueuePacket(byte[] packet, uint id)
    {
        if (!isServer)
        {
            OwnClient.acknowledgeChecker.QueuePacket(packet, id);
        }
    }

    public void RegisterPackageReceived(uint id, IPEndPoint ipEndPoint)
    {
        if (ConnectionManager.Instance.isServer)
        {
            clients[ipToId[ipEndPoint]].acknowledgeChecker.RegisterPackageReceived(id);
        }
        else
        {
            OwnClient.acknowledgeChecker.RegisterPackageReceived(id);
        }
    }
}
