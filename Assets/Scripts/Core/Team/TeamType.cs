[System.Flags]
public enum TeamType
{
    None    = 0,
    Player  = 1 << 0,
    PlayerAmmo = 1 << 1,
    Enemy   = 1 << 2,
    EnemyAmmo = 1 << 3,
    Boss    = 1 << 4
}
