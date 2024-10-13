
using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;
using System.IO;

namespace HellerCommaA.Extensions;

public class PostRenderExtension : Extension
{
    public double StepPriority = 9.0f;
    public const string FeatureFlagPostRender = "feature_flag_post_render";

    #region FilmGrain
    public const string FILM_GRAIN_PREFIX = "[Grain]";
    public const string NodeNameFilmGrain = "ProPostFilmGrain";
    public T2IRegisteredParam<bool> FGGrayScale;
    public T2IRegisteredParam<string> FGGrainType;
    public T2IRegisteredParam<float> FGGrainSat;
    public T2IRegisteredParam<float> FGGrainPower;
    public T2IRegisteredParam<float> FGShadows;
    public T2IRegisteredParam<float> FGHighs;
    public T2IRegisteredParam<float> FGScale;
    public T2IRegisteredParam<int> FGSharpen;
    public T2IRegisteredParam<float> FGSrcGamma;
    public T2IRegisteredParam<long> FGSeed;
    #endregion

    #region Vignette
    public const string VIGNETTE_PREFIX = "[Vig]";
    public const string NodeNameVignette = "ProPostVignette";
    public T2IRegisteredParam<float> VStrength;
    public T2IRegisteredParam<float> VPosX;
    public T2IRegisteredParam<float> VPosY;
    #endregion

    #region Lut
    public const string LUT_PREFIX = "[LUT]";
    public const string NodeNameLut = "ProPostApplyLUT";
    public List<string> LutModels = [];
    public T2IRegisteredParam<float> LutStrength;
    public T2IRegisteredParam<bool> LutLogSpace;
    public T2IRegisteredParam<string> LutName;
    #endregion

    #region RadialBlur
    public const string R_BLUR_PREFIX = "[R. Blur]";
    public const string NodeNameRadialBlur = "ProPostRadialBlur";
    public T2IRegisteredParam<float> RBStrength;
    public T2IRegisteredParam<float> RBPosX;
    public T2IRegisteredParam<float> RBPosY;
    public T2IRegisteredParam<float> RBFocusSpread;
    public T2IRegisteredParam<int> RBSteps;
    #endregion


