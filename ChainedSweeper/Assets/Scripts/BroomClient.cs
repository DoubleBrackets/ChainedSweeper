using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class BroomClient : MonoBehaviour
{
    [Tooltip("The port we bind to (that the controller server sends to)")]
    [SerializeField]
    private int _bindingPort;

    [Tooltip("The port the controller server is bound to (i.e listening on)")]
    [SerializeField]
    private int _connectingPort;

    [SerializeField]
    private UnityEvent _onReload;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        ReadLoop(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid ReadLoop(CancellationToken token)
    {
        Debug.Log($"Starting UDP Client bound to port {_bindingPort}");
        bool alreadyinuse = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
            .Any(p => p.Port == _bindingPort);

        if (alreadyinuse)
        {
            Debug.LogWarning(
                $"Port {_bindingPort} is already in use! The loop will begin reading when it is freed.");
        }

        UdpClient client = new(_bindingPort);
        Debug.Log($"Starting read loop bound to port {_bindingPort}");
        IPEndPoint remoteEndpoint = null;
        try
        {
            while (true)
            {
                await UniTask.DelayFrame(1, cancellationToken: token);
                if (client.Available > 0)
                {
                    byte[] receivedData = client.Receive(ref remoteEndpoint);
                    foreach (byte b in receivedData)
                    {
                        if (b.Equals(1))
                        {
                            _onReload?.Invoke();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Closing UDP Client: {e}");
            client.Close();
        }
    }
}