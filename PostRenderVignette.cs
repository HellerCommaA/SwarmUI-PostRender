using System.IO;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace HellerCommaA.Extensions;

public class PostRenderVignette : Extension
{
    public static double StepInjectPriority = Utils.STEP_ORDER_VIG;
    private const string NodeNameVignette = "ProPostVignette";
    private const string FeatureFlagVignette = "pro_post_vignette";
    private const string PREFIX = "[VIG]";
    public static T2IRegisteredParam<float> Vignette;
    public static T2IRegisteredParam<float> PosX;
    public static T2IRegisteredParam<float> PosY;

    public override void OnInit()
    {
        // Define required nodes
        ComfyUIBackendExtension.NodeToFeatureMap[NodeNameVignette] = FeatureFlagVignette;

        // Setup parameters
        T2IParamGroup PostRenderGroup = new("Post Render Vignette", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9);
        int orderCounter = 0;
        // var modelRoot = Utilities.CombinePathWithAbsolute(Environment.CurrentDirectory, Program.ServerSettings.Paths.ModelRoot);
        Vignette = T2IParamTypes.Register<float>(new($"{PREFIX} Strength",
            "Vignette strength, lower is weaker",
            "0.2",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagVignette,
            OrderPriority: orderCounter++
        ));
        PosX = T2IParamTypes.Register<float>(new($"{PREFIX} X Position",
            "Vignette X position, 0 is left, 0.5 is center, 1 is right",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagVignette,
            OrderPriority: orderCounter++
        ));
        PosY = T2IParamTypes.Register<float>(new($"{PREFIX} Y Position",
            "Vignette Y position, 0 is top, 0.5 is center, 1 is bottom",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagVignette,
            OrderPriority: orderCounter++
        ));

        WorkflowGenerator.AddStep(g =>
        {
            if (!ComfyUIBackendExtension.FeaturesSupported.Contains(FeatureFlagVignette))
            {
                Logs.Warning("PostRenderVignette: Feature not supported");
                return;
            }
            float yPos = 0.5f;
            float xPos = 0.5f;
            float intensity = 0.2f;
            if (g.UserInput.TryGet(Vignette, out intensity) && g.UserInput.TryGet(PosX, out xPos) && g.UserInput.TryGet(PosY, out yPos))
            {
                // got all of our user vars
                string vigNode = g.CreateNode(NodeNameVignette, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["intensity"] = intensity,
                    ["center_x"] = xPos,
                    ["center_y"] = yPos,
                });
                g.FinalImageOut = [vigNode, 0];
            }
        }, StepInjectPriority);
    }
}