    public override void OnPreLaunch()
    {
        base.OnPreLaunch();

    }
    public override void OnInit()
    {
        base.OnInit();

        ComfyUISelfStartBackend.FoldersToForwardInComfyPath.Add("luts");

        InstallableFeatures.RegisterInstallableFeature(new("ProPost", FeatureFlagPostRender, "https://github.com/digitaljohn/comfyui-propost", "digitaljohn", "This will install ProPost nodes developed by digitaljohn\nDo you wish to install?"));
        ScriptFiles.Add("assets/pro_post.js");

        string path = Utilities.CombinePathWithAbsolute(Program.ServerSettings.Paths.ActualModelRoot, "luts");
        T2IParamTypes.ConcatDropdownValsClean(ref LutModels,
            [.. Directory.EnumerateFiles(path, "*.cube", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(path, f))]
        );

        ComfyUIBackendExtension.RawObjectInfoParsers.Add(rawObjectInfo =>
        {
            if (rawObjectInfo.TryGetValue("ProPostApplyLUT", out JToken lutNode))
            {
                T2IParamTypes.ConcatDropdownValsClean(ref LutModels, lutNode["input"]["required"]["lut_name"][0].Select(m => $"{m}"));
            }
        });

        // reactor is 9.0, lets list as after
        double orderPriorityCtr = 9.1;

        ComfyUIBackendExtension.NodeToFeatureMap[NodeNameFilmGrain] = FeatureFlagPostRender;

        #region FilmGrain
        T2IParamGroup GrainGroup = new("Film Grain", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: orderPriorityCtr);
        orderPriorityCtr += 0.1f;
        int orderCounter = 0;
        FGGrayScale = T2IParamTypes.Register<bool>(new($"{FILM_GRAIN_PREFIX} Gray Scale",
            "Enables grayscale mode. If true, the output will be in grayscale",
            "false",
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGGrainType = T2IParamTypes.Register<string>(new($"{FILM_GRAIN_PREFIX} Grain Type",
            "Sets the grain type",
            "Fine",
            GetValues: _ => ["Fine", "Fine Simple", "Coarse", "Coarser"],
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGGrainSat = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Grain Saturation",
            "Grain color saturation",
            "0.5",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGGrainPower = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Grain Power",
            "Overall intensity of the grain effect",
            "0.7",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGShadows = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Shadows",
            "Intensity of grain in the shadows",
            "0.2",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGHighs = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Highlights",
            "Intensity of the grain in the highlights",
            "0.2",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGScale = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Scale",
            "Image scaling ratio. Scales the image before applying grain and scales back afterwards",
            "1.0",
            Min: 0.0, Max: 10.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGSharpen = T2IParamTypes.Register<int>(new($"{FILM_GRAIN_PREFIX} Sharpen",
            "Number of sharpening passes",
            "0",
            Min: 0, Max: 10,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGSrcGamma = T2IParamTypes.Register<float>(new($"{FILM_GRAIN_PREFIX} Source Gamma",
            "Gamma compensation applied to the input",
            "1.0",
            Min: 0.0, Max: 10.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        FGSeed = T2IParamTypes.Register<long>(new($"{FILM_GRAIN_PREFIX} Seed",
            "Seed for the grain random generator",
            "-1",
            Min: -1, Max: 1000, Step: 1,
            ViewType: ParamViewType.SEED,
            Group: GrainGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        WorkflowGenerator.AddStep(g =>
        {
            // only try one of these, if we have one we have them all
            if (g.UserInput.TryGet(FGGrayScale, out bool grayScale))
            {
                if (!g.Features.Contains(FeatureFlagPostRender))
                {
                    throw new SwarmUserErrorException("Post Render parameters specified, but feature isn't installed");
                }
                string filmNode = g.CreateNode(NodeNameFilmGrain, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["gray_scale"] = grayScale,
                    ["grain_type"] = g.UserInput.Get(FGGrainType),
                    ["grain_sat"] = g.UserInput.Get(FGGrainSat),
                    ["grain_power"] = g.UserInput.Get(FGGrainPower),
                    ["shadows"] = g.UserInput.Get(FGShadows),
                    ["highs"] = g.UserInput.Get(FGHighs),
                    ["scale"] = g.UserInput.Get(FGScale),
                    ["sharpen"] = g.UserInput.Get(FGSharpen),
                    ["src_gamma"] = g.UserInput.Get(FGSrcGamma),
                    ["seed"] = g.UserInput.Get(FGSeed),
                });
                g.FinalImageOut = [filmNode, 0];
            }
        }, StepPriority);
        StepPriority += 0.1f;
        #endregion

        orderCounter = 0;

        #region Vignette
        T2IParamGroup VigGroup = new("Vignette", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: orderPriorityCtr);
        orderPriorityCtr += 0.1f;
        VStrength = T2IParamTypes.Register<float>(new($"{VIGNETTE_PREFIX} Vignette Strength",
            "Vignette strength, lower is weaker",
            "0.2",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: VigGroup,
            OrderPriority: orderCounter++
        ));
        VPosX = T2IParamTypes.Register<float>(new($"{VIGNETTE_PREFIX} X Position",
            "Vignette X position, 0 is left, 0.5 is center, 1 is right",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: VigGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        VPosY = T2IParamTypes.Register<float>(new($"{VIGNETTE_PREFIX} Y Position",
            "Vignette Y position, 0 is top, 0.5 is center, 1 is bottom",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: VigGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        WorkflowGenerator.AddStep(g =>
        {
            if (g.UserInput.TryGet(VStrength, out float vStr))
            {
                if (!g.Features.Contains(FeatureFlagPostRender))
                {
                    throw new SwarmUserErrorException("Post Render parameters specified, but feature isn't installed");
                }
                string vigNode = g.CreateNode(NodeNameVignette, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["intensity"] = vStr,
                    ["center_x"] = g.UserInput.Get(VPosX),
                    ["center_y"] = g.UserInput.Get(VPosY),
                });
                g.FinalImageOut = [vigNode, 0];
            }
        }, StepPriority);
        StepPriority += 0.1f;
        #endregion

        orderCounter = 0;

        #region RBlur
        T2IParamGroup rBlurGroup = new("Radial Blur", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: orderPriorityCtr);
        orderPriorityCtr += 0.1f;
        RBStrength = T2IParamTypes.Register<float>(new($"{R_BLUR_PREFIX} Strength",
            "Blur Strength, lower is weaker",
            "64",
            Min: 0, Max: 256, Step: 1,
            ViewType: ParamViewType.SLIDER,
            Group: rBlurGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        RBPosX = T2IParamTypes.Register<float>(new($"{R_BLUR_PREFIX} X Position",
            "Blur X position, 0 is left, 0.5 is center, 1 is right",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: rBlurGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        RBPosY = T2IParamTypes.Register<float>(new($"{R_BLUR_PREFIX} Y Position",
            "Blur Y position, 0 is top, 0.5 is center, 1 is bottom",
            "0.5",
            Min: 0, Max: 1, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: rBlurGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        RBFocusSpread = T2IParamTypes.Register<float>(new($"{R_BLUR_PREFIX} Focus Spread",
            "Spread of the area of focus, higher is sharper",
            "1",
            Min: 0.1, Max: 8.0, Step: 0.1,
            ViewType: ParamViewType.SLIDER,
            Group: rBlurGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        RBSteps = T2IParamTypes.Register<int>(new($"{R_BLUR_PREFIX} Steps",
            "Number of steps to use when bluring image, higher is slower",
            "5",
            Min: 1, Max: 32, Step: 1,
            ViewType: ParamViewType.SLIDER,
            Group: rBlurGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));

        WorkflowGenerator.AddStep(g =>
        {
            if (g.UserInput.TryGet(RBStrength, out float bStr))
            {
                if (!g.Features.Contains(FeatureFlagPostRender))
                {
                    throw new SwarmUserErrorException("Post Render parameters specified, but feature isn't installed");
                }
                string blurNode = g.CreateNode(NodeNameRadialBlur, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["blur_strength"] = bStr,
                    ["center_x"] = g.UserInput.Get(RBPosX),
                    ["center_y"] = g.UserInput.Get(RBPosY),
                    ["focus_spread"] = g.UserInput.Get(RBFocusSpread),
                    ["steps"] = g.UserInput.Get(RBSteps),
                });
                g.FinalImageOut = [blurNode, 0];
            }
        }, StepPriority);
        StepPriority += 0.1f;
        #endregion

        orderCounter = 0;

        #region Lut
        T2IParamGroup lutGroup = new("Apply LUT", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: orderPriorityCtr);
        orderPriorityCtr += 0.1f;

        LutName = T2IParamTypes.Register<string>(new($"{LUT_PREFIX} Name",
            "LUT to apply to the image.\n" +
            $"To add new LUTs place them in SwarmUI/Models/luts",
            "None",
            IgnoreIf: "None",
            GetValues: _ => LutModels,
            Group: lutGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        LutStrength = T2IParamTypes.Register<float>(new($"{LUT_PREFIX} LUT Strength",
            "The strength of the LUT effect",
            "1.0",
            Min: 0.0, Max: 1.0, Step: 0.01,
            ViewType: ParamViewType.SLIDER,
            Group: lutGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));
        LutLogSpace = T2IParamTypes.Register<bool>(new($"{LUT_PREFIX} LOG Space",
            "If true, the image is processed in LOG color space",
            "false",
            ViewType: ParamViewType.NORMAL,
            Group: lutGroup,
            FeatureFlag: FeatureFlagPostRender,
            OrderPriority: orderCounter++
        ));

        WorkflowGenerator.AddStep(g =>
        {
            if (g.UserInput.TryGet(LutName, out string lName))
            {
                if (!g.Features.Contains(FeatureFlagPostRender))
                {
                    throw new SwarmUserErrorException("Post Render parameters specified, but feature isn't installed");
                }
                string lutNode = g.CreateNode(NodeNameLut, new JObject
                {
                    ["image"] = g.FinalImageOut,
                    ["lut_name"] = lName,
                    ["log"] = g.UserInput.Get(LutLogSpace),
                    ["strength"] = g.UserInput.Get(LutStrength),
                });
                g.FinalImageOut = [lutNode, 0];
            }
        }, StepPriority);
        StepPriority += 0.1f;
        #endregion

        orderCounter = 0;

    }

}
