using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform powerBarFill;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform zonesContainer;

    [Header("Power Zones (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float perfectZoneStart = 0.65f;
    [Range(0f, 1f)]
    [SerializeField] private float perfectZoneEnd = 0.75f;
    [Range(0f, 1f)]
    [SerializeField] private float goodZoneStart = 0.78f;
    [Range(0f, 1f)]
    [SerializeField] private float goodZoneEnd = 0.92f;

    [Header("Tolerance Zones - Near Perfect/Good")]
    [Range(0f, 0.2f)]
    [SerializeField] private float nearPerfectTolerance = 0.08f;
    [Range(0f, 0.2f)]
    [SerializeField] private float nearGoodTolerance = 0.10f;

    [Header("Colors")]
    [SerializeField] private Color perfectColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color goodColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.7f);

    private float maxHeight;
    private GameObject perfectZoneObj;
    private GameObject goodZoneObj;

    private void Start()
    {
        maxHeight = powerBarFill.parent.GetComponent<RectTransform>().rect.height;
        fillImage.color = fillColor;
        CreateZones();
    }

    private void Update()
    {
        UpdateZonesInRealtime();
    }

    private void CreateZones()
    {
        perfectZoneObj = CreateZone("PerfectZone", perfectZoneStart, perfectZoneEnd, perfectColor);
        goodZoneObj = CreateZone("GoodZone", goodZoneStart, goodZoneEnd, goodColor);
    }

    private GameObject CreateZone(string zoneName, float startNormalized, float endNormalized, Color color)
    {
        GameObject zone = new GameObject(zoneName);
        zone.transform.SetParent(zonesContainer, false);

        RectTransform zoneRect = zone.AddComponent<RectTransform>();
        Image zoneImage = zone.AddComponent<Image>();

        zoneImage.color = color;

        zoneRect.anchorMin = new Vector2(0, startNormalized);
        zoneRect.anchorMax = new Vector2(1, endNormalized);
        zoneRect.offsetMin = Vector2.zero;
        zoneRect.offsetMax = Vector2.zero;

        return zone;
    }

    private void UpdateZonesInRealtime()
    {
        if (perfectZoneObj != null)
        {
            RectTransform rect = perfectZoneObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, perfectZoneStart);
            rect.anchorMax = new Vector2(1, perfectZoneEnd);
        }

        if (goodZoneObj != null)
        {
            RectTransform rect = goodZoneObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, goodZoneStart);
            rect.anchorMax = new Vector2(1, goodZoneEnd);
        }
    }

    public void UpdatePower(float normalizedPower)
    {
        normalizedPower = Mathf.Clamp01(normalizedPower);

        powerBarFill.anchorMin = new Vector2(0, 0);
        powerBarFill.anchorMax = new Vector2(1, normalizedPower);
        powerBarFill.offsetMin = Vector2.zero;
        powerBarFill.offsetMax = Vector2.zero;
    }

    public ShotPowerType GetShotPowerType(float normalizedPower)
    {
        if (normalizedPower >= perfectZoneStart && normalizedPower <= perfectZoneEnd)
        {
            return ShotPowerType.Perfect;
        }
        else if (IsNearPerfect(normalizedPower))
        {
            return ShotPowerType.NearPerfect;
        }
        else if (normalizedPower >= goodZoneStart && normalizedPower <= goodZoneEnd)
        {
            return ShotPowerType.Good;
        }
        else if (IsNearGood(normalizedPower))
        {
            return ShotPowerType.NearGood;
        }
        else if (normalizedPower < perfectZoneStart - nearPerfectTolerance)
        {
            return ShotPowerType.Weak;
        }
        else if (normalizedPower > goodZoneEnd + nearGoodTolerance)
        {
            return ShotPowerType.TooStrong;
        }
        else
        {
            return ShotPowerType.Normal;
        }
    }

    private bool IsNearPerfect(float power)
    {
        float distanceToStart = Mathf.Abs(power - perfectZoneStart);
        float distanceToEnd = Mathf.Abs(power - perfectZoneEnd);
        float minDistance = Mathf.Min(distanceToStart, distanceToEnd);

        return minDistance <= nearPerfectTolerance &&
               (power < perfectZoneStart || power > perfectZoneEnd);
    }

    private bool IsNearGood(float power)
    {
        float distanceToStart = Mathf.Abs(power - goodZoneStart);
        float distanceToEnd = Mathf.Abs(power - goodZoneEnd);
        float minDistance = Mathf.Min(distanceToStart, distanceToEnd);

        return minDistance <= nearGoodTolerance &&
               (power < goodZoneStart || power > goodZoneEnd);
    }
}

