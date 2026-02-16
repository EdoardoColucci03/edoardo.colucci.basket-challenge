using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryVisualizer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int linePoints = 50;
    [SerializeField] private float timeStep = 0.1f;

    [Header("Colors")]
    [SerializeField] private Color perfectColor = Color.green;
    [SerializeField] private Color nearPerfectColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color goodColor = Color.blue;
    [SerializeField] private Color nearGoodColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color badColor = Color.red;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = linePoints;
        lineRenderer.enabled = false;
    }

    public void ShowTrajectory(Vector3 startPos, Vector3 velocity, ShotPowerType powerType)
    {
        lineRenderer.enabled = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.startColor = GetColorForPowerType(powerType);
        lineRenderer.endColor = GetColorForPowerType(powerType);

        Vector3[] points = new Vector3[linePoints];

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeStep;
            points[i] = CalculatePositionAtTime(startPos, velocity, time);
        }

        lineRenderer.positionCount = linePoints;
        lineRenderer.SetPositions(points);
    }

    private Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 velocity, float time)
    {
        Vector3 gravity = Physics.gravity;
        return startPos + velocity * time + 0.5f * gravity * time * time;
    }

    private Color GetColorForPowerType(ShotPowerType type)
    {
        switch (type)
        {
            case ShotPowerType.Perfect: return perfectColor;
            case ShotPowerType.NearPerfect: return nearPerfectColor;
            case ShotPowerType.Good: return goodColor;
            case ShotPowerType.NearGood: return nearGoodColor;
            case ShotPowerType.Normal: return normalColor;
            default: return badColor;
        }
    }

    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }
}