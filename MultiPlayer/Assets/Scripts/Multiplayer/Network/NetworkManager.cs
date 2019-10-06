using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class NetworkManager : MBSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    private readonly Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
    private readonly Dictionary<IPEndPoint, uint> ipToId = new Dictionary<IPEndPoint, uint>();

    public uint clientId { get; set; }
    
    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
        PacketManager.Instance.Awake();
    }

    public void StartClient(IPAddress ip, int port)
    {
        isServer = false;
        
        this.port = port;
        this.ipAddress = ip;
        
        connection = new UdpConnection(ip, port, this);

        AddClient(new IPEndPoint(ip, port));
    }

    void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: " + ip.Address);

            uint id = clientId;
            ipToId[ip] = clientId;
            
            clients.Add(clientId, new Client(ip, id, Time.realtimeSinceStartup));

            clientId ++;
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        AddClient(ip);

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToClient(byte[] data, IPEndPoint ipEndPoint) {
        connection.Send(data, ipEndPoint);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    void Update()
    {
        // We flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}