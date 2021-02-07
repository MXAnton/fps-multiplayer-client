using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void ServerRespondToClient(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if (_id == Client.instance.myId)
        {
            Client.instance.ServerRespond();
        }
    }
    
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void LocalPlayerMovementVars(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _gravity = _packet.ReadFloat();
        float _moveSpeed = _packet.ReadFloat();
        float _runSpeedMultiplier = _packet.ReadFloat();
        float _jumpSpeed = _packet.ReadFloat();

        if (GameManager.instance.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.playerMovementController.SetMovementVars(_gravity, _moveSpeed, _runSpeedMultiplier, _jumpSpeed);
        }
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        bool _doTeleport = _packet.ReadBool();

        float _runSpeed = _packet.ReadFloat();

        float _headXRotation = _packet.ReadFloat();

        //Debug.Log("Get player position");
        if (GameManager.instance.players.TryGetValue(_id, out PlayerManager _player))
        {
            //Debug.Log("Get Value");
            if (_player.GetComponent<PlayerController>())
            {
                //Debug.Log("My server position");
                _player.transitionToPosition = _position;

                if (_doTeleport)
                {
                    _player.SetPosition(_position);
                }
            }
            else
            {
                _player.transitionToPosition = _position;
                _player.headXRotation = _headXRotation;

                if (_doTeleport)
                {
                    _player.SetPosition(_position);
                }
            }
            _player.runSpeed = _runSpeed;
        }
    }

    public static void PlayerPositionRespond(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        float _yVelocity = _packet.ReadFloat();
        bool _doTeleport = _packet.ReadBool();

        int _lastestMovementRespondId = _packet.ReadInt();

        //Debug.Log("Get player position");
        if (GameManager.instance.players.TryGetValue(_id, out PlayerManager _player))
        {
            //Debug.Log("My server position CORRECTED");
            //if (_doTeleport)
            //{
            //    _player.SetPosition(_position);
            //}
            _player.playerMovementController.MovementRespond(_lastestMovementRespondId, _position, _yVelocity);
        }
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.instance.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.rotation = _rotation;
        }
    }

    public static void PlayerInputs(Packet _packet)
    {
        int _id = _packet.ReadInt();

        if (GameManager.instance.players.TryGetValue(_id, out PlayerManager _player))
        {
            bool[] _inputs = new bool[_packet.ReadInt()];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }
            _player.inputs = _inputs;
        }
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Destroy(GameManager.instance.players[_id].gameObject);
        GameManager.instance.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.instance.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.instance.players[_id].Respawn();
    }

    public static void PlayerShot(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _viewDirection = _packet.ReadVector3();

        GameManager.instance.players[_id].Shoot(_viewDirection);
    }

    public static void PlayerHitInfo(Packet _packet)
    {
        Vector3 _hitPoint = _packet.ReadVector3();
        float _damageGiven = _packet.ReadFloat();

        UIManager.instance.ShowHitDamage(_hitPoint, _damageGiven);
    }

    public static void PlayerDeathsAndKills(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _kills = _packet.ReadInt();
        int _deaths = _packet.ReadInt();

        GameManager.instance.players[_id].UpdatePlayerDeathsAndKills(_kills, _deaths);
    }

    public static void PlayerKilled(Packet _packet)
    {
        string _killerName = _packet.ReadString();
        string _killedName = _packet.ReadString();

        UIManager.instance.PlayerKilled(_killerName, _killedName);
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.instance.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();

        GameManager.instance.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.instance.players[_byPlayer].itemCount++;
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _thrownByPlayer = _packet.ReadInt();

        GameManager.instance.SpawnProjectile(_projectileId, _position);
        GameManager.instance.players[_thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.instance.projectiles.TryGetValue(_projectileId, out ProjectileManager _projectile))
        {
            //_projectile.transform.position = _position;
            _projectile.LerpMove(_position);
        }
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.instance.projectiles[_projectileId].Explode(_position);
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.instance.SpawnEnemy(_enemyId, _position);
    }

    public static void EnemyPosition(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        bool _doTeleport = _packet.ReadBool();

        if (GameManager.instance.enemies.TryGetValue(_enemyId, out EnemyManager _enemy))
        {
            _enemy.LerpMove(_position);

            if (_doTeleport)
            {
                _enemy.transform.position = _position;
            }
        }
    }

    public static void EnemyRotation(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.instance.enemies.TryGetValue(_enemyId, out EnemyManager _enemy))
        {
            _enemy.transform.rotation = _rotation;
        }
    }

    public static void EnemyHealth(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.instance.enemies[_enemyId].SetHealth(_health);
    }

    public static void EnemyShot(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _viewDirection = _packet.ReadVector3();

        GameManager.instance.enemies[_id].Shoot(_viewDirection);
    }
}
