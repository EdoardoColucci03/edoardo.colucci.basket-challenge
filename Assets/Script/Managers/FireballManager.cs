using System.Collections;
using UnityEngine;

public class FireballManager : MonoBehaviour
{
    public static FireballManager Instance;

    [Header("Fireball Settings")]
    [SerializeField] private int basketsToActivate = 4;
    [SerializeField] private float fireballDuration = 8f;

    private int consecutiveBaskets = 0;
    private bool isFireballActive = false;
    private float fireballTimeLeft = 0f;
    private Coroutine fireballCoroutine;

    public bool IsFireballActive => isFireballActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isFireballActive)
            fireballTimeLeft -= Time.deltaTime;
    }

    public void OnBasketScored()
    {
        if (isFireballActive) return;

        consecutiveBaskets++;
        GameplayUI.Instance?.UpdateFireballBar(consecutiveBaskets, basketsToActivate);

        Debug.Log($"<color=orange>[Fireball] Consecutive baskets: {consecutiveBaskets}/{basketsToActivate}</color>");

        if (consecutiveBaskets >= basketsToActivate)
        {
            consecutiveBaskets = 0;
            isFireballActive = true;
            fireballTimeLeft = fireballDuration;

            if (fireballCoroutine != null)
                StopCoroutine(fireballCoroutine);

            fireballCoroutine = StartCoroutine(FireballExpireRoutine());
            AudioManager.Instance?.PlayFireballActive();

            GameplayUI.Instance?.ShowFireball(true);
            Debug.Log("<color=red>[Fireball] ACTIVATED </color>");
        }
    }

    public void OnMiss()
    {
        if (consecutiveBaskets == 0 && !isFireballActive) return;

        consecutiveBaskets = 0;
        GameplayUI.Instance?.UpdateFireballBar(0, basketsToActivate);

        if (isFireballActive)
        {
            DeactivateFireball();
            return;
        }

        Debug.Log("<color=grey>[Fireball] Miss");
    }

    public int ApplyMultiplier(int points)
    {
        return isFireballActive ? points * 2 : points;
    }

    public void ResetState()
    {
        consecutiveBaskets = 0;
        DeactivateFireball();
        GameplayUI.Instance?.UpdateFireballBar(0, basketsToActivate);
    }

    private void DeactivateFireball()
    {
        isFireballActive = false;
        fireballTimeLeft = 0f;

        if (fireballCoroutine != null)
        {
            StopCoroutine(fireballCoroutine);
            fireballCoroutine = null;
        }

        GameplayUI.Instance?.ShowFireball(false);
        AudioManager.Instance?.StopFireballLoop();
        Debug.Log("<color=grey>[Fireball] Deactivated</color>");
    }

    private IEnumerator FireballExpireRoutine()
    {
        yield return new WaitForSeconds(fireballDuration);
        Debug.Log("<color=grey>[Fireball] Expired</color>");
        DeactivateFireball();
    }
}