using System;
using UnityEngine;

public class BaseUpdater : IDisposable
{
    private float speed = 1.0f;
    private int score = 0;
    private int obstacleSum = 0;

    private System.Random random;

    public int Score => score;

    public BaseUpdater()
    {
        random = new System.Random(RunnerSimulation.FixedSeed);
    }

    public virtual void Update(InputType inputType)
    {
        int randomObstacle = random.Next(0, 3);
        obstacleSum += randomObstacle;
        ObstacleType obstacleType = (ObstacleType)randomObstacle;

        speed += 10f * RunnerSimulation.FixedDeltatime;
        speed = (float)Math.Round(speed, 3);
        score += (int)(100f * RunnerSimulation.FixedDeltatime) + ObstacleScore(inputType, obstacleType);
    }

    private int ObstacleScore(InputType inputType, ObstacleType obstacleType)
    {
        if (obstacleType == ObstacleType.None)
            return 0;
        if (obstacleType == ObstacleType.Top && inputType == InputType.Duck)
            return 0;
        if (obstacleType == ObstacleType.Bottom && inputType == InputType.Jump)
            return 0;
        return -1;
    }

    public virtual void Dispose()
    {
        Debug.Log($"Score : {score}\nSpeed : {speed}\nObstacles : {obstacleSum}");
    }
}
