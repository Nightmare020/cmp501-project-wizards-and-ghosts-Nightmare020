using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
    public class PositionIndicator : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private PlayerManager _otherPlayer;
     

        private void LateUpdate()
        {
            if (_playerManager == null)
            {
                _playerManager = GetComponentInParent<PlayerManager>();
            }

            if (_otherPlayer == null)
            {
                _otherPlayer = FindOtherPlayer();
            }

            if (_otherPlayer == null)
            {
                // Avoid null reference errors
                return;
            }

            Ray ray = new Ray(_playerManager.GetOtherPlayer().transform.position, transform.parent.position - _playerManager.GetOtherPlayer().transform.position);
            transform.position = ray.GetPoint(distance);
        }

        private PlayerManager FindOtherPlayer()
        {
            PlayerManager[] players = FindObjectsOfType<PlayerManager>();

            foreach (var player in players)
            {
                if (player != _playerManager)
                {
                    return player;
                }
            }

            // If no other player found
            return null;
        }
    }
}