using System;
using System.Collections.Generic;

[Serializable]
public struct Replay
{
    public int Seed;
    public List<InputType> InputResults;

    public Replay(int seed)
    {
        Seed = seed;
        InputResults = new List<InputType>();
    }

    public void AddInput(InputType inputResult)
    {
        InputResults.Add(inputResult);
    }

    public InputType GetInput(int index) 
    {
        if (InputResults.Count <= index)
            return InputType.None;
        return InputResults[index];
    }
}
