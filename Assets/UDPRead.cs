using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class UDPRead : MonoBehaviour
{
    // Start is called before the first frame update
    public static byte[] data;
    public static Socket socket;
    public static EndPoint remote;
    public static byte[] send_msg;
    public static string pose_message;
    public static string jaw_message;
    public static string dVRK_msg;
    public static bool jaw_match;
    public static int read_msg_count = 0;
    void Start()
    {
        data = new byte[1024];
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, 48052); //Change port
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ip);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)(sender);
    }

    // Update is called once per frame
    void Update()
    {
        data = new byte[1024];
        socket.ReceiveFrom(data, ref remote);
        dVRK_msg = Encoding.UTF8.GetString(data);
        Debug.Log(dVRK_msg);
    }
}
