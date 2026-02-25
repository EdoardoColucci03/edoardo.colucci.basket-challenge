using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPositionManager : MonoBehaviour
{
    [Header("Spawn Positions")]
    [SerializeField] private Transform[] spawnPositions;

    [Header("Selection Mode")]
    [SerializeField] private SelectionMode selectionMode = SelectionMode.Random;

    private int currentIndex = 0;

    public Transform GetNextSpawnPosition()
    {
        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            Debug.LogError("[SpawnPositionManager] No spawn positions assigned!");
            return transform;
        }

        Transform selectedPosition = null;

        switch (selectionMode)
        {
            case SelectionMode.Random:
                selectedPosition = GetRandomPosition();
                break;

            case SelectionMode.Sequential:
                selectedPosition = GetSequentialPosition();
                break;
        }

        //Debug.Log($"<color=magenta>[SpawnPositionManager] Next player position: {selectedPosition.name}</color>");
        return selectedPosition;
    }

    private Transform GetRandomPosition()
    {
        int randomIndex = Random.Range(0, spawnPositions.Length);
        return spawnPositions[randomIndex];
    }

    private Transform GetSequentialPosition()
    {
        Transform position = spawnPositions[currentIndex];
        currentIndex = (currentIndex + 1) % spawnPositions.Length;
        return position;
    }

    private void OnDrawGizmos()
    {
        if (spawnPositions == null || spawnPositions.Length == 0) return;

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            if (spawnPositions[i] == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPositions[i].position, 0.3f);

            Gizmos.color = Color.white;
            Vector3 labelPos = spawnPositions[i].position + Vector3.up * 0.5f;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"Player Pos {i + 1}");
#endif
        }
    }
}

public enum SelectionMode
{
    Random,
    Sequential
}
