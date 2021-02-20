using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(Client.instance.username);

            SendTCPData(_packet);
        }
    }

    public static void RequestServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.requestServer))
        {
            if (Client.instance == null)
            {
                return;
            }
            _packet.Write(Client.instance.myId);

            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(int _requestId, bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_requestId);
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.instance.players[Client.instance.myId].playerMovementController.transform.rotation);
            _packet.Write(GameManager.instance.players[Client.instance.myId].playerController.camTransform.localRotation.x);

            SendUDPData(_packet);
        }
    }

    public static void PlayerTryPickUpWeapon(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerTryPickUpWeapon))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }

    public static void PlayerTryDropWeapon(int _weaponId, int _usedWeapon, Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerTryDropWeapon))
        {
            _packet.Write(_weaponId);

            _packet.Write(_usedWeapon);
            _packet.Write(_facing);

            _packet.Write(false); // Disclude startpos and startrot

            SendTCPData(_packet);
        }
    }
    public static void PlayerTryDropWeapon(int _weaponId, int _usedWeapon, Vector3 _throwStartPos, Vector3 _throwStartRot, Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerTryDropWeapon))
        {
            _packet.Write(_weaponId);

            _packet.Write(_usedWeapon);
            _packet.Write(_facing);

            _packet.Write(true); // Include startpos and startrot
            _packet.Write(_throwStartPos);
            _packet.Write(_throwStartRot);

            SendTCPData(_packet);
        }
    }

    public static void PlayerShoot(Vector3 _fireOrigin, Vector3 _facing, int _fireMode)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_fireOrigin);
            _packet.Write(_facing);
            _packet.Write(_fireMode);

            SendTCPData(_packet);
        }
    }

    public static void PlayerWeaponUsed(int _weaponUsed)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerWeaponUsed))
        {
            _packet.Write(_weaponUsed);

            SendTCPData(_packet);
        }
    }

    public static void PlayerFireMode(int _fireMode)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerFireMode))
        {
            _packet.Write(_fireMode);

            SendTCPData(_packet);
        }
    }

    public static void PlayerReload(int _weapon)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerReload))
        {
            _packet.Write(_weapon);

            SendTCPData(_packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }
    #endregion
}
