using UnityEngine;

public class ExpManager : MonoBehaviour
{
    [SerializeField] private float levelUpRequiredExp = 10;
    public float LevelUpRequiredExp => levelUpRequiredExp;

    [SerializeField] private float levelUpRate = 1.2f;

    public void IncreaseRequiredExp()
    {
        levelUpRequiredExp = levelUpRequiredExp * levelUpRate;
    }
}