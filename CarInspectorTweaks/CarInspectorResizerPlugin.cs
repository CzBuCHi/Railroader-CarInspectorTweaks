namespace CarInspectorTweaks;

using HarmonyLib;
using JetBrains.Annotations;
using Railloader;
using Serilog;

[UsedImplicitly]
public sealed class CarInspectorTweaksPlugin : SingletonPluginBase<CarInspectorTweaksPlugin> {

    private const string ModIdentifier = "CarInspectorTweaks";

    public static IModdingContext Context { get; private set; } = null!;
    public static IUIHelper UiHelper { get; private set; } = null!;
    public static Settings Settings { get; private set; } = null!;

    private readonly ILogger _Logger = Log.ForContext<CarInspectorTweaksPlugin>()!;

    public CarInspectorTweaksPlugin(IModdingContext context, IUIHelper uiHelper) {
        Context = context;
        UiHelper = uiHelper;
        Settings = Context.LoadSettingsData<Settings>(ModIdentifier) ?? new Settings();
    }

    public override void OnEnable() {
        _Logger.Information("OnEnable");
        var harmony = new Harmony(ModIdentifier);
        harmony.PatchAll();
    }

    public override void OnDisable() {
        _Logger.Information("OnDisable");
        var harmony = new Harmony(ModIdentifier);
        harmony.UnpatchAll();
    }

    public static void SaveSettings() {
        Context.SaveSettingsData(ModIdentifier, Settings);
    }

}