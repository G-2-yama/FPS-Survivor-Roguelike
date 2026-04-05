[System.Flags]
public enum TeamType
{
    None    = 0,
    Player  = 1 << 0,
    Enemy   = 1 << 1,
    Boss    = 1 << 2
}
