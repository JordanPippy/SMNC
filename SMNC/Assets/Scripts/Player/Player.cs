using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Player
{
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

        public override void OnNetworkSpawn()
        {
            /*
            if (IsOwner)
            {
                Move();
            }
            */
            position.Value = new Vector3(5.12f, 6f, 4.747f);
        }
        public void Move(Vector3 newPosition)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = handleMovement(newPosition, position.Value);
                transform.position = randomPosition;
                position.Value = randomPosition;
            }
            else
            {
                if (IsOwner)
                    SubmitPositionRequestServerRpc(newPosition);
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
        {
            position.Value = handleMovement(newPosition, position.Value);
        }

        static Vector3 handleMovement(Vector3 newPosition, Vector3 oldPosition)
        {
            newPosition *= 2.0f;
            return oldPosition + newPosition;

        }

        void Update()
        {
            transform.position = position.Value;
        }
    }
}