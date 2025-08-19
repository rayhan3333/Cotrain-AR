// using UnityEngine;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;

// public class UdpJsonSender : MonoBehaviour
// {
//     UdpClient udpClient;
//     public string ip = "10.203.67.31";
//     public int port = 48010;
//     int i = 0;
//     void Start()
//     {
//         udpClient = new UdpClient();
//         ip = "127.0.0.1";
//     }

//     void Update()
//     {
//         if (i == 10)
//         {
//             float[] positions = new float[] { 1.23f, 4.56f, 7.89f };
//             string json = JsonHelper.ToJson(positions);  // Convert to JSON array string
//             byte[] data = Encoding.UTF8.GetBytes(json);
//             udpClient.Send(data, data.Length, ip, port);
//             i = 0;
//         }
//         i++;
//     }

//     void OnApplicationQuit()
//     {
//         udpClient.Close();
//     }

//     // Helper to serialize float array into JSON array string
//     public static class JsonHelper
//     {
//         public static string ToJson(float[] array)
//         {
//             return "[" + string.Join(",", array) + "]";
//         }
//     }
// }

using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using DVRK;
using System.Collections;


public class UdpJsonSender : MonoBehaviour
{
    UdpClient udpClient;
    IPEndPoint endpoint;

    public String ipAddress;
    public int port;

    ArmControllerClassic suj;
    bool isSUJ;
    PSM psm;
    ECM ecm;

    bool isECM;

    public float[] states;
    

    void Start()
    {
        udpClient = new UdpClient();
        StartCoroutine(WaitForIPAddress());
    }

    IEnumerator WaitForIPAddress()
    {
        while (string.IsNullOrEmpty(ipAddress))
        {
            yield return null;
        }

        endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        Debug.Log("IP Address set to: " + ipAddress);
    }

    void Update()
    {
        if (endpoint != null && states != null && states.Length != 0)
        {
            string json = JsonHelper.ToJson(states);
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpClient.Send(data, data.Length, endpoint);
            //Debug.Log(gameObject.name + " sent: " + json);
        }
    }

    

    void OnApplicationQuit()
    {
        udpClient.Close();
    }

    public static class JsonHelper
    {
        public static string ToJson(float[] array)
        {
            return "[" + string.Join(",", array) + "]";
        }
    }
}