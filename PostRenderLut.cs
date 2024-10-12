using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace HellerCommaA.Extensions;

public class PostRenderLut : Extension
{
    public static double StepInjectPriority = Utils.STEP_ORDER_LUT;
    private const string NodeNameLut = "ProPostApplyLUT";
    private const string FeatureFlagLut = "pro_post_apply_lut";
    private const string PREFIX = "[LUT]";
    public static T2IRegisteredParam<float> Strength;
    public static T2IRegisteredParam<bool> LogSpace;
    public static T2IRegisteredParam<string> LutName;

    private static ModelHelper lutHelper = new("luts")
    {
        Default = "None",
        Filter = model => string.Equals(Path.GetExtension(model), ".cube", StringComparison.OrdinalIgnoreCase)
    };

    public override void OnInit()
    {
        // Define required nodes
        ComfyUIBackendExtension.NodeToFeatureMap[NodeNameLut] = FeatureFlagLut;

        var modelRoot = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, Program.ServerSettings.Paths.ModelRoot);

        // Setup parameters
        T2IParamGroup PostRenderGroup = new("Post Render Apply LUT", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9);
        int orderCounter = 0;
        LutName = T2IParamTypes.Register<string>(new($"{PREFIX} Name",
            "LUT to apply to the image.\n" +
            $"To add new LUTs place them in <b>both</b> <i>'{modelRoot}/{lutHelper.Subfolder}' " +
            $"<b>AND</b> 'ComfyUI/models/{lutHelper.Subfolder}'</i>\n" +
            "(sorry this takes up more space, but maybe there will be a better way to do it eventually)",
            lutHelper.GetDefault(),
            IgnoreIf: "None",
            GetValues: _ => lutHelper.GetValues(),
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagLut,
            OrderPriority: orderCounter++
        ));
        Strength = T2IParamTypes.Register<float>(new($"{PREFIX} Strength",
            "The strength of the LUT effect",
            "1.0",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagLut,
            OrderPriority: orderCounter++
        ));
        LogSpace = T2IParamTypes.Register<bool>(new($"{PREFIX} LOG space",
            "If true, the image is processed in LOG color space",
            "false",
            ViewType: ParamViewType.NORMAL,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagLut,
            OrderPriority: orderCounter++
        ));

        WorkflowGenerator.AddStep(g =>
        {
            if (!ComfyUIBackendExtension.FeaturesSupported.Contains(FeatureFlagLut))
            {
                Logs.Warning("PostRenderApplyLUT: Feature not supported");
                return;
            }
            string lutName = "";
            float strength = 0f;
            bool isLog = false;

            if (g.UserInput.TryGet(Strength, out strength) && g.UserInput.TryGet(LogSpace, out isLog) &&
                g.UserInput.TryGet(LutName, out lutName))
            {
                string lutNode = g.CreateNode(NodeNameLut, new JObject
                {
                    ["lut_name"] = lutName,
                    ["image"] = g.FinalImageOut,
                    ["log"] = isLog,
                    ["strength"] = strength,
                });
                g.FinalImageOut = [lutNode, 0];
            }
        }, StepInjectPriority);
    }
}