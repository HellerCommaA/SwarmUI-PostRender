using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace HellerCommaA.Extensions;

public class PostRenderFilmGrain : Extension
{
    public static double StepInjectPriority = Utils.STEP_ORDER_FILM_GRAIN;
    private const string NodeNameFilmGrain = "ProPostFilmGrain";
    private const string FeatureFlagGrain = "pro_post_grain";
    private const string PREFIX = "[Film]";
    public static T2IRegisteredParam<bool> GrayScale;
    public static T2IRegisteredParam<string> GrainType;
    public static T2IRegisteredParam<float> GrainSat;
    public static T2IRegisteredParam<float> GrainPower;
    public static T2IRegisteredParam<float> Shadows;
    public static T2IRegisteredParam<float> Highs;
    public static T2IRegisteredParam<float> Scale;
    public static T2IRegisteredParam<int> Sharpen;
    public static T2IRegisteredParam<float> SrcGamma;
    public static T2IRegisteredParam<long> NoiseSeed;

    public override void OnInit()
    {
        // Define required nodes
        ComfyUIBackendExtension.NodeToFeatureMap[NodeNameFilmGrain] = FeatureFlagGrain;

        // Setup parameters
        T2IParamGroup PostRenderGroup = new("Post Render Film Grain", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9);
        int orderCounter = 0;
        GrayScale = T2IParamTypes.Register<bool>(new($"{PREFIX} GrayScale",
            "Enables grayscale mode. If true, the output will be in grayscale.",
            "false",
            ViewType: ParamViewType.NORMAL,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        GrainType = T2IParamTypes.Register<string>(new($"{PREFIX} Grain Type",
            "Sets the grain type",
            "Fine",
            GetValues: _ => ["Fine", "Fine Simple", "Coarse", "Coarser"],
            ViewType: ParamViewType.NORMAL,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        GrainSat = T2IParamTypes.Register<float>(new($"{PREFIX} Grain Saturation",
            "Grain color saturation",
            "0.5",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        GrainPower = T2IParamTypes.Register<float>(new($"{PREFIX} Grain Power",
            "Overall intensity of the grain effect",
            "0.7",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        Shadows = T2IParamTypes.Register<float>(new($"{PREFIX} Shadows",
            "Intensity of grain in the shadows",
            "0.2",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        Highs = T2IParamTypes.Register<float>(new($"{PREFIX} Highlights",
            "Intensity of the grain in the highlights",
            "0.2",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        Scale = T2IParamTypes.Register<float>(new($"{PREFIX} Scale",
            "Image scaling ratio. Scales the image before applying grain and scales back afterwards",
            "1.0",
            Min: 0.0, Max: 10.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        Sharpen = T2IParamTypes.Register<int>(new($"{PREFIX} Sharpen",
            "Number of sharpening passes",
            "0",
            Min: 0, Max: 10,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        SrcGamma = T2IParamTypes.Register<float>(new($"{PREFIX} Source Gamma",
            "Gamma compensation applied to the input",
            "1.0",
            Min: 0.0, Max: 10.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));
        NoiseSeed = T2IParamTypes.Register<long>(new($"{PREFIX} Seed",
            "Seed for the grain random generator",
            "-1",
            Min: -1, Max: 1000, Step: 1,
            ViewType: ParamViewType.SEED,
            Group: PostRenderGroup,
            FeatureFlag: FeatureFlagGrain,
            OrderPriority: orderCounter++,
            Nonreusable: true
        ));

        WorkflowGenerator.AddStep(g =>
        {
            if (!ComfyUIBackendExtension.FeaturesSupported.Contains(FeatureFlagGrain))
            {
                Logs.Warning("PostRenderFilmGrain: Feature not supported");
                return;
            }
            bool grayScale = false;
            string grainType = "";
            float grainSat = 0f;
            float grainPower = 0f;
            float shadows = 0f;
            float highs = 0f;
            float scale = 0f;
            int sharpen = 0;
            float srcGamma = 0f;
            long noiseSeed = 0;
            if (g.UserInput.TryGet(GrayScale, out grayScale) && g.UserInput.TryGet(GrainType, out grainType) &&
                g.UserInput.TryGet(GrainSat, out grainSat) && g.UserInput.TryGet(GrainPower, out grainPower) &&
                g.UserInput.TryGet(Shadows, out shadows) && g.UserInput.TryGet(Highs, out highs) &&
                g.UserInput.TryGet(Scale, out scale) && g.UserInput.TryGet(Sharpen, out sharpen) &&
                g.UserInput.TryGet(SrcGamma, out srcGamma) && g.UserInput.TryGet(NoiseSeed, out noiseSeed))
            {
                string filmNode = g.CreateNode(NodeNameFilmGrain, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["gray_scale"] = grayScale,
                    ["grain_type"] = grainType,
                    ["grain_sat"] = grainSat,
                    ["grain_power"] = grainPower,
                    ["shadows"] = shadows,
                    ["highs"] = highs,
                    ["scale"] = scale,
                    ["sharpen"] = sharpen,
                    ["src_gamma"] = srcGamma,
                    ["seed"] = noiseSeed,
                });
                g.FinalImageOut = [filmNode, 0];
            }
        }, StepInjectPriority);
    }
}