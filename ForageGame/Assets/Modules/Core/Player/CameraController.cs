using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TDK.PlayerSystem;

namespace TDK.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        public float followZDistance;
        public float followYDistance;
        public float followSharpness;
        public float velocityWeight;
        public float lookingDirectionOffset;

        void LateUpdate()
        {
            if (!Player.Instance) return;

            Vector3 targetPos = Player.Instance.transform.position;

            targetPos.y += followYDistance;
            targetPos.z -= followZDistance;
            targetPos += velocityWeight * Player.Instance.playerController._rigidbody.linearVelocity;
            targetPos += lookingDirectionOffset * Player.Instance.playerController.ViewDirection;

            transform.position = Vector3.Lerp(transform.position, targetPos, followSharpness * Time.deltaTime);

            //transform.LookAt(Player.Instance.transform.position);
            // transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, 0);
        }
    }
}