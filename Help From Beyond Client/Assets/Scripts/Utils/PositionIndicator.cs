using UnityEngine;

namespace Utils
{
    public class PositionIndicator : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField] private PlayerManager _playerManager;
     

        private void LateUpdate()
        {
            Ray ray = new Ray(_playerManager.GetOtherPlayer().transform.position, transform.parent.position - _playerManager.GetOtherPlayer().transform.position);
            transform.position = ray.GetPoint(distance);
        }
    }
}