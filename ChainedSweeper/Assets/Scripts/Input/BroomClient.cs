using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Input
{
    public class BroomClient : MonoBehaviour, IGameInput
    {
        [Header("Keyboard Mocking")]

        [SerializeField]
        private InputActionReference _reloadActionRef;

        [SerializeField]
        private InputActionReference _sweepNodeActionRef;

        [Header("Input Server")]

        [Tooltip("The port we bind to (that the controller server sends to)")]
        [SerializeField]
        private int _bindingPort;

        [Tooltip("The port the controller server is bound to (i.e listening on)")]
        [SerializeField]
        private int _connectingPort;

        public Action OnReload { get; set; }

        public Action<int> OnHitSweepNode { get; set; }

        private Queue<byte> _receiveProcessQueue = new();

        public void Initialize()
        {
            ReadLoop(destroyCancellationToken).Forget();
            _reloadActionRef.SubscribeToAllAndEnable(HandleKeyboardReloadAction);
            _sweepNodeActionRef.SubscribeToAllAndEnable(HandleSweepAction);
        }

        public void Cleanup()
        {
            _reloadActionRef.UnsubscribeToAllAndDisable(HandleKeyboardReloadAction);
            _sweepNodeActionRef.UnsubscribeToAllAndDisable(HandleSweepAction);
        }

        private void HandleKeyboardReloadAction(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                OnReload?.Invoke();
            }
        }

        private void HandleSweepAction(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                var val = (int)ctx.ReadValue<float>();
                OnHitSweepNode?.Invoke(val);
            }
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
                        Debug.Log($"Processing {client.Available} bytes");
                        byte[] receivedData = client.Receive(ref remoteEndpoint);
                        foreach (byte b in receivedData)
                        {
                            _receiveProcessQueue.Enqueue(b);
                        }
                    }

                    // Each game input is 2 bytes
                    // First byte: 0000 for a broom reload, 0001 for a sweep node hit
                    // Second byte: State
                    for (var i = 0; i < _receiveProcessQueue.Count; i += 2)
                    {
                        byte byte0 = _receiveProcessQueue.Dequeue();
                        byte byte1 = _receiveProcessQueue.Dequeue();

                        if (byte0 == 0)
                        {
                            // Broom reload
                            bool broomState = byte1 != 0;
                            if (broomState)
                            {
                                OnReload?.Invoke();
                            }

                            Debug.Log(broomState ? "Reload" : "End Reload");
                        }
                        else
                        {
                            // Node hit
                            int nodeHit = byte1;
                            Debug.Log($"Hit node {nodeHit}");
                            OnHitSweepNode?.Invoke(nodeHit);
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
}