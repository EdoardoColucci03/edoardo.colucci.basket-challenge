using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BonusRarity
{
    Common,   
    Rare,     
    Epic      
}

public class BackboardBonus
{
    public BonusRarity Rarity { get; private set; }
    public int Points { get; private set; }
    public Color Color { get; private set; }
    public string Label { get; private set; }

    private BackboardBonus(BonusRarity rarity, int points, Color color, string label)
    {
        Rarity = rarity;
        Points = points;
        Color = color;
        Label = label;
    }

    private static readonly float CommonWeight = 0.50f;
    private static readonly float RareWeight = 0.30f;

    public static BackboardBonus GetRandom()
    {
        float roll = Random.value;

        if (roll < CommonWeight)
            return Common();

        if (roll < CommonWeight + RareWeight)
            return Rare();

        return Epic();
    }

    public static BackboardBonus Common() => new BackboardBonus(
        BonusRarity.Common,
        4,
        new Color(0.9f, 0.9f, 0.2f),   
        "+4"
    );

    public static BackboardBonus Rare() => new BackboardBonus(
        BonusRarity.Rare,
        6,
        new Color(0.4f, 0.4f, 1.0f),
        "+6"
    );

    public static BackboardBonus Epic() => new BackboardBonus(
        BonusRarity.Epic,
        8,
        new Color(0.8f, 0.2f, 1.0f),
        "+8"
    );
}
