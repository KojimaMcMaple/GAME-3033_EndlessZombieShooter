/// <summary>
///  The Source file name: GlobalEnums.cs
///  Author's name: Trung Le (Kyle Hunter)
///  Student Number: 101264698
///  Program description: Global game manager script
///  Date last Modified: See GitHub
///  Revision History: See GitHub
/// </summary>
public static class GlobalEnums
{
    public enum BulletDir
    {
        DEFAULT,
        LEFT,
        RIGHT,
        UP,
        DOWN
    };

    public enum ObjType
    {
        DEFAULT,
        PLAYER,
        ENEMY,
        BOSS,
        NUM_OF_TYPE
    };

    public enum FoodType
    {
        DEFAULT,
        LOW,
        HIGH,
        BEYOND,
        NUM_OF_TYPE
    };

    public enum EnemyState
    {
        IDLE,
        MOVE_TO_TARGET,
        ATTACK
    };

    public enum MovingPlatformDir
    {
        HORIZONTAL,
        VERTICAL,
        UP_RIGHT,
        DOWN_RIGHT,
        UP_LEFT,
        DOWN_LEFT,
        NUM_OF_DIR,
    };
}
