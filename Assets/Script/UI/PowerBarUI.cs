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
    [SerializeField] private RectTransform basketballIcon;
    private float rotationSpeed = 180f;
    [SerializeField] private RectTransform fillIndicator;

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

    [Header("Glow Settings")]
    [SerializeField] private float glowSpeed = 4f;
    [SerializeField] private float glowMinAlpha = 0.5f;
    [SerializeField] private float glowMaxAlpha = 1f;

    [Header("Zone Shuffle Settings")]
    [SerializeField] private float zoneShuffleDuration = 0.4f;
    [Tooltip("Ogni quanti punti scatta il primo shuffle - si riduce col punteggio")]
    [SerializeField] private int shuffleIntervalStart = 20;
    [Tooltip("Intervallo minimo tra shuffle ad alto punteggio")]
    [SerializeField] private int shuffleIntervalMin = 6;
    [Tooltip("Punteggio a cui si raggiunge l'intervallo minimo")]
    [SerializeField] private int shuffleMaxDifficultyScore = 100;
    [Tooltip("Posizione minima (0-1) da cui le zone possono apparire")]
    [SerializeField] private float minZonePosition = 0.5f;

    [Header("Zone Size Progression")]
    [Tooltip("Dimensione minima zona perfect ad alto punteggio")]
    [SerializeField] private float perfectSizeMin = 0.05f;
    [Tooltip("Dimensione minima zona good ad alto punteggio")]
    [SerializeField] private float goodSizeMin = 0.07f;
    [Tooltip("Distanza fissa tra le due zone")]
    [SerializeField] private float minZoneGap = 0.03f;

    private float perfectSizeStart;
    private float goodSizeStart;

    private float maxHeight;
    private GameObject perfectZoneObj;
    private GameObject goodZoneObj;
    private Image perfectZoneImage;
    private Image goodZoneImage;

    private float currentPower = 0f;
    private int lastShuffleScore = 0;
    private bool isShuffling = false;
    private bool isFrozenBar = false;

    private float snapshotPerfectZoneStart;
    private float snapshotPerfectZoneEnd;
    private float snapshotGoodZoneStart;
    private float snapshotGoodZoneEnd;

    private int activeShuffleIntervalStart;
    private int activeShuffleIntervalMin;
    private float activePerfectSizeMin;
    private float activeGoodSizeMin;

    private void Start()
    {
        maxHeight = powerBarFill.parent.GetComponent<RectTransform>().rect.height;
        fillImage.color = fillColor;

        perfectSizeStart = perfectZoneEnd - perfectZoneStart;
        goodSizeStart = goodZoneEnd - goodZoneStart;

        CreateZones();
        InitDifficultySettings();
        TakeSnapshot();
    }

    private void InitDifficultySettings()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameMode != GameMode.VsAI)
        {
            activeShuffleIntervalStart = shuffleIntervalStart;
            activeShuffleIntervalMin = shuffleIntervalMin;
            activePerfectSizeMin = perfectSizeMin;
            activeGoodSizeMin = goodSizeMin;
            return;
        }

        switch (GameManager.Instance.CurrentDifficulty)
        {
            case AIDifficulty.Easy:
                activeShuffleIntervalStart = 25;
                activeShuffleIntervalMin = 10;
                activePerfectSizeMin = 0.08f;
                activeGoodSizeMin = 0.10f;
                break;
            case AIDifficulty.Normal:
                activeShuffleIntervalStart = shuffleIntervalStart;
                activeShuffleIntervalMin = shuffleIntervalMin;
                activePerfectSizeMin = perfectSizeMin;
                activeGoodSizeMin = goodSizeMin;
                break;
            case AIDifficulty.Hard:
                activeShuffleIntervalStart = 15;
                activeShuffleIntervalMin = 4;
                activePerfectSizeMin = 0.07f;
                activeGoodSizeMin = 0.08f;
                break;
        }
    }

    private void Update()
    {
        UpdateZonesInRealtime();
        UpdateGlow();
        CheckScoreForShuffle();
    }

    private void CreateZones()
    {
        perfectZoneObj = CreateZone("PerfectZone", perfectZoneStart, perfectZoneEnd, perfectColor);
        goodZoneObj = CreateZone("GoodZone", goodZoneStart, goodZoneEnd, goodColor);
        perfectZoneImage = perfectZoneObj.GetComponent<Image>();
        goodZoneImage = goodZoneObj.GetComponent<Image>();
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
        if (isShuffling) return;

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

    private void UpdateGlow()
    {
        if (perfectZoneImage == null || goodZoneImage == null) return;

        ShotPowerType powerType = GetShotPowerType(currentPower);

        float t = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);

        if (powerType == ShotPowerType.Perfect)
            perfectZoneImage.color = Color.Lerp(perfectColor, Color.white, t * 0.9f);
        else
            perfectZoneImage.color = perfectColor;

        if (powerType == ShotPowerType.Good)
            goodZoneImage.color = Color.Lerp(goodColor, Color.white, t * 0.9f);
        else
            goodZoneImage.color = goodColor;
    }

    private void CheckScoreForShuffle()
    {
        if (GameManager.Instance == null || isShuffling || isFrozenBar) return;

        int score = GameManager.Instance.GetTotalScore();
        int interval = GetCurrentShuffleInterval(score);

        if (score >= lastShuffleScore + interval)
        {
            lastShuffleScore = score;
            StartCoroutine(ShuffleZones(score));
        }
    }

    private int GetCurrentShuffleInterval(int score)
    {
        float t = Mathf.Clamp01((float)score / shuffleMaxDifficultyScore);
        return Mathf.RoundToInt(Mathf.Lerp(activeShuffleIntervalStart, activeShuffleIntervalMin, t));
    }

    private float GetCurrentZoneSize(float sizeStart, float sizeMin, int score)
    {
        float t = Mathf.Clamp01((float)score / shuffleMaxDifficultyScore);
        return Mathf.Lerp(sizeStart, sizeMin, t);
    }

    private void TakeSnapshot()
    {
        snapshotPerfectZoneStart = perfectZoneStart;
        snapshotPerfectZoneEnd = perfectZoneEnd;
        snapshotGoodZoneStart = goodZoneStart;
        snapshotGoodZoneEnd = goodZoneEnd;
    }

    private IEnumerator ShuffleZones(int score)
    {
        isShuffling = true;

        float perfectSize = GetCurrentZoneSize(perfectSizeStart, activePerfectSizeMin, score);
        float goodSize = GetCurrentZoneSize(goodSizeStart, activeGoodSizeMin, score);

        bool inverted = Random.value > 0.5f;
        float gap = minZoneGap;

        float totalSpace = perfectSize + goodSize + gap;
        float maxStart = 1f - totalSpace - 0.02f;
        float startA = Random.Range(minZonePosition, Mathf.Max(minZonePosition, maxStart));

        float newPerfectStart, newPerfectEnd, newGoodStart, newGoodEnd;

        if (!inverted)
        {
            newPerfectStart = startA;
            newPerfectEnd = startA + perfectSize;
            newGoodStart = newPerfectEnd + gap;
            newGoodEnd = newGoodStart + goodSize;
        }
        else
        {
            newGoodStart = startA;
            newGoodEnd = startA + goodSize;
            newPerfectStart = newGoodEnd + gap;
            newPerfectEnd = newPerfectStart + perfectSize;
        }

        newPerfectEnd = Mathf.Min(newPerfectEnd, 0.98f);
        newGoodEnd = Mathf.Min(newGoodEnd, 0.98f);
        newPerfectStart = Mathf.Min(newPerfectStart, newPerfectEnd - perfectSize);
        newGoodStart = Mathf.Min(newGoodStart, newGoodEnd - goodSize);

        float elapsed = 0f;
        float oldPerfectStart = perfectZoneStart;
        float oldPerfectEnd = perfectZoneEnd;
        float oldGoodStart = goodZoneStart;
        float oldGoodEnd = goodZoneEnd;

        while (elapsed < zoneShuffleDuration)
        {
            float t = elapsed / zoneShuffleDuration;
            float smooth = t * t * (3f - 2f * t);

            perfectZoneStart = Mathf.Lerp(oldPerfectStart, newPerfectStart, smooth);
            perfectZoneEnd = Mathf.Lerp(oldPerfectEnd, newPerfectEnd, smooth);
            goodZoneStart = Mathf.Lerp(oldGoodStart, newGoodStart, smooth);
            goodZoneEnd = Mathf.Lerp(oldGoodEnd, newGoodEnd, smooth);

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

            elapsed += Time.deltaTime;
            yield return null;
        }

        perfectZoneStart = newPerfectStart;
        perfectZoneEnd = newPerfectEnd;
        goodZoneStart = newGoodStart;
        goodZoneEnd = newGoodEnd;

        TakeSnapshot();

        isShuffling = false;
    }

    public void FreezeBar()
    {
        isFrozenBar = true;
        TakeSnapshot();
    }

    public void UpdatePower(float normalizedPower)
    {
        if (isFrozenBar) return;

        normalizedPower = Mathf.Clamp01(normalizedPower);
        currentPower = normalizedPower;

        powerBarFill.anchorMin = new Vector2(0, 0);
        powerBarFill.anchorMax = new Vector2(1, normalizedPower);
        powerBarFill.offsetMin = Vector2.zero;
        powerBarFill.offsetMax = Vector2.zero;

        if (basketballIcon != null)
        {
            float dir = normalizedPower > 0.01f ? -1f : 0f;
            basketballIcon.Rotate(0, 0, dir * rotationSpeed * Time.deltaTime);
        }

        if (fillIndicator != null)
        {
            Vector3[] corners = new Vector3[4];
            powerBarFill.GetWorldCorners(corners);
            Vector3 topCenter = (corners[1] + corners[2]) / 2f;
            fillIndicator.position = topCenter;
        }
    }

    public void ResetBar()
    {
        isFrozenBar = false;
        UpdatePower(0f);
    }

    public ShotPowerType GetShotPowerType(float normalizedPower)
    {
        float pStart = isFrozenBar ? snapshotPerfectZoneStart : perfectZoneStart;
        float pEnd = isFrozenBar ? snapshotPerfectZoneEnd : perfectZoneEnd;
        float gStart = isFrozenBar ? snapshotGoodZoneStart : goodZoneStart;
        float gEnd = isFrozenBar ? snapshotGoodZoneEnd : goodZoneEnd;

        if (normalizedPower >= pStart && normalizedPower <= pEnd)
            return ShotPowerType.Perfect;

        if (normalizedPower >= gStart && normalizedPower <= gEnd)
            return ShotPowerType.Good;

        if (IsNearZone(normalizedPower, pStart, pEnd, nearPerfectTolerance))
            return ShotPowerType.NearPerfect;

        if (IsNearZone(normalizedPower, gStart, gEnd, nearGoodTolerance))
            return ShotPowerType.NearGood;

        float lowerZoneBottom = Mathf.Min(pStart, gStart);
        float upperZoneTop = Mathf.Max(pEnd, gEnd);

        if (normalizedPower < lowerZoneBottom - nearPerfectTolerance)
            return ShotPowerType.Weak;

        if (normalizedPower > upperZoneTop + nearGoodTolerance)
            return ShotPowerType.TooStrong;

        return ShotPowerType.Normal;
    }

    private bool IsNearZone(float power, float zoneStart, float zoneEnd, float tolerance)
    {
        float distanceToStart = Mathf.Abs(power - zoneStart);
        float distanceToEnd = Mathf.Abs(power - zoneEnd);
        float minDistance = Mathf.Min(distanceToStart, distanceToEnd);
        return minDistance <= tolerance && (power < zoneStart || power > zoneEnd);
    }
}