// 태그 관리용 전역 클래스
// 태그 추가시 갱신 요망
public static class Tags
{
    public enum Layers
    { 
        Default = 0,
        TransparentFx = 1,
        IgnoreLaycast = 2,
        Water = 4,
        UI = 5
    }

    public readonly static string Player = "Player";
    public readonly static string Platform = "Platform";
    public readonly static string MovingPlatform = "MovingPlatform";
    public readonly static string RotatingPlatform = "RotatingPlatform";
    public readonly static string TOFPlatform = "TOFPlatform";
    public readonly static string Fall = "Fall";
    public readonly static string SavePoint = "SavePoint";
    public readonly static string Goal = "Goal";
    public readonly static string OutArea = "OutArea";
    public readonly static string Obstacle = "Obstacle";
}