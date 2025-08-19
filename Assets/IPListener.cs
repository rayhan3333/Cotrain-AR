using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
public class IPListener : MonoBehaviour
{
    UdpClient udp;
    IPEndPoint groupEP;

    String receivedIP;
    // Start is called before the first frame update
    void Awake()
    {
        udp = new UdpClient(47999);
        udp.BeginReceive(OnUdpReceive, null);
    }

    void OnUdpReceive(IAsyncResult ar)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = udp.EndReceive(ar, ref ep);
        receivedIP = ep.Address.ToString();
        UdpJsonSender[] senders = FindObjectsOfType<UdpJsonSender>();
        Debug.Log("IP Address Recieved from AMBF: " + receivedIP);
        foreach (UdpJsonSender sender in senders)
        {
            sender.ipAddress = receivedIP;
        }
    }
    void Update()
    {
        UdpJsonSender[] senders = FindObjectsOfType<UdpJsonSender>();

         foreach (UdpJsonSender sender in senders)
        {
            sender.ipAddress = receivedIP;
        }
    }

    void OnApplicationQuit()
    {
        udp.Close();
    }
}
