using UnityEditor;
using UnityEngine;
using static RunnerSimulation;

public class ServerSimulator
{
    private const float secs = 5f;

    [MenuItem("Simulator/Record60 And ServerVerify")]
    public static void RecordAndServerVerify60()
    {
        Debug.Log("----------Record60AndServerVerify----------");
        Run(60);
    }

    private static void Run(int fps)
    {
        int frameCount = (int)(fps * secs);
        float deltaTime = 1f / fps;

        RunnerSimulation runnerSimulation = new(SimMode.Record);
        runnerSimulation.Start();

        for (int i = 0; i < frameCount; i++)
        {
            InputType inputType = (InputType)Random.Range(0, 3);
            runnerSimulation.Update(deltaTime, inputType);
        }
        runnerSimulation.Dispose();

        SeverVerify(runnerSimulation.Score, runnerSimulation.Replay);
    }

    private static void SeverVerify(int score, Replay replay)
    {
        RunnerSimulation runnerSimulation = new(SimMode.Replay, replay);
        runnerSimulation.Start();

        for (int i = 0; i < replay.InputResults.Count; i++)
        {
            runnerSimulation.Update(RunnerSimulation.FixedDeltatime);
        }
        runnerSimulation.Dispose();

        Debug.Log($"Submit Score : {score}\nVerify Score : {runnerSimulation.Score}");
    }
}
