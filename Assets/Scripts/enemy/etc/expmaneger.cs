using UnityEngine;

public class ExpManager : MonoBehaviour
{
    [SerializeField] private int levelUpExp = 10;
    public int LevelUpExp => levelUpExp;

    [SerializeField] private float levelUpRate = 1.2f;

    public void IncreaseRequiredExp()
    {
        levelUpExp = Mathf.RoundToInt(levelUpExp * levelUpRate);
    }
}