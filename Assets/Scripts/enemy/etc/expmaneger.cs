using UnityEngine;

public class ExpManager : MonoBehaviour
{
    [SerializeField] private int levelUpRequiredExp = 10;
    public int LevelUpRequiredExp => levelUpRequiredExp;

    [SerializeField] private float levelUpRate = 1.2f;

    public void IncreaseRequiredExp()
    {
        levelUpRequiredExp = Mathf.RoundToInt(levelUpRequiredExp * levelUpRate);
    }
}