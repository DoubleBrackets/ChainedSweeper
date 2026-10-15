using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Input
{
    public class BoardClientOut : MonoBehaviour
    {
        [SerializeField]
        private int _port;

        private UdpClient _client;

        private void Awake()
        {
            _client = new UdpClient();
        }

        private void OnDestroy()
        {
            _client.Close();
        }

        public void SendLedSignal(int index)
        {
            var remoteEndpoint = new IPEndPoint(IPAddress.Loopback, _port);
            _client.Send(new[] { (byte)index }, 1, remoteEndpoint);
        }
    }
}