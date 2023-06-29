// 스테이지 정보 저장용 클래스
using System;

public class StageData
{
    public readonly string stageName;
    public readonly string stageDescription;
    public readonly string stageType;
    public readonly int timeLimit;

    public StageData(string stageName, string stageDescription, 
                     string stageType, string timeLimit)
    {
        this.stageName = stageName;
        this.stageDescription = stageDescription;
        this.stageType = stageType;
        this.timeLimit = Convert.ToInt32(timeLimit);
    }
}