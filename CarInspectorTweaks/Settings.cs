using System.Collections.Generic;
using Model.Definition;

namespace CarInspectorTweaks;

public class Settings
{
    public readonly Dictionary<CarArchetype, string?> TabStates = new();

    public bool  WhistleButtons           { get; set; }
    public bool  FastStrongMan            { get; set; }
    public bool  RememberTab              { get; set; }
    public bool  CopyRepairDestination    { get; set; }
    public bool  ShowCarSpeed             { get; set; }
    public bool  BleedAll                 { get; set; }
    public bool  ToggleSwitch             { get; set; }
    public bool  ManualControls           { get; set; }
    public bool  UpdateCarCustomizeWindow { get; set; }
    public bool  ConsistManage            { get; set; }
    public float OilThreshold             { get; set; }
    public bool  ShowCarOil               { get; set; }
    public bool  WaypointControls         { get; set; }
    public int   YardMaxSpeed             { get; set; } = 15;
    public bool  SetCarInspectorHeight    { get; set; }
    public int   CarInspectorHeight       { get; set; } = 500;
}
