using UnityEditor;
using UnityEngine;
using static RunnerSimulation;

public class RunnerSimulationMenu
{
    private const float secs = 5f;

    [MenuItem("Simulator/Record")]
    public static void RunRecord()
    {
        Debug.Log("----------Record----------");
        Run(60, SimMode.Record);
    }

    [MenuItem("Simulator/Replay30")]
    public static void RunReplay30()
    {
        Debug.Log("----------Replay30----------");
        Run(30, SimMode.Replay);
    }

    [MenuItem("Simulator/Replay60")]
    public static void RunReplay60()
    {
        Debug.Log("----------Replay60----------");
        Run(60, SimMode.Replay);
    }

    [MenuItem("Simulator/Replay120")]
    public static void RunReplay120()
    {
        Debug.Log("----------Replay120----------");
        Run(120, SimMode.Replay);
    }

    private static void Run(int fps, SimMode simMode)
    {
        int frameCount = (int)(fps * secs);
        float deltaTime = 1f / fps;

        RunnerSimulation runnerSimulation = new(simMode);
        runnerSimulation.Start();

        for (int i = 0; i < frameCount; i++)
        {
            if (simMode == SimMode.Record)
            {
                InputType inputType = (InputType)Random.Range(0, 3);
                runnerSimulation.Update(deltaTime, inputType);
            }
            else if (simMode == SimMode.Replay)
            {
                runnerSimulation.Update(deltaTime);
            }
        }
        runnerSimulation.Dispose();
    }
}
