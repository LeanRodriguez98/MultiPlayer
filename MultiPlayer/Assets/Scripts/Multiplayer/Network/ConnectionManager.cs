using System.Net;
using System.Collections.Generic;
using UnityEngine;

public struct Client {
    public enum State {
        ConnectionPending,
        Connected,
    }


    public float timeStamp;
    public uint id;
    public long clientSalt;
    public long serverSalt;
    public IPEndPoint ipEndPoint;
    public State state;

    public Client(IPEndPoint ipEndPoint, uint id, float timeStamp) {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.clientSalt = 0;
        this.serverSalt = 0;
        this.state = State.ConnectionPending;
    }
}


public class ConnectionManager : MonoBehaviour {
    private List<IPEndPoint> ips;
    private List<Client> clients;
    private long clientSalt;
    private bool isServer;
    private const float SEND_RATE = 0.1f;

    public enum State {
        ConnectionSent,
        ChallengeRequested,
        ChallengedResponded,
        Connected,
    }

    private void SendToServer<T>(NetworkPacket<T> packet) {
        PacketManager.Instance.SendPacket<T>(packet);
    }

    private void SendToClient<T>(NetworkPacket<T> packet, IPEndPoint ipEndPoint) {
        PacketManager.Instance.SendPacket<T>(packet, ipEndPoint);
    }

    private void SendConnectionRequest() {
        ConnectionRequestPacket request = new ConnectionRequestPacket();
        request.payload.clientSalt = clientSalt;
        SendToServer(request);
    }

    private void SendChallengeRequest(ConnectionRequestData connectionRequestData, long clientId, long clientSalt, long serverSalt, IPEndPoint ipEndPoint) {
        ChallengeRequestPacket request = new ChallengeRequestPacket();
        request.payload.clientId = clientId;
        request.payload.clientSalt = clientSalt;
        request.payload.serverSalt = serverSalt;
        SendToClient(request, ipEndPoint);
    }

    private void SendChallengeResponse(long clientSalt, long serverSalt) {
        ChallengeResponsePacket request = new ChallengeResponsePacket();
        request.payload.result = clientSalt ^ serverSalt;
        SendToServer(request);
    }
}