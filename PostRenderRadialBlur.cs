using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace HellerCommaA.Extensions;

public class PostRenderRadialBlur : Extension
{
    public static double StepInjectPriority = Utils.STEP_ORDER_RADIAL_BLUR;
    private const string NodeNameBlur = "ProPostRadialBlur";
    private const string FeatureFlagBlur = "pro_post_blur";
    private const string PREFIX = "[Blur]";
    public static T2IRegisteredParam<float> Strength;
    public static T2IRegisteredParam<float> PosX;
    public static T2IRegisteredParam<float> PosY;
    public static T2IRegisteredParam<float> FocusSpread;
    public static T2IRegisteredParam<int> Steps;

    public override void OnInit()
    {
        // Define required nodes
        ComfyUIBackendExtension.NodeToFeatureMap[NodeNameBlur] = FeatureFlagBlur;

        // Setup parameters
        T2IParamGroup PostRenderGroup = new("Post Render Blur", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9);
        int orderCounter = 0;
        Strength = T2IParamTypes.Register<float>(new($"{PREFIX} Strength",
            "Blur Strength, lower is weaker",
            "64",
            Min: 0, Max: 256, Step: 1,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagBlur,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        PosX = T2IParamTypes.Register<float>(new($"{PREFIX} X Position",
            "Blur X position, 0 is left, 0.5 is center, 1 is right",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagBlur,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        PosY = T2IParamTypes.Register<float>(new($"{PREFIX} Y Position",
            "Blur Y position, 0 is top, 0.5 is center, 1 is bottom",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagBlur,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        FocusSpread = T2IParamTypes.Register<float>(new($"{PREFIX} Focus Spread",
            "Spread of the area of focus, higher is sharper",
            "1",
            Min: 0.1, Max: 8.0, Step: 0.1,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagBlur,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        Steps = T2IParamTypes.Register<int>(new($"{PREFIX} Steps",
            "Number of steps to use when bluring image, higher is slower",
            "5",
            Min: 1, Max: 32, Step: 1,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagBlur,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        WorkflowGenerator.AddStep(g =>
        {
            if (!ComfyUIBackendExtension.FeaturesSupported.Contains(FeatureFlagBlur))
            {
                Logs.Warning("PostRenderBlur: Feature not supported");
                return;
            }
            float yPos = 0.0f;
            float xPos = 0.0f;
            float strength = 0.0f;
            float focusSpread = 0.0f;
            int steps = 0;
            if (g.UserInput.TryGet(Strength, out strength) && g.UserInput.TryGet(PosX, out xPos) && g.UserInput.TryGet(PosY, out yPos) && g.UserInput.TryGet(FocusSpread, out focusSpread) && g.UserInput.TryGet(Steps, out steps))
            {
                // got all of our user vars
                string blurNode = g.CreateNode(NodeNameBlur, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["blur_strength"] = strength,
                    ["center_x"] = xPos,
                    ["center_y"] = yPos,
                    ["focus_spread"] = focusSpread,
                    ["steps"] = steps,
                });
                g.FinalImageOut = [blurNode, 0];
            }
        }, StepInjectPriority);
    }
}