using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DraggablesManager : MonoBehaviour
{
    [SerializeField]
    private bool m_UsePhysics = true;

    [SerializeField]
    private float m_MaxSpeed = 10f;

    [SerializeField]
    private float m_StoppableDistance = 1f;

    [SerializeField]
    private float m_MaxDistance = 40f;

    [SerializeField]
    private float m_MaxReleaseVelocity = 20f;

    private Rigidbody m_Draggable;

    private Camera m_Camera;

    private float distance;

    private RaycastHit[] m_Hits = new RaycastHit[16];

    private Vector2 m_LastPosition;

    private bool m_UsedGravity;
    private bool m_WasKinematic;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    private Vector3 m_TargetPos;

    private void FixedUpdate()
    {
        if (m_Draggable == null)
            return;

        if (m_UsePhysics)
        {
            var delta = m_TargetPos - m_Draggable.position;
            var desiredVelocity = delta / Time.deltaTime;
            var magnitude = desiredVelocity.magnitude;
            if (magnitude > m_MaxSpeed)
                magnitude = m_MaxSpeed;

            var dist = delta.magnitude;
            if (dist < m_StoppableDistance)
                magnitude *= dist / m_StoppableDistance;

            m_Draggable.velocity = desiredVelocity.normalized * magnitude;
        }
        else
        {
            m_Draggable.MovePosition(m_TargetPos);
        }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (Cursor.visible)
            {
                int hits = Physics.RaycastNonAlloc(m_Camera.ScreenPointToRay(Input.mousePosition), m_Hits, 500);
                for (int i = 0; i < hits; i++)
                {
                    var draggable = m_Hits[i].collider.GetComponent<Draggable>();
                    if (draggable != null)
                    {
                        m_Draggable = draggable.GetComponent<Rigidbody>();
                        m_UsedGravity = m_Draggable.useGravity;
                        m_WasKinematic = m_Draggable.isKinematic;
                        m_Draggable.useGravity = false;
                        m_Draggable.isKinematic = false;
                        distance = m_Camera.WorldToScreenPoint(m_Draggable.position).z;
                        if (distance > m_MaxDistance)
                            distance = m_MaxDistance;

                        draggable.StartDragging();

                        break;
                    }
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (m_Draggable != null)
            {
                m_Draggable.useGravity = m_UsedGravity;
                m_Draggable.isKinematic = m_WasKinematic;

                var magnitude = m_Draggable.velocity.magnitude;
                if (magnitude > m_MaxReleaseVelocity)
                    magnitude = m_MaxReleaseVelocity;
                m_Draggable.velocity = m_Draggable.velocity.normalized * magnitude;
                m_Draggable.GetComponent<Draggable>().StopDragging();
                m_Draggable = null;
            }
        }
        else
        {
            if (m_Draggable != null)
            {
                var currentPosition = Input.mousePosition;

                var pos3D = new Vector3(currentPosition.x, currentPosition.y, distance);
                 m_TargetPos = m_Camera.ScreenToWorldPoint(pos3D);
            }
        }
    }
}
