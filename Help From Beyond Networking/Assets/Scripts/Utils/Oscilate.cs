using UnityEngine;

namespace Utils
{
    public class Oscilate : MonoBehaviour
    {
        // User Inputs
        public float amplitude = 0.5f;
        public float frequency = 1f;
        [SerializeField] Vector2 dir;
        private float offset;


// Position Storage Variables
        Vector2 posOffset;
        Vector2 tempPos;

// Use this for initialization
        void Start()
        {
// Store the starting position & rotation of the object
            posOffset = transform.position;
            dir.Normalize();
            offset = Random.Range(0, 100);
        }

// Update is called once per frame
        void Update()
        {
            tempPos = posOffset;
            float value = Mathf.Sin((Time.fixedTime * Mathf.PI * frequency) + offset) * amplitude;
            tempPos += dir * value;
            transform.position = tempPos;
        }
    }
}