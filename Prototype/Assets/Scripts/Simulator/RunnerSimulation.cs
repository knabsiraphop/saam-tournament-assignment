using System;
using System.Diagnostics;

public class RunnerSimulation : IDisposable
{
    public enum SimMode
    {
        None,
        Record,
        Replay,
    }

    public const float FixedDeltatime = 1f / 60f;
    public const int FixedSeed = 1;

    private readonly BaseUpdater updater;
    private readonly Replay replay;

    private float accumulator = 0f;
    private int tickIndex = 0;
    private Stopwatch stopwatch;
    private SimMode simMode;

    public int Score => updater.Score;
    public Replay Replay => replay;

    public void Start()
    {
        stopwatch = Stopwatch.StartNew();
    }

    public RunnerSimulation(SimMode simMode, Replay replay = default)
    {
        this.simMode = simMode;
        switch (simMode)
        {
            case SimMode.None:
                return;
            case SimMode.Record:
                this.replay = new Replay(FixedSeed);
                updater = new RecordUpdater(this.replay.AddInput);
                break;
            case SimMode.Replay:
                this.replay = !replay.Equals(default(Replay)) ? replay : ReplayFileIO.Load();
                updater = new ReplayUpdater();
                break;
        }
    }

    public void Update(float inputDeltaTime, InputType inputType)
    {
        accumulator += inputDeltaTime;
        while (accumulator >= FixedDeltatime)
        {
            updater.Update(inputType);
            accumulator -= FixedDeltatime;
        }
    }

    public void Update(float inputDeltaTime)
    {
        accumulator += inputDeltaTime;
        while (accumulator >= FixedDeltatime)
        {
            InputType inputType = replay.GetInput(tickIndex);
            tickIndex++;
            updater.Update(inputType);
            accumulator -= FixedDeltatime;
        }
    }

    public void Dispose()
    {
        stopwatch.Stop();
        UnityEngine.Debug.Log($"ReplayTime : {stopwatch.Elapsed.TotalMilliseconds:F4} ms");
        stopwatch = null;

        if (simMode == SimMode.Record)
            ReplayFileIO.Save(replay);

        tickIndex = 0;
        updater.Dispose();
    }
}
