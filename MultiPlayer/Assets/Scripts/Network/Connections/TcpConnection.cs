using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class TcpConnection
{
    private struct DataReceived
    {
        public byte[] data;
        public IPEndPoint ipEndPoint;
    }

    private readonly TcpClient connection;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();

    public TcpConnection(string host, int port, IReceiveData receiver = null)
    {
        connection = new TcpClient(/* Elegir la sobrecarga correcta */);

        this.receiver = receiver;

        connection.BeginConnect(host, port, OnReceive, null); // Buscar bien que va aca
    }

    public TcpConnection(IPAddress ip, int port, IReceiveData receiver = null)
    {
        connection = new TcpClient(/* Elegir la sobrecarga correcta */);
        connection.Connect(ip, port);

        this.receiver = receiver;

        connection.BeginConnect(ip, port, OnReceive, null);
    }

    void OnReceive(IAsyncResult ar)
    {
        try
        {
            DataReceived dataReceived = new DataReceived();
            //dataReceived.data = connection.EndReceive(ar, ref dataReceived.ipEndPoint);
            connection.EndConnect(ar);

            lock (handler)
            {
                dataReceivedQueue.Enqueue(dataReceived);
            }
        }
        catch (SocketException e)
        {
            // This happens when a client disconnects, as we fail to send to that port.
            UnityEngine.Debug.LogError("[UdpConnection] " + e.Message);
        }

        //connection.BeginConnect(OnReceive, null); // Descomentar esto mañana
    }
}
