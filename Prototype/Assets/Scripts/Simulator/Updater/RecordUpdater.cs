using System;
using UnityEngine;

public class RecordUpdater : BaseUpdater
{
    private Action<InputType> updateReplay;

    public RecordUpdater(Action<InputType> updateReplay) : base()
    {
        this.updateReplay = updateReplay;
    }

    public override void Update(InputType inputType)
    {
        updateReplay?.Invoke(inputType);
        base.Update(inputType);
    }
}
