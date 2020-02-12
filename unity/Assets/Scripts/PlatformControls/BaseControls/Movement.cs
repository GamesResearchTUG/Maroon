﻿using UnityEngine;

namespace PlatformControls.BaseControls
{
    public abstract class Movement : MonoBehaviour
    {
        protected bool IsMoving = false;

        [SerializeField]
        protected float Speed = 20.0f;

        [SerializeField]
        protected GameObject MinPosition;

        [SerializeField]
        protected GameObject MaxPosition;

        [SerializeField]
        protected bool SimulationRunningDuringMovement = false;

        protected void StartMoving()
        {
            IsMoving = true;

            if (!SimulationRunningDuringMovement)
                SimulationController.Instance.StartSimulation();
        }

        protected void StopMoving()
        {
            IsMoving = false;

            if (!SimulationRunningDuringMovement)
                SimulationController.Instance.StopSimulation();
        }

        protected void Move(Vector3 target)
        {
            if(!IsMoving)
                StartMoving();

            var maxDistance = Vector3.Distance(MinPosition.transform.position, MaxPosition.transform.position);

            var newPosition = Vector3.MoveTowards(transform.position, target, 10.0f);

            if (Vector3.Distance(newPosition, MinPosition.transform.position) <= maxDistance
                && Vector3.Distance(newPosition, MaxPosition.transform.position) <= maxDistance)
            {
                transform.position = newPosition;
            } 
        }
    }
}