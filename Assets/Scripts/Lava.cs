using System;
using UnityEngine;

namespace LavaJump
{
    public class Lava : MonoBehaviour
    {
        [SerializeField] private TimedPosition[] positions;

        private int _index;
        private float _speed;
        private float _currentTime;

        private void Start()
        {
            transform.position = positions[0].position;
            _speed = GetNext();
        }

        private void Update()
        {
            if (_index == positions.Length)
                return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                positions[_index].position,
                Time.deltaTime * _speed
            );

            _currentTime += Time.deltaTime;

            if (positions[_index].time - _currentTime < float.Epsilon)
            {
                _speed = GetNext();
            }
        }

        private float GetNext()
        {
            _index++;

            if (_index == positions.Length)
                return 0.0f;

            var speed = Vector2.Distance(positions[_index].position, positions[_index - 1].position) /
                        (positions[_index].time - positions[_index - 1].time);
            return speed;
        }

        [Serializable]
        private class TimedPosition
        {
            [SerializeField] public float time;
            [SerializeField] public Vector2 position;
        }
    }
}