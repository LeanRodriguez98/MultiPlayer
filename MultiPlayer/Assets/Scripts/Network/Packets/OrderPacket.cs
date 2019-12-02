using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class OrderPacket<P> : MonoBehaviour {
    protected uint idReceived = 0;
    protected static uint lastIdExecuted = 0;
    protected static uint lastIdSent = 0;

    public abstract void OnFinishDeserializing(Action<P> action, P payload);
}


public abstract class ReliableOrderPacket<P> : OrderPacket<P> {
    protected static Dictionary<uint, P> pendingPackets = new Dictionary<uint, P>();

    public override void OnFinishDeserializing(Action<P> action, P payload) {
        uint nextId = lastIdExecuted + 1;
        if (idReceived == nextId) {
            action?.Invoke(payload);
            pendingPackets.Remove(idReceived);
            uint pendingID = idReceived + 1;
            while (pendingPackets.ContainsKey(pendingID)) {
                action?.Invoke(pendingPackets[pendingID]);
                pendingPackets.Remove(pendingID);
                pendingID++;
            }
            lastIdExecuted = pendingID - 1;
        } else if (idReceived > nextId) {
            pendingPackets.Add(idReceived, payload);
            if (pendingPackets.Count > 32) {
                List<uint> ids = pendingPackets.Keys.ToList();
                ids.Sort();
                uint pendingID = ids[0];
                while (pendingPackets.ContainsKey(pendingID)) {
                    action?.Invoke(pendingPackets[pendingID]);
                    pendingPackets.Remove(pendingID);
                    pendingID++;
                }
                lastIdExecuted = pendingID - 1;
            }
        }
    }
}

public abstract class UnreliableOrderPacket<P> : OrderPacket<P> {

    public override void OnFinishDeserializing(Action<P> action, P payload) {
        if (idReceived > lastIdExecuted) {
            action?.Invoke(payload);
            lastIdExecuted = idReceived;
        }  else if (idReceived < lastIdExecuted) {
            UnityEngine.Debug.Log("rechazado");
        }
    }
}

