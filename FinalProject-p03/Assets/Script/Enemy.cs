using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace UnityStandardAssets.Characters.FirstPerson{
    public class Enemy : MonoBehaviour
    {

        public Transform[] points;
        public int pointIndex;
        public Transform currentPoint;
        public float moveSpeed;


        void Start()
        {
            InitializeNewPoint();

        }

        void InitializeNewPoint()
        {
            int rand = Random.Range(0, points.Length);
            pointIndex = rand;
            currentPoint = points[pointIndex];
            fwd = ((currentPoint.position - transform.position));
            StartCoroutine(GoToTarget());
        }

        void Update()
        {

        }

        private Vector3 fwd;

        IEnumerator GoToTarget()
        {
            while (Math.Abs(transform.position.x - currentPoint.position.x) > 0.1f)
            {

                transform.forward = fwd;
                transform.position =
                    Vector3.Lerp(transform.position, currentPoint.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            InitializeNewPoint();
        }





    }


}
