using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();
    public Dictionary<int, EnemyManager> enemies = new Dictionary<int, EnemyManager>();
    public Dictionary<int, Weapon> weapons = new Dictionary<int, Weapon>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;

    public GameObject[] weaponsPrefabs;

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
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, Vector3.zero, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, Vector3.zero, _rotation);
        }
        _player.GetComponent<PlayerManager>().Initialize(_id, _username);
        players.Add(_id, _player.GetComponent<PlayerManager>());

        _player.GetComponent<PlayerManager>().usedTransform.position = _position;

        _player.GetComponent<PlayerManager>().transitionToPosition = _position;
    }

    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        GameObject _spawner = Instantiate(itemSpawnerPrefab, _position, itemSpawnerPrefab.transform.rotation);
        _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int _id, Vector3 _position)
    {
        GameObject _projectile = Instantiate(projectilePrefab, _position, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().Initialize(_id);
        projectiles.Add(_id, _projectile.GetComponent<ProjectileManager>());

        _projectile.GetComponent<ProjectileManager>().LerpMove(_position);
    }

    public void SpawnEnemy(int _id, Vector3 _position)
    {
        GameObject _enemy = Instantiate(enemyPrefab, _position, Quaternion.identity);
        _enemy.GetComponent<EnemyManager>().Initialize(_id);
        enemies.Add(_id, _enemy.GetComponent<EnemyManager>());

        _enemy.GetComponent<EnemyManager>().LerpMove(_position);
    }


    public void SpawnWeapon(int _id, int _weaponToSpawn, Vector3 _position, int _currentClipAmmo, int _currentExtraAmmo, int _maxClipAmmo, int _maxExtraAmmo, float _reloadTime,
            float _autoFireRate, float _burstFireRate, float _semiFireRate, float _fireSpread, float _shootDistance)
    {
        GameObject _weapon = Instantiate(weaponsPrefabs[_weaponToSpawn], _position, Quaternion.identity);
        _weapon.GetComponent<Weapon>().Initialize(_id);
        weapons.Add(_id, _weapon.GetComponent<Weapon>());

        _weapon.GetComponent<Weapon>().currentClipAmmo = _currentClipAmmo;
        _weapon.GetComponent<Weapon>().currentExtraAmmo = _currentExtraAmmo;
        _weapon.GetComponent<Weapon>().maxClipAmmo = _maxClipAmmo;
        _weapon.GetComponent<Weapon>().maxExtraAmmo = _maxExtraAmmo;
        _weapon.GetComponent<Weapon>().reloadTime = _reloadTime;
        _weapon.GetComponent<Weapon>().autoFireRate = _autoFireRate;
        _weapon.GetComponent<Weapon>().burstFireRate = _burstFireRate;
        _weapon.GetComponent<Weapon>().semiFireRate = _semiFireRate;
        _weapon.GetComponent<Weapon>().fireSpread = _fireSpread;
        _weapon.GetComponent<Weapon>().fireDistance = _shootDistance;
    }
}
