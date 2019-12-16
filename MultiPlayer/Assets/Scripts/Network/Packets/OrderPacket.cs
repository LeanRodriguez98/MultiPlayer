using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Order<P> : MonoBehaviour
{
    protected uint idReceived = 0;
    protected static uint lastIdExecuted = 0;
    protected static uint lastIdSent = 0;
    public abstract void OnFinishDeserializing(Action<P> action, P payload);
}

public abstract class ReliableOrder<P> : Order<P>
{
    protected static Dictionary<uint, P> pendingPackets = new Dictionary<uint, P>();
    private const uint LIMIT = 32;
    public override void OnFinishDeserializing(Action<P> action, P payload)
    {
        uint nextId = lastIdExecuted + 1;// este seria el que me tendria que llegar
        if (idReceived == nextId)// si el que llego es el que estaria esperando
        {
            if (action != null)
                action.Invoke(payload);// lo ejecuta

            uint pendingID = idReceived + 1;
            while (pendingPackets.ContainsKey(pendingID))// en caso de que tengasd paquetes pendientes por ejecutar se ejcutan todos los siguientes consecutivos que sean posibles
            {
                if (action != null)
                    action.Invoke(pendingPackets[pendingID]);
                pendingPackets.Remove(pendingID);
                pendingID++;
            }
            lastIdExecuted = pendingID - 1;//  lastIdEcexuted se pone en el punto del ultimo que ejecuto pendindID
        }
        else if (idReceived > nextId)
        {
            if (!pendingPackets.ContainsKey(idReceived))
                pendingPackets.Add(idReceived, payload);// en caso de no tener ya el paquete en los pendientes, lo agrego

            if (pendingPackets.Count > LIMIT)// verifica si los paquetes que no se ejecutaron ya son demasiado viejos y los quita
            {
                List<uint> ids = pendingPackets.Keys.ToList();
                ids.Sort();
                uint pendingID = ids[0];
                while (pendingPackets.ContainsKey(pendingID))
                {
                    if (action != null)
                        action.Invoke(pendingPackets[pendingID]);
                    pendingPackets.Remove(pendingID);
                    pendingID++;
                }
                lastIdExecuted = pendingID - 1;
            }
        }
    }
}

public abstract class NotReliableOrder<P> : Order<P>
{
    public override void OnFinishDeserializing(Action<P> action, P payload)
    {
        if (idReceived > lastIdExecuted)// en caso de no ser en orden, solo se ejecutan los paquetes si son mas nuevos que el ultimo que ejecute
        {
            if (action != null)
                action.Invoke(payload);
            lastIdExecuted = idReceived;
        }
    }
}