using System.IO;
using SwarmUI.Core;
using SwarmUI.Utils;

namespace HellerCommaA.Extensions;

/**
 * this is a copy from Quaggles:
 * https://github.com/Quaggles/SwarmUI-FaceTools/blob/master/Utils.cs
**/

class Utils {
    public static double STEP_ORDER_RADIAL_BLUR = 9.7f;
    public static double STEP_ORDER_FILM_GRAIN = 9.8f;
    public static double STEP_ORDER_VIG = 9.9f;
    public static double STEP_ORDER_LUT = 9.99f;
}


public struct ModelHelper
{
    public readonly string Subfolder;
    public Func<string, bool> Filter = null;
    public string Default = null;
    public List<string> AlwaysInclude = new();
    public string NullValue = "None";
    public SearchOption SearchOption = SearchOption.AllDirectories;
    public readonly string AbsoluteModelPath;
    public SlashDirection PathSlashPolicy = SlashDirection.Any;

    public ModelHelper(string subfolder)
    {
        Subfolder = subfolder;
        AbsoluteModelPath = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, Program.ServerSettings.Paths.ModelRoot, subfolder);
    }

    string ApplyPathPolicy(string path)
    {
        return PathSlashPolicy switch
        {
            SlashDirection.Any => path,
            SlashDirection.Forward => path.Replace('\\', '/'),
            SlashDirection.Back => path.Replace('/', '\\'),
            _ => path
        };
    }

    public List<string> GetPaths()
    {
        var results = new List<string>();
        if (Directory.Exists(AbsoluteModelPath))
        {
            var models = Directory.EnumerateFiles(AbsoluteModelPath, "*", SearchOption);
            foreach (var model in models)
            {
                if (Filter != null && Filter.Invoke(model) == false) continue;
                results.Add(ApplyPathPolicy(model));
            }
        }

        results.Sort(StringComparer.OrdinalIgnoreCase);
        return results;
    }

    public List<string> GetValues()
    {
        var results = new List<string>(AlwaysInclude);
        foreach (var filePath in GetPaths())
        {
            var relativePath = Path.GetRelativePath(AbsoluteModelPath, filePath);
            if (!results.Contains(relativePath))
                results.Add(ApplyPathPolicy(relativePath));
        }
        results.Sort(StringComparer.OrdinalIgnoreCase);
        results.Insert(0, ApplyPathPolicy(NullValue));
        return results;
    }

    public string GetDefault(string overrideDefault = null)
    {
        var defaultValue = Default;
        // Allow overriding the default
        if (!string.IsNullOrEmpty(overrideDefault))
            defaultValue = overrideDefault;
        // Use the default if the model exists (Or it was in the always include list)
        if (!string.IsNullOrEmpty(defaultValue))
            if (AlwaysInclude.Contains(defaultValue) || File.Exists(Utilities.CombinePathWithAbsolute(AbsoluteModelPath, defaultValue)))
                return ApplyPathPolicy(defaultValue);

        // Otherwise pick the first model found
        foreach (var filePath in GetValues())
        {
            var relativePath = Path.GetRelativePath(AbsoluteModelPath, filePath);
            if (File.Exists(Utilities.CombinePathWithAbsolute(AbsoluteModelPath, filePath)))
                return ApplyPathPolicy(relativePath);
        }

        return ApplyPathPolicy(NullValue);
    }
}

public enum SlashDirection
{
    Any,
    Forward,
    Back
}
