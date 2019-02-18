﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace HelpCharacter
{
    public class HelpCharacterController : MonoBehaviour
    {
        [SerializeField]
        private float _turnSpeed = 5.0f;
        
        [SerializeField]
        private Camera _mainCamera;

        private void Start()
        {
            if(_mainCamera == null)
             _mainCamera = Camera.main;

            if (SceneManager.GetActiveScene().name.Contains("Laboratory") && GameManager.Instance.LabLoaded)
                return;

            foreach (var helpMessage in gameObject.GetComponents<HelpMessage>())
                helpMessage.ShowMessage();
        }

        private void Update()
        {
            if (_mainCamera == null)
                return;            

            var direction = _mainCamera.transform.position - transform.position; // set direction of help character
            direction.Normalize(); //for look rotation direction vector needs to be orthogonal

            // slerp = Rotation from X to Y. X is current rotation, Y is where player direction vector is
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), _turnSpeed * Time.deltaTime);
        }
    }
}