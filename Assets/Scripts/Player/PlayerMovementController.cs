using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovementController : MonoBehaviour
{
    public PlayerManager playerManager;
    public PlayerController playerController;
    public GameObject playerMovementSimulatorPrefab;
    private PlayerMovementSimulator playerMovementSimulator;

    public Dictionary<int, bool[]> movementRequestsInputs = new Dictionary<int, bool[]>();
    public Dictionary<int, Quaternion> movementRequestsRotations = new Dictionary<int, Quaternion>();
    public Dictionary<int, Vector3> clientPredictedMovements = new Dictionary<int, Vector3>();
    public int nextMovementRequestId = 0;
    public int nextClientPredictMoveId = 0;
    public float maxMovementPredictionWrong = 0.05f;
    public float maxMovementPredictionWrongForce = 0.3f;

    public GameObject serverPositionObjectPrefab;
    public GameObject serverPositionObject;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float runSpeedMultiplier = 2f;

    [Header("Jump")]
    public float jumpSpeed = 3.5f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float yVelocity;

    [Header("Ground Checking")]
    public float height = 2f;
    public float stepHeight = 0.5f;
    public float stepSearchOffset = 1f;

    [Header("Other")]
    public LayerMask groundedDisclude;
    public LayerMask discludePlayer;
    public CapsuleCollider bodyCollider;


    private bool[] inputs;

    private void Start()
    {
        movementRequestsInputs = new Dictionary<int, bool[]>();
        movementRequestsRotations = new Dictionary<int, Quaternion>();
        clientPredictedMovements = new Dictionary<int, Vector3>();

        serverPositionObject = Instantiate(serverPositionObjectPrefab, transform.position, Quaternion.identity);

        playerMovementSimulator = Instantiate(playerMovementSimulatorPrefab, transform.position, Quaternion.identity).GetComponent<PlayerMovementSimulator>();
        playerMovementSimulator.playerMovementController = this;
        playerMovementSimulator.realPlayerCol = bodyCollider;
    }

    private void Update()
    {
        CollisionCheck();

        if (IsGrounded())
        {
            GetUpFromGround();
        }
    }

    private void FixedUpdate()
    {
        inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space),
            Input.GetKey(KeyCode.LeftShift)
        };

        Vector2 _inputDirection = GetInputDirection(inputs[0], inputs[1], inputs[2], inputs[3]);


        if (playerManager.health > 0)
        {
            SendInputToServer();
            PredictMovement(0, _inputDirection, inputs[4], inputs[5], transform.rotation);
        }
    }

    private void SendInputToServer()
    {
        movementRequestsInputs.Add(nextMovementRequestId, inputs);
        movementRequestsRotations.Add(nextMovementRequestId, transform.rotation);

        ClientSend.PlayerMovement(nextMovementRequestId, inputs);
        nextMovementRequestId++;
    }

    private void PredictMovement(int _oldClientPredictMovementKey, Vector2 _inputDirection, bool _input4, bool _input5, Quaternion _lookRotation)
    {
        transform.rotation = _lookRotation;

        //// remove predict
        //transform.position = serverPositionObject.transform.position;
        //return;

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;
        if (_input5)
        {
            _moveDirection *= runSpeedMultiplier;
            playerController.camTransform.GetComponent<CameraController>().SetFieldOfView(FieldOfViewState.running);
        }
        else
        {
            playerController.camTransform.GetComponent<CameraController>().SetFieldOfView(FieldOfViewState.normal);
        }

        if (IsGrounded())
        {
            yVelocity = 0;
            if (_input4)
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;
        if (yVelocity > 0)
        {
            // If moving upwards, make sure the player doesn't touch roof. If player touch roof, set yVelocity to 0 to make sure that the player doesn't "slide" around in the roof.
            if (GetClearDistanceAbovePlayer() < 0.1f)
            {
                yVelocity = 0;
            }
        }


        _moveDirection.y = yVelocity;


        // If something i in from of the player with less distance than _moveDirection, don't move.
        //if (Physics.Raycast(transform.position, _moveDirection, out RaycastHit _hit, Vector3.Distance(Vector3.zero, _moveDirection), discludePlayer, QueryTriggerInteraction.Ignore))
        //{
        //    _moveDirection = Vector3.zero;
        //}
        if (CollisionInOffset(_moveDirection / bodyCollider.radius))
        {
            //_moveDirection = Vector3.zero;
        }


        Vector3 _newPosition = transform.position + _moveDirection;
        playerManager.transitionToPosition = _newPosition;
        transform.position = _newPosition;



        CollisionCheck();

        if (IsGrounded())
        {
            GetUpFromGround();
        }
        if (IsGroundedInFront(new Vector3(_moveDirection.x, 0, _moveDirection.z)))
        {
            GetUpFromGroundWithOffset(new Vector3(_moveDirection.x, 0, _moveDirection.z));
        }


        playerManager.transitionToPosition = transform.position;

        if (_oldClientPredictMovementKey == 0)
        {
            clientPredictedMovements.Add(nextClientPredictMoveId, transform.position);
            nextClientPredictMoveId++;
        }
        else
        {
            clientPredictedMovements[_oldClientPredictMovementKey] = transform.position;
        }
    }

    public void MovementRespond(int _latestMovementRespondId, Vector3 _serverPosition, float _yVelocity)
    {
        serverPositionObject.transform.position = _serverPosition;

        if (clientPredictedMovements.TryGetValue(_latestMovementRespondId, out Vector3 _clientPosition))
        {
            //Debug.Log("Predictment wrong: " + Vector3.Distance(_clientPosition, _serverPosition));

            // If too big difference between server player position and client player position, snap the client's player position to the server player position.
            if (Vector3.Distance(_clientPosition, _serverPosition) > maxMovementPredictionWrongForce)
            {
                playerManager.transitionToPosition = _serverPosition;
                clientPredictedMovements[_latestMovementRespondId] = _serverPosition;
                transform.position = _serverPosition;
                yVelocity = _yVelocity;

                if (movementRequestsInputs.Count == 0)
                {
                    return;
                }
                // Remove all old client movemement predictions
                int _minimumKey = movementRequestsInputs.Keys.Min();
                for (int i = _minimumKey; i <= _latestMovementRespondId; i++)
                {
                    movementRequestsInputs.Remove(i);
                    movementRequestsRotations.Remove(i);
                    clientPredictedMovements.Remove(i);
                }
                if (movementRequestsInputs.Count == 0)
                {
                    return;
                }
            }
            else if (Vector3.Distance(_clientPosition, _serverPosition) > maxMovementPredictionWrong)
            {
                //Debug.Log("maxMovementpredictwrong, not force");

                // Set movement simulation client player position to the correct old server position, and then simulate the client's new movement PREDICTIONS that the server hasn't handled yet.
                // Last of all, set the real client player position to the new correct simulated position.
                bodyCollider.enabled = false;
                clientPredictedMovements[_latestMovementRespondId] = _serverPosition;

                yVelocity = _yVelocity;

                // Remove all old client movemement predictions
                int _minimumKey = movementRequestsInputs.Keys.Min();
                for (int i = _minimumKey; i <= _latestMovementRespondId; i++)
                {
                    movementRequestsInputs.Remove(i);
                    movementRequestsRotations.Remove(i);
                    clientPredictedMovements.Remove(i);
                }
                if (movementRequestsInputs.Count == 0)
                {
                    playerManager.transitionToPosition = _serverPosition;
                    //transform.position = _serverPosition;
                    bodyCollider.enabled = true;
                    //Debug.LogWarning("count == 0 ---- 2");
                    return;
                }

                playerMovementSimulator.SimulateNewMovement(_serverPosition, _yVelocity);
            }
        }
    }


    private void CollisionCheck()
    {
        // Check with body collider
        Collider[] overlaps = new Collider[10];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(bodyCollider.center), bodyCollider.radius, overlaps, discludePlayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < num; i++)
        {
            if (overlaps[i].gameObject == gameObject)
            {
                return;
            }
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(bodyCollider, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                transform.position = transform.position + penetrationVector;
            }
        }
    }
    private bool CollisionInOffset(Vector3 _offset)
    {
        // Check with body collider
        Collider[] overlaps = new Collider[10];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(bodyCollider.center) + _offset, bodyCollider.radius, overlaps, discludePlayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < num; i++)
        {
            if (overlaps[i].gameObject != gameObject)
            {
                return true;
            }

            //Transform t = overlaps[i].transform;
            //Vector3 dir;
            //float dist;

            //if (Physics.ComputePenetration(bodyCollider, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            //{
            //    Vector3 penetrationVector = dir * dist;
            //    transform.position = transform.position + penetrationVector;
            //}
        }

        return false;
    }

    private void GetUpFromGround()
    {
        float _distanceAbovePlayer = GetClearDistanceAbovePlayer();

        // Store all correct raycast hits and store the smallestHitDistance
        float _smallestHitDistance = stepHeight + 0.05f;
        Ray _downRay = new Ray(new Vector3(transform.position.x, transform.position.y - (height / 2 - stepHeight), transform.position.z), -Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_downRay, stepHeight + 0.5f, discludePlayer, QueryTriggerInteraction.Ignore);
        if (_hits != null)
        {
            foreach (RaycastHit _hit in _hits)
            {
                if (_hit.transform.gameObject != gameObject)
                {
                    if (_hit.distance < _smallestHitDistance)
                    {
                        _smallestHitDistance = _hit.distance;
                    }
                }
            }
        }

        // Check if the smallestHitDistance is < stepHeight + 0.05f. If so, calculate the distance to move up
        if (_smallestHitDistance < stepHeight + 0.05f)
        {
            float _distanceToMoveUp = stepHeight + 0.05f - _smallestHitDistance;
            // If distanceToMoveUp > the clear distance above the player, set distanceToMoveUp to distanceAbovePlayer.
            if (_distanceToMoveUp > _distanceAbovePlayer)
            {
                _distanceToMoveUp = _distanceAbovePlayer;
            }

            Vector3 newYPos = new Vector3(transform.position.x, transform.position.y + _distanceToMoveUp, transform.position.z);
            transform.position = newYPos;
        }
    }
    private void GetUpFromGroundWithOffset(Vector3 _direction)
    {
        float _distanceAbovePlayer = GetClearDistanceAbovePlayer();

        // Store all correct raycast hits and store the smallestHitDistance
        float _smallestHitDistance = stepHeight + 0.05f;
        Vector3 _raycastPosWithOffset = new Vector3(transform.position.x, transform.position.y - (height / 2 - stepHeight), transform.position.z);
        _raycastPosWithOffset -= _direction * stepSearchOffset;
        Ray _downRayInFront = new Ray(_raycastPosWithOffset, -Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_downRayInFront, stepHeight + 0.5f, discludePlayer, QueryTriggerInteraction.Ignore);
        if (_hits != null)
        {
            foreach (RaycastHit _hit in _hits)
            {
                if (_hit.transform.gameObject != gameObject)
                {
                    if (_hit.distance < _smallestHitDistance)
                    {
                        _smallestHitDistance = _hit.distance;
                    }
                }
            }
        }
        // Check if the smallestHitDistance is < stepHeight + 0.05f. If so, calculate the distance to move up
        if (_smallestHitDistance < stepHeight + 0.05f)
        {
            float _distanceToMoveUp = stepHeight + 0.05f - _smallestHitDistance;
            // If distanceToMoveUp > the clear distance above the player, set distanceToMoveUp to distanceAbovePlayer.
            if (_distanceToMoveUp > _distanceAbovePlayer)
            {
                _distanceToMoveUp = _distanceAbovePlayer;
            }

            Vector3 newYPos = new Vector3(transform.position.x, transform.position.y + _distanceToMoveUp, transform.position.z);
            transform.position = newYPos;
        }
    }

    private float GetClearDistanceAbovePlayer()
    {
        float _newDistanceToCeiling = height * 5;

        Ray _upRay = new Ray(transform.position, Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_upRay, height * 5, discludePlayer, QueryTriggerInteraction.Ignore);
        if (_hits != null)
        {
            float _smallestHitDistance = _newDistanceToCeiling;
            foreach (RaycastHit _hit in _hits)
            {
                if (_hit.transform.gameObject != gameObject)
                {
                    if (_hit.distance < _smallestHitDistance)
                    {
                        _smallestHitDistance = _hit.distance;
                    }
                }
            }
            _newDistanceToCeiling = _smallestHitDistance;
        }

        _newDistanceToCeiling -= height / 2;

        return _newDistanceToCeiling;
    }

    private bool IsGrounded()
    {
        bool _isGrounded = false;
        RaycastHit[] _hits;
        _hits = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y - (height / 2 - stepHeight), transform.position.z), -Vector3.up, stepHeight + 0.1f
                                    , groundedDisclude, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _hits.Length; i++)
        {
            RaycastHit _hit = _hits[i];
            if (_hit.transform.gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }

        return _isGrounded;
    }
    private bool IsGroundedInFront(Vector3 _direction)
    {
        bool _isGrounded = false;
        Vector3 _raycastPos = new Vector3(transform.position.x, transform.position.y - (height / 2 - stepHeight));
        _raycastPos += _direction * stepSearchOffset;
        RaycastHit[] _hits;
        _hits = Physics.RaycastAll(_raycastPos, -Vector3.up, stepHeight + 0.1f, groundedDisclude, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _hits.Length; i++)
        {
            RaycastHit _hit = _hits[i];
            if (_hit.transform.gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }

        return _isGrounded;
    }

    private Vector2 GetInputDirection(bool _input0, bool _input1, bool _input2, bool _input3)
    {
        Vector2 _inputDirection = Vector2.zero;
        if (_input0)
        {
            _inputDirection.y += 1;
        }
        if (_input1)
        {
            _inputDirection.y -= 1;
        }
        if (_input2)
        {
            _inputDirection.x -= 1;
        }
        if (_input3)
        {
            _inputDirection.x += 1;
        }

        return _inputDirection;
    }

    public void SetMovementVars(float _gravity, float _moveSpeed, float _runSpeedMultiplier, float _jumpSpeed)
    {
        gravity = _gravity;
        moveSpeed = _moveSpeed;
        runSpeedMultiplier = _runSpeedMultiplier;
        jumpSpeed = _jumpSpeed;
    }
}