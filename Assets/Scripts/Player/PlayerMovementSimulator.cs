using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovementSimulator : MonoBehaviour
{
    public PlayerController playerController;
    public CapsuleCollider parentCol;
    public CapsuleCollider thisCol;

    private float yVelocity;

    public void SimulateNewMovement(Vector3 _serverPosition, float _yVelocity)
    {
        StopAllCoroutines();

        //Debug.Log("Simulate");
        parentCol.enabled = false;
        thisCol.enabled = true;

        transform.position = _serverPosition;
        yVelocity = _yVelocity;

        // Now add the client's new movement predictions
        int _minimumKey = playerController.movementRequestsInputs.Keys.Min();
        int _maximumKey = playerController.movementRequestsInputs.Keys.Max();
        for (int i = _minimumKey; i <= _maximumKey; i++)
        {
            playerController.movementRequestsInputs.TryGetValue(i, out bool[] _inputs);
            playerController.movementRequestsRotations.TryGetValue(i, out Quaternion _rotation);

            //clientPredictedMovements.Remove(i);
            StartCoroutine(PredictMovement(i, GetInputDirection(_inputs[0], _inputs[1], _inputs[2], _inputs[3]), _inputs[4], _inputs[5], _rotation));
        }


        playerController.playerManager.LerpMove(transform.position);
        playerController.gameObject.transform.rotation = transform.rotation;
        playerController.yVelocity = yVelocity;
        playerController.gameObject.transform.position = transform.position;

        thisCol.enabled = false;
        parentCol.enabled = true;
    }



    private IEnumerator PredictMovement(int _oldClientPredictMovementKey, Vector2 _inputDirection, bool _input4, bool _input5, Quaternion _lookRotation)
    {
        transform.rotation = _lookRotation;

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= playerController.moveSpeed;
        if (_input5)
        {
            _moveDirection *= playerController.runSpeedMultiplier;
            //playerController.camTransform.GetComponent<CameraController>().SetFieldOfView(FieldOfViewState.running);
        }
        //else
        //{
        //    playerController.camTransform.GetComponent<CameraController>().SetFieldOfView(FieldOfViewState.normal);
        //}

        if (IsGrounded())
        {
            yVelocity = 0;
            if (_input4)
            {
                yVelocity = playerController.jumpSpeed;
            }
        }
        yVelocity += playerController.gravity;
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
        if (Physics.Raycast(transform.position, _moveDirection, out RaycastHit _hit, Vector3.Distance(Vector3.zero, _moveDirection), playerController.discludePlayer, QueryTriggerInteraction.Ignore))
        {
            _moveDirection = Vector3.zero;
        }


        Vector3 _newPosition = transform.position + _moveDirection;
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

        if (_oldClientPredictMovementKey == 0)
        {
            playerController.clientPredictedMovements.Add(playerController.nextClientPredictMoveId, transform.position);
            playerController.nextClientPredictMoveId++;
        }
        else
        {
            playerController.clientPredictedMovements[_oldClientPredictMovementKey] = transform.position;
        }

        yield return new WaitForSeconds(0);
    }


    private void CollisionCheck()
    {
        // Check with body collider
        Collider[] overlaps = new Collider[10];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(playerController.bodyCollider.center), playerController.bodyCollider.radius, overlaps, playerController.discludePlayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < num; i++)
        {
            if (overlaps[i].gameObject == gameObject)
            {
                return;
            }
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(playerController.bodyCollider, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                transform.position = transform.position + penetrationVector;
            }
        }
    }

    private void GetUpFromGround()
    {
        float _distanceAbovePlayer = GetClearDistanceAbovePlayer();

        // Store all correct raycast hits and store the smallestHitDistance
        float _smallestHitDistance = playerController.stepHeight + 0.05f;
        Ray _downRay = new Ray(new Vector3(transform.position.x, transform.position.y - (playerController.height / 2 - playerController.stepHeight), transform.position.z), -Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_downRay, playerController.stepHeight + 0.5f, playerController.discludePlayer, QueryTriggerInteraction.Ignore);
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
        if (_smallestHitDistance < playerController.stepHeight + 0.05f)
        {
            float _distanceToMoveUp = playerController.stepHeight + 0.05f - _smallestHitDistance;
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
        float _smallestHitDistance = playerController.stepHeight + 0.05f;
        Vector3 _raycastPosWithOffset = new Vector3(transform.position.x, transform.position.y - (playerController.height / 2 - playerController.stepHeight), transform.position.z);
        _raycastPosWithOffset -= _direction * playerController.stepSearchOffset;
        Ray _downRayInFront = new Ray(_raycastPosWithOffset, -Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_downRayInFront, playerController.stepHeight + 0.5f, playerController.discludePlayer, QueryTriggerInteraction.Ignore);
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
        if (_smallestHitDistance < playerController.stepHeight + 0.05f)
        {
            float _distanceToMoveUp = playerController.stepHeight + 0.05f - _smallestHitDistance;
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
        float _newDistanceToCeiling = playerController.height * 5;

        Ray _upRay = new Ray(transform.position, Vector3.up);
        RaycastHit[] _hits = Physics.RaycastAll(_upRay, playerController.height * 5, playerController.discludePlayer, QueryTriggerInteraction.Ignore);
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

        _newDistanceToCeiling -= playerController.height / 2;

        return _newDistanceToCeiling;
    }

    private bool IsGrounded()
    {
        bool _isGrounded = false;
        RaycastHit[] _hits;
        _hits = Physics.RaycastAll(new Vector3(transform.position.x, transform.position.y - (playerController.height / 2 - playerController.stepHeight), transform.position.z), -Vector3.up,
                                                playerController.stepHeight + 0.1f, playerController.groundedDisclude, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _hits.Length; i++)
        {
            RaycastHit _hit = _hits[i];
            if (_hit.transform.gameObject != gameObject)
            {
                _isGrounded = true;
            }
        }

        return _isGrounded;
        //return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
    private bool IsGroundedInFront(Vector3 _direction)
    {
        bool _isGrounded = false;
        Vector3 _raycastPos = new Vector3(transform.position.x, transform.position.y - (playerController.height / 2 - playerController.stepHeight));
        _raycastPos += _direction * playerController.stepSearchOffset;
        RaycastHit[] _hits;
        _hits = Physics.RaycastAll(_raycastPos, -Vector3.up, playerController.stepHeight + 0.1f, playerController.groundedDisclude, QueryTriggerInteraction.Ignore);

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
}
