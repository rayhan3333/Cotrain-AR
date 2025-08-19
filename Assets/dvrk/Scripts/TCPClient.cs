#if !UNITY_EDITOR && UNITY_WSA
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace DVRK
{
    public class TCPListener : MonoBehaviour
    {
        public int port = 1337;
        private StreamSocketListener socketListener;
        private Queue<string> messageQueue = new Queue<string>();

        public string GetLatestTCPMessage()
        {
            string msg = "";
            while (messageQueue.Count > 0)
                msg = messageQueue.Dequeue();
            return msg;
        }

        async void Start()
        {
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnection;

            try
            {
                var icp = NetworkInformation.GetInternetConnectionProfile();

                var hostName = NetworkInformation.GetHostNames().SingleOrDefault(
                    hn => hn.IPInformation?.NetworkAdapter != null &&
                          hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

                await socketListener.BindEndpointAsync(hostName, port.ToString());
                Debug.Log("TCP server listening on port " + port);
            }
            catch (Exception ex)
            {
                Debug.LogError("TCP Bind failed: " + ex);
            }
        }

        private async void OnConnection(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Log("TCP connection received");

            try
            {
                using (var reader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
                {
                    while (true)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;

                        Debug.Log("Received TCP message: " + line);
                        messageQueue.Enqueue(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("TCP client disconnected or error: " + ex);
            }
        }

        private void OnDestroy()
        {
            if (socketListener != null)
            {
                socketListener.ConnectionReceived -= OnConnection;
                socketListener.Dispose();
                socketListener = null;
                Debug.Log("TCP socket listener disposed");
            }
        }
    }
}
#endif
