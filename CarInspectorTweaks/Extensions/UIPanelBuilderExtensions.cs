using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Helpers;
using UI.Builder;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CarInspectorTweaks.Extensions;

// ReSharper disable once InconsistentNaming
public static class UIPanelBuilderExtensions
{
    public static int GetFieldCount(this UIPanelBuilder builder) {
        var container = Traverse.Create(builder)!.Field("_container")!.GetValue<RectTransform>()!;
        return container.childCount;
    }

    public static void ReplaceField(this UIPanelBuilder builder, int position, RectTransform newControl) {
        var container = Traverse.Create(builder)!.Field("_container")!.GetValue<RectTransform>()!;
        var wrapper   = container.GetChild(position)!;
        var value     = wrapper.Find("Value")!.GetComponent<RectTransform>()!;
        value.DestroyAllChildren();
        newControl.SetParent(value, false);
        newControl.SetFrameFillParent();
        newControl.SetTextMarginsTop(4f);
    }

    public static void ReplaceHStack(this UIPanelBuilder builder, int position, Action<UIPanelBuilder> closure, float spacing = 4f) {
        var container     = Traverse.Create(builder)!.Field("_container")!.GetValue<RectTransform>()!;
        var panel         = Traverse.Create(builder)!.Field("_panel")!.GetValue<UIPanel>()!;
        var panelChildren = Traverse.Create(panel)!.Field("_children")!.GetValue<HashSet<UIPanel>>()!;

        var rectView = container.GetChild(position)!;
        if (rectView.name != "HStack") {
            throw new InvalidOperationException("Child is not a HStack!");
        }

        var rectTransform = rectView.GetComponent<RectTransform>();
        var target = panelChildren
            .FirstOrDefault(child =>
                Traverse.Create(child)!.Field("_container")!.GetValue<RectTransform>()! == rectTransform
            );

        if (target == null) {
            throw new InvalidOperationException("Cannot find target panel");
        }

        var horizontalLayoutGroup = rectView.gameObject!.GetComponent<HorizontalLayoutGroup>()!;
        horizontalLayoutGroup.spacing = spacing;

        Traverse.Create(target)!.Field("_buildClosure")!.SetValue(closure);

        typeof(UIPanel).GetMethod("Rebuild", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(target, null!);
    }
}
