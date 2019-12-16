using System.Collections.Generic;

public class AcknowledgeChecker
{ // La clase se encarga de dar el nuevo ackID
    private uint currAacknowledge = 0;
    public uint NextAcknowledge
    {
        get
        {
            return currAacknowledge++;
        }
    }
    private Dictionary<uint, byte[]> pendingPackets = new Dictionary<uint, byte[]>();
    private List<uint> packetsReceived = new List<uint>();
    private const uint LIMIT = 32;
    public void QueuePacket(byte[] packet, uint id) // Cada vez que manda un paquete, si es relaiable crea un queuepacket para spamearlo
    {
        pendingPackets.Add(id, packet);
        if (id > LIMIT) // si se mandan mas de un limite los recorre y elimina los viejos
        {
            List<uint> toDelete = new List<uint>();
            uint oldID = id - LIMIT;
            Dictionary<uint, byte[]>.Enumerator iterator = pendingPackets.GetEnumerator();
            while (iterator.MoveNext())
                if (iterator.Current.Key < oldID)
                    toDelete.Add(iterator.Current.Key);
            int toDeleteSize = toDelete.Count;
            for (int i = 0; i < toDeleteSize; i++)
                pendingPackets.Remove(toDelete[i]);
        }
    }

    private void RemovePacket(uint id)
    {
        pendingPackets.Remove(id);
    }

    public void SendPendingPackets()// madna todos los relaiable hasta que se notifiquen o queden viejos
    {
        Dictionary<uint, byte[]>.Enumerator iterator = pendingPackets.GetEnumerator();
        while (iterator.MoveNext())
            PacketManager.Instance.SendPacket(iterator.Current.Value);
    }

    public void Write(uint id, ref uint lastAacknowledge, ref uint aacknowledgeArray)
    {
        int difference = (int)((long)id - (long)lastAacknowledge);// se calcula la diferencia entre el ultimo y el actual
        if (difference > 0)// si hay diferencia
        {
            aacknowledgeArray = aacknowledgeArray << difference;// muevo la diferencia hacia la izquierda para igualar el lastack
            aacknowledgeArray |= 1U << (difference - 1);// guardas al anterior ultimo ack para recordad que ya lo habias recivido
            lastAacknowledge = id;// guardo el lastack con el nuevo
        }
        else if (difference > -LIMIT)// Si es menos anterior al limite que conservas lo escrivis para conservar el dato de que llego
            aacknowledgeArray |= 1U << ((-difference) - 1);

        //en caso de ser 0, todo bien
    }

    public bool Read(uint id, uint lastAcknowledge, uint acknowledgeArray)
    {
        int difference = (int)((long)lastAcknowledge - (long)id);// marca si es mas nuevo o mas viejo
        if (difference == 0)// si da 0, el ID es el lastack
            return true;
        else if (difference < 0 || difference > LIMIT)// si esta fuera de los limites de verificacion
            return false;
        else
            return (acknowledgeArray & (1 << (difference - 1))) != 0;// verifica si el byte esta encendido
    }

    public void RegisterPackageReceived(uint id)
    {
        packetsReceived.Add(id);

        if (id > LIMIT)
        {
            List<uint> toDelete = new List<uint>();
            uint oldID = id - LIMIT;
            int packetsReceivedSize = packetsReceived.Count;
            for (int i = 0; i < packetsReceivedSize; i++)
                if (packetsReceived[i] < oldID)
                    toDelete.Add(packetsReceived[i]);
            int toDeleteSize = toDelete.Count;
            for (int i = 0; i < toDeleteSize; i++)
                packetsReceived.Remove(toDelete[i]);
        }
    }

    public bool GetAcknowledgeConfirmation(out uint lastAacknowledge, out uint acknowledgekArray)
    {
        //verifica si el paquete llego con anterioridad
        //esto hace que se deje de spamear el paquete cuando llega
        lastAacknowledge = 0;
        acknowledgekArray = 0;
        int receivedCount = packetsReceived.Count;
        if (receivedCount == 0)
            return false;//si da false no hay nada que confirmar
        for (int i = 0; i < receivedCount; i++)// genero el lastAck y el ackArray correnspandiente
            Write(packetsReceived[i], ref lastAacknowledge, ref acknowledgekArray);
        return true;//si da true hay data que confirmar
    }

    public void ClearPackets(uint lastAck, uint prevAckArray)
    {
        pendingPackets.Remove(lastAck);// quito el lastack
        uint id = lastAck - 1;
        for (uint i = 0; i < LIMIT; i++, id--)
            if (Read(id, lastAck, prevAckArray))
                pendingPackets.Remove(id);// quita los 32 anteriores que tenga los bytes encendidos para limpiar el diccionario de paquetes
    }
}
