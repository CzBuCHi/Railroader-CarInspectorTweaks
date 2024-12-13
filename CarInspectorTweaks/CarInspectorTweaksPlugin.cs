using HarmonyLib;
using JetBrains.Annotations;
using Railloader;
using Serilog;
using UI.Builder;

namespace CarInspectorTweaks;

[UsedImplicitly]
public sealed class CarInspectorTweaksPlugin : SingletonPluginBase<CarInspectorTweaksPlugin>, IModTabHandler
{
    private const string ModIdentifier = "CarInspectorTweaks";

    public static IModdingContext Context  { get; private set; } = null!;
    public static IUIHelper       UiHelper { get; private set; } = null!;
    public static Settings        Settings { get; private set; } = null!;

    private readonly ILogger _Logger = Log.ForContext<CarInspectorTweaksPlugin>()!;

    public CarInspectorTweaksPlugin(IModdingContext context, IUIHelper uiHelper) {
        Context = context;
        UiHelper = uiHelper;
        Settings = Context.LoadSettingsData<Settings>(ModIdentifier) ?? new Settings();
    }

    public override void OnEnable() {
        _Logger.Information("OnEnable");
        ApplyPatches();
    }

    public override void OnDisable() {
        _Logger.Information("OnDisable");
        UnpatchAll();
    }

    public static void SaveSettings() {
        Context.SaveSettingsData(ModIdentifier, Settings);
    }

    public void ModTabDidOpen(UIPanelBuilder builder) {
        builder.AddField("Whistle Buttons", builder.AddToggle(() => Settings.WhistleButtons, o => Settings.WhistleButtons = o)!)!
               .Tooltip("Whistle Buttons", "Adds 'Prev' and 'Next' buttons under whistle drop down to simplify whistle selection.");

        builder.AddField("Faster strong man", builder.AddToggle(() => Settings.FastStrongMan, o => Settings.FastStrongMan = o)!)!
               .Tooltip("Faster strong man", "Engineer is able to push car up to 6mph.");

        builder.AddField("Remember tab", builder.AddToggle(() => Settings.RememberTab, o => Settings.RememberTab = o)!)!
               .Tooltip("Remember tab", "Game will remember selected tab when selecting different car of same type.");

        builder.AddField("Copy repair destination", builder.AddToggle(() => Settings.CopyRepairDestination, o => Settings.CopyRepairDestination = o)!)!
               .Tooltip("Copy repair destination", "Adds 'Copy repair destination' button to equipment panel when car has repair destination selected.");

        builder.AddField("Show car speed", builder.AddToggle(() => Settings.ShowCarSpeed, o => Settings.ShowCarSpeed = o)!)!
               .Tooltip("Show car speed", "Shows car speed on car tab.");

        builder.AddField("Show car oil", builder.AddToggle(() => Settings.ShowCarOil, o => Settings.ShowCarOil = o)!)!
               .Tooltip("Show car oil", "Shows car oil state on car tab.");

        builder.AddField("Bleed all", builder.AddToggle(() => Settings.BleedAll, o => Settings.BleedAll = o)!)!
               .Tooltip("Bleed all", "Adds 'Bleed all' button to car panel.");

        builder.AddField("Toggle switch", builder.AddToggle(() => Settings.ToggleSwitch, o => Settings.ToggleSwitch = o)!)!
               .Tooltip("Toggle switch", "Adds 'toggle switch' button to manual orders and yard tab.");

        builder.AddField("Manual controls", builder.AddToggle(() => Settings.ManualControls, o => Settings.ManualControls = o)!)!
               .Tooltip("Manual controls", "Adds controls to manual orders tab.");

        builder.AddField("Update customize window", builder.AddToggle(() => Settings.UpdateCarCustomizeWindow, o => Settings.UpdateCarCustomizeWindow = o)!)!
               .Tooltip("Update customize window", "Updates car customize window when different car is selected.");

        builder.AddField("Manage Consist", builder.AddToggle(() => Settings.ConsistManage, o => {
                   Settings.ConsistManage = o;
                   builder.Rebuild();
               })!)!
               .Tooltip("Manage Consist", "Adds 'connect air', 'release handbrakes' and 'oil all cars' buttons to manual orders and yard tab.");

        if (Settings.ConsistManage) {
            builder.AddField("Oil Threshold", builder.AddSliderQuantized(() => Settings.OilThreshold,
                       () => (Settings.OilThreshold * 100).ToString("0") + "%",
                       o => Settings.OilThreshold = o, 0.01f, 0, 1,
                       o => Settings.OilThreshold = o)!)!
                   .Tooltip("Oil Threshold", "Show 'oil all cars' button if any car has less oil than specified.");
        }

        builder.AddField("Waypoint controls", builder.AddToggle(() => Settings.WaypointControls, o => Settings.WaypointControls = o)!)!
               .Tooltip("Waypoint controls", "Adds ability to set waypoints.");

        builder.AddButton("Save", ModTabDidClose);
    }

    public void ModTabDidClose() {
        SaveSettings();
        UnpatchAll();
        ApplyPatches();
    }

    private void UnpatchAll() {
        var harmony = new Harmony(ModIdentifier);
        harmony.UnpatchAll(ModIdentifier);
    }

    private void ApplyPatches() {
        var harmony = new Harmony(ModIdentifier);

        if (Settings.WhistleButtons) {
            harmony.PatchCategory("WhistleButtons");
        }

        if (Settings.FastStrongMan) {
            harmony.PatchCategory("FastStrongMan");
        }

        if (Settings.RememberTab) {
            harmony.PatchCategory("RememberTab");
        }

        if (Settings.CopyRepairDestination) {
            harmony.PatchCategory("CopyRepairDestination");
        }

        if (Settings.ShowCarSpeed) {
            harmony.PatchCategory("ShowCarSpeed");
        }

        if (Settings.ShowCarOil) {
            harmony.PatchCategory("ShowCarOil");
        }

        if (Settings.BleedAll) {
            harmony.PatchCategory("BleedAll");
        }

        if (Settings.ManualControls) {
            harmony.PatchCategory("ManualControls");
        }

        if (Settings.ToggleSwitch) {
            harmony.PatchCategory("ToggleSwitch");
        }

        if (Settings.UpdateCarCustomizeWindow) {
            harmony.PatchCategory("UpdateCarCustomizeWindow");
        }

        if (Settings.ConsistManage) {
            harmony.PatchCategory("ConsistManage");
        }

        if (Settings.WaypointControls) {
            harmony.PatchCategory("WaypointControls");
        }
    }
}
