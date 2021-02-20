using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "81.232.86.164"; // local ip: 127.0.0.1
    public int port = 25565;
    public int myId = 0;
    public string username;
    public TCP tcp;
    public UDP udp;

    public bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;


    public bool isReConnecting = false;
    public float waitedForServerCallbackTimer;
    public float maxServerCallbackTime = 3;


    public bool waitingForServerCallback = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        DontDestroyOnLoad(this.gameObject);
        packetHandlers = new Dictionary<int, PacketHandler>();
    }

    private void OnApplicationQuit()
    {
        if (isConnected)
        {
            Disconnect();
        }
    }

    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        InitializeClientData();


        instance.isConnected = true;
        tcp.Connect();
    }

    private void FixedUpdate()
    {
        if (isConnected && !tcp.socket.Connected && isReConnecting == false)
        {
            Debug.Log("Couldn't connect!");

            StartCoroutine(TryReconnect());
        }

        // Check if client still connected to server
        if (waitingForServerCallback)
        {
            waitedForServerCallbackTimer -= Time.fixedDeltaTime;

            if (waitedForServerCallbackTimer <= 0)
            {
                //waitingForServerCallback = false;
                waitedForServerCallbackTimer = maxServerCallbackTime;
                StartCoroutine(TryReconnect());
                Debug.Log("Didn't receive callback...");
            }
        }
    }

    private IEnumerator TryReconnect()
    {
        isReConnecting = true;
        waitingForServerCallback = false;

        Debug.Log("Trying to reconnect..."); 
        GameObject _reconnectingTextObject = Instantiate(UIManager.instance.reconnectingTextPrefab);
        
        isConnected = false;
        if (tcp != null)
        {
            if (tcp.socket != null)
            {
                tcp.socket.Close();
            }
        }
        if (udp != null)
        {
            if (udp.socket != null)
            {
                udp.socket.Close();
            }
        }
        ConnectToServer();

        yield return new WaitForSeconds(2);

        //if (instance == null)
        //{
        //    Destroy(this);
        //}

        isReConnecting = false;
        // Still not connected? If so, end sockets and leave
        if (!tcp.socket.Connected)
        {
            Debug.Log("Couldn't reconnect!");
            Destroy(_reconnectingTextObject);
            Instantiate(UIManager.instance.couldntReconnectTextPrefab);

            instance.Disconnect();
            StopAllCoroutines();
        }


        isReConnecting = false;
        // send request to server
        waitedForServerCallbackTimer = maxServerCallbackTime;
        waitingForServerCallback = true;

        ClientSend.RequestServer();

        Destroy(_reconnectingTextObject);
    }

    public void ServerRespond()
    {
        waitedForServerCallbackTimer = maxServerCallbackTime;
        waitingForServerCallback = true;
        ClientSend.RequestServer();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                Debug.LogWarning("TCP socket couldn't connect to ip!");
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }


            instance.ServerRespond();
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.serverRespondToClient, ClientHandle.ServerRespondToClient },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.localPlayerMovementVars, ClientHandle.LocalPlayerMovementVars },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerPositionRespond, ClientHandle.PlayerPositionRespond },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerInputs, ClientHandle.PlayerInputs },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            { (int)ServerPackets.playerShot, ClientHandle.PlayerShot },
            { (int)ServerPackets.playerReloadDone, ClientHandle.PlayerReloadDone },
            { (int)ServerPackets.playerHitInfo, ClientHandle.PlayerHitInfo },
            { (int)ServerPackets.playerDeathsAndKills, ClientHandle.PlayerDeathsAndKills },
            { (int)ServerPackets.playerKilled, ClientHandle.PlayerKilled },
            { (int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner },
            { (int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned },
            { (int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp },
            { (int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            { (int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition },
            { (int)ServerPackets.projectileExploded, ClientHandle.ProjectileExploded },
            { (int)ServerPackets.spawnEnemy, ClientHandle.SpawnEnemy },
            { (int)ServerPackets.enemyPosition, ClientHandle.EnemyPosition },
            { (int)ServerPackets.enemyRotation, ClientHandle.EnemyRotation },
            { (int)ServerPackets.enemyHealth, ClientHandle.EnemyHealth },
            { (int)ServerPackets.enemyShot, ClientHandle.EnemyShot },
            { (int)ServerPackets.spawnWeapon, ClientHandle.SpawnWeapon },
            { (int)ServerPackets.weaponPositionAndRotation, ClientHandle.WeaponPositionAndRotation },
            { (int)ServerPackets.playerPickedWeapon, ClientHandle.PlayerPickedWeapon },
            { (int)ServerPackets.playerDroppedWeapon, ClientHandle.PlayerDroppedWeapon },
            { (int)ServerPackets.playerWeaponUsed, ClientHandle.PlayerWeaponUsed }
        };

        Debug.Log("Initialized packets.");
    }

    public void Disconnect()
    {
        //for (int i = 0; i < 3; i++)
        //{
        //    if (GameManager.instance.players[instance.myId].playerController.weaponsController.weaponsEquiped[i] != null)
        //    {
        //        ClientSend.PlayerTryDropWeapon(GameManager.instance.players[instance.myId].playerController.weaponsController.weaponsEquiped[i].GetComponent<Weapon>().id,
        //                                        i, Vector3.zero);
        //    }
        //}

        StopAllCoroutines();

        isConnected = false;
        if (tcp != null)
        {
            if (tcp.socket != null)
            {
                tcp.socket.Close();
            }
        }
        if (udp != null)
        {
            if (udp.socket != null)
            {
                udp.socket.Close();
            }
        }

        Debug.Log("Disconnected from server.");

        Client.instance = null;
        Destroy(gameObject);

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
