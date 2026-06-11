using UnityEngine;

[CreateAssetMenu(menuName = "Tiny Car/Surface Profile", fileName = "TinyCarSurfaceProfile")]
public class TinyCarSurfaceProfile : ScriptableObject
{
    public TinyCarSurfaceParameters parameters = new TinyCarSurfaceParameters();

    public TinyCarSurfaceParameters GetParameters()
    {
        TinyCarSurfaceParameters clone = parameters == null
            ? new TinyCarSurfaceParameters()
            : parameters.clone();

        if (string.IsNullOrEmpty(clone.name))
        {
            clone.name = name;
        }

        return clone;
    }
}
