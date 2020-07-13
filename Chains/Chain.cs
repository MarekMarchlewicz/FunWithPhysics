using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Chain : MonoBehaviour
{
    [SerializeField]
    private Rigidbody m_EndPoint;

    [SerializeField]
    private int m_SegmentsNum = 1;

    private Rigidbody m_Rigidbody;

    [Header("Segment Physics")]
    [SerializeField]
    private float m_Mass = 0.5f;
    [SerializeField]
    private float m_Drag = 0.05f;
    [SerializeField]
    private float m_AngularDrag = 0f;

    [Header("Joints")]
    [SerializeField]
    private float m_AngleLimit = 60f;
    [SerializeField]
    private float m_PositionDamper = 0f;
    [SerializeField]
    private float m_PositionSpring = 0f;
    [SerializeField]
    private RigidbodyInterpolation m_Interpolation = RigidbodyInterpolation.Interpolate;
    [SerializeField]
    private JointProjectionMode m_ProjectionMode;
    [SerializeField]
    private bool m_EnablePreProcessing = false;

    [SerializeField]
    private float m_Spring = 1f;
    [SerializeField]
    private float m_Damper = 5f;

    [SerializeField]
    private float m_MaxForce = 5f;

    [Header("Visuals")]
    [SerializeField]
    private LineRenderer m_LineRenderer;

    private List<Rigidbody> m_LineSegments;

    private void Start()
    {
        if (m_EndPoint == null)
            return;

        m_Rigidbody = GetComponent<Rigidbody>();

        if (Vector3.Distance(m_Rigidbody.position, m_EndPoint.position) < 0.01f)
            return;

        GenerateChain();
    }

    private void GenerateChain()
    {
        m_LineSegments = new List<Rigidbody>(m_SegmentsNum + 1);
        m_LineSegments.Add(m_Rigidbody);
        if (m_SegmentsNum > 1)
        {
            Vector3 step = (m_EndPoint.position - m_Rigidbody.position) / m_SegmentsNum;

            List<Rigidbody> additionalSegments = new List<Rigidbody>(m_SegmentsNum);
            for(int i = 0; i < m_SegmentsNum - 1; i++)
            {
                var newGO = new GameObject($"Segment {i}");
                newGO.transform.position = m_Rigidbody.position + (i * step);
                var rb = newGO.AddComponent<Rigidbody>();
                rb.mass = m_Mass;
                rb.drag = m_Drag;
                rb.angularDrag = m_AngularDrag;
                rb.interpolation = m_Interpolation;
                additionalSegments.Add(rb);
            }

            m_LineSegments.AddRange(additionalSegments);
        }

        m_LineSegments.Add(m_EndPoint);

        for(int i = 1; i < m_LineSegments.Count; i++)
        {
            var first = m_LineSegments[i - 1];
            var second = m_LineSegments[i];
            ConnectBodies(second, first);
        }
    }

    private void ConnectBodies(Rigidbody first, Rigidbody second)
    {
        var joint = first.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = second;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        joint.axis = Vector3.forward;
        joint.secondaryAxis = Vector3.zero;

        var jointDriver = new JointDrive { positionDamper = m_PositionDamper, positionSpring = m_PositionSpring, maximumForce = m_MaxForce };

        joint.highAngularXLimit = new SoftJointLimit { limit = m_AngleLimit / 2f};
        joint.lowAngularXLimit = new SoftJointLimit { limit = -m_AngleLimit / 2f};
        joint.angularYLimit = new SoftJointLimit { limit = m_AngleLimit / 2f };
        joint.angularZLimit = new SoftJointLimit { limit = m_AngleLimit / 2f};
        joint.angularXDrive = jointDriver;
        joint.angularYZDrive = jointDriver;

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = first.position - second.position;

        joint.projectionMode = m_ProjectionMode;

        joint.linearLimitSpring = new SoftJointLimitSpring { damper = m_Damper, spring = m_Spring };
        joint.enablePreprocessing = m_EnablePreProcessing;
    }

    private void Update()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        if (m_LineRenderer == null || m_LineSegments == null)
            return;

        m_LineRenderer.positionCount = m_LineSegments.Count;
        for(int i = 0; i < m_LineSegments.Count; i++)
        {
            m_LineRenderer.SetPosition(i, m_LineSegments[i].position);
        }
    }

    private void OnValidate()
    {
        if (m_SegmentsNum < 1)
            m_SegmentsNum = 1;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Physics/Select Configurable Joints")]
    public static void SelectAllConfigurableJoints()
    {
        UnityEditor.Selection.objects = GameObject.FindObjectsOfType<ConfigurableJoint>();
    }
#endif
}
