using System.IO;
using UnityEngine;

public static class ReplayFileIO
{
    private static readonly string Path = $"{Application.temporaryCachePath}/Replay_Record.txt";

    public static void Save(Replay replay)
    {
        string json = JsonUtility.ToJson(replay, true);
        File.WriteAllText(Path, json);
        //Debug.Log($"Saved : {Path}");
    }

    public static Replay Load()
    {
        string json = File.ReadAllText(Path);
        //Debug.Log($"Loaded : {Path}");
        return JsonUtility.FromJson<Replay>(json);
    }
}
