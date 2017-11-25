﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using RPG.Characters; // So we can detectect by type

namespace RPG.CameraUI
{
    public class CameraRaycaster : MonoBehaviour
    {
		[SerializeField] Texture2D walkCursor = null;
        [SerializeField] Texture2D enemyCursor = null;
		[SerializeField] Vector2 cursorHotspot = new Vector2(0, 0);
        [SerializeField] float minZoom = 10f;
        [SerializeField] float maxZoom = 30f;
        [SerializeField] float scrollSpeed = 5f;

        float cameraDistance = 0f;

        const int POTENTIALLY_WALKABLE_LAYER = 8;
        float maxRaycastDepth = 100f; // Hard coded value

        Rect currentScrenRect;

        public delegate void OnMouseOverEnemy(EnemyAI enemy);
		public event OnMouseOverEnemy onMouseOverEnemy;

		public delegate void OnMouseOverTerrain(Vector3 destination);
        public event OnMouseOverTerrain onMouseOverPotentiallyWalkable;

		void Update()
        {
            currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);

            // Check if pointer is over an interactable UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // Impliment UI interaction
                PerformRaycasts(); // add to stop UI problem
            }
            else
            {
                PerformRaycasts();
            }
            cameraDistance += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            if (transform.localPosition.y < minZoom && cameraDistance > 0)
                cameraDistance = 0;
            if (transform.localPosition.y > maxZoom && cameraDistance < 0)
                cameraDistance = 0;
            transform.Translate(new Vector3(0, 0, cameraDistance) * 2, Space.Self);
            cameraDistance = 0;
        }

        void PerformRaycasts()
		{
            if (currentScrenRect.Contains(Input.mousePosition))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Specify layer priorities below, order matters
                if (RaycastForEnemy(ray)) { return; }
                if (RaycastForPotentiallyWalkable(ray)) { return; }
            }
		}

	    bool RaycastForEnemy(Ray ray)
		{
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo, maxRaycastDepth);
            var gameObjectHit = hitInfo.collider.gameObject;
            var enemyHit = gameObjectHit.GetComponent<EnemyAI>();
            if (enemyHit)
            {
                Cursor.SetCursor(enemyCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverEnemy(enemyHit);
                return true;
            }
            return false;
		}

        private bool RaycastForPotentiallyWalkable(Ray ray)
        {
            RaycastHit hitInfo;
            LayerMask potentiallyWalkableLayer = 1 << POTENTIALLY_WALKABLE_LAYER;
            bool potentiallyWalkableHit = Physics.Raycast(ray, out hitInfo, maxRaycastDepth, potentiallyWalkableLayer);
            if (potentiallyWalkableHit)
            {
                Cursor.SetCursor(walkCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverPotentiallyWalkable(hitInfo.point);
                return true;
            }
            return false;
        }
    }
}