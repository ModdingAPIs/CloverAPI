#pragma warning disable Harmony003

using CloverAPI.Content.Data;
using CloverAPI.Content.Settings;
using Panik;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;

namespace CloverAPI.Patches;

#nullable enable

// ===== Runtime state machine for the hub =====
internal static class HubState
{
    public enum View { None, HubIndex, Page }

    public static View Current = View.None;

    public static int HubPageOffset = 0;     // pagination over pages list
    public static int PageItemOffset = 0;    // pagination within a page
    public static int ActivePage = -1;       // index into ModSettingsManager.Pages

    public static void Reset()
    {
        ModSettingsManager.CancelKeybindListening();
        Current = View.None;
        HubPageOffset = 0;
        PageItemOffset = 0;
        ActivePage = -1;
    }
}

// ===== Harmony patches into MainMenuScript =====
[HarmonyPatch]
internal static class MainMenu_Patches
{
    // In MainMenuScript.MenuIndex.settings, the menu options are:
    // 0: Accessibility
    // 1: Video/Audio
    // 2: Others
    // 3: Delete Data
    // 4: Twitch (if supported) / Back (if not)
    // 5: Back (if Twitch is supported)
    private const int ModsSlotWithoutTwitch = 4;
    private const int ModsSlotWithTwitch = 5;

    private static bool _rowValidationLogged;

    private readonly struct HubLayout
    {
        public HubLayout(int capacity)
        {
            Capacity = capacity;
            BackRow = capacity - 1;
            NextRow = capacity >= 2 ? capacity - 2 : -1;
            ListSlots = Mathf.Max(0, capacity - 2);
            Step = Math.Max(1, ListSlots);
        }

        public int Capacity { get; }
        public int ListSlots { get; }
        public int NextRow { get; }
        public int BackRow { get; }
        public int Step { get; }
    }

    private static readonly FieldInfo DesiredNavigationIndexField =
        AccessTools.Field(typeof(MainMenuScript), "desiredNavigationIndex");

    private static readonly FieldInfo ControllerElementsField =
        AccessTools.Field(typeof(DiegeticMenuController), "elements");

    private static readonly FieldInfo RightNavigationPressField =
        AccessTools.Field(typeof(MainMenuScript), "rightNavigationPress");

    private static readonly FieldInfo LeftNavigationPressField =
        AccessTools.Field(typeof(MainMenuScript), "leftNavigationPress");

    private static readonly FieldInfo MenuIndexField =
        AccessTools.Field(typeof(MainMenuScript), "menuIndex");

    private static bool IsInHub() => HubState.Current != HubState.View.None;

    private static bool HasPages() => ModSettingsManager.Pages.Count > 0;

    private static int GetModsSlotIndex() => TwitchMaster.IsTwitchSupported() ? ModsSlotWithTwitch : ModsSlotWithoutTwitch;

    private static int GetBackSlotIndex() => GetModsSlotIndex() + 1;

    private static void SetDesiredNavigationIndex(MainMenuScript menu, int value)
    {
        DesiredNavigationIndexField?.SetValue(menu, value);
    }

    private static MainMenuScript.MenuIndex GetMenuIndex(MainMenuScript menu)
    {
        if (MenuIndexField == null)
            return MainMenuScript.MenuIndex.undefined;

        object raw = MenuIndexField.GetValue(menu);
        return raw is MainMenuScript.MenuIndex state ? state : MainMenuScript.MenuIndex.undefined;
    }

    private static void SetMenuIndex(MainMenuScript menu, MainMenuScript.MenuIndex value)
    {
        MenuIndexField?.SetValue(menu, value);
    }

    private static List<DiegeticMenuElement>? GetElementsList(DiegeticMenuController? controller)
    {
        if (controller == null || ControllerElementsField == null)
            return null;

        return ControllerElementsField.GetValue(controller) as List<DiegeticMenuElement>;
    }

    private static bool GetFlag(FieldInfo field, MainMenuScript menu)
    {
        return field != null && menu != null && field.GetValue(menu) is bool value && value;
    }

    private static void SetTextSafe(TextMeshProUGUI[] texts, int index, string? value)
    {
        if (texts != null && index >= 0 && index < texts.Length && texts[index] != null)
            texts[index].text = value ?? string.Empty;
    }

    private static DiegeticMenuElement? GetMenuElement(MainMenuScript? menu, int index)
    {
        if (menu?.menuElements == null || index < 0 || index >= menu.menuElements.Length)
            return null;

        return menu.menuElements[index];
    }

    private static bool TryGetOptionTexts(MainMenuScript menu, out TextMeshProUGUI[] optionTexts)
    {
        if (menu?.optionTexts == null || menu.optionTexts.Length == 0)
        {
            optionTexts = Array.Empty<TextMeshProUGUI>();
            return false;
        }

        optionTexts = menu.optionTexts;
        return true;
    }

    private static bool TryGetHubLayout(MainMenuScript menu, out HubLayout layout)
    {
        layout = default;
        if (menu == null)
            return false;

        int capacity = VisibleCapacity(menu);
        if (capacity <= 0)
            return false;

        layout = new HubLayout(capacity);
        return true;
    }

    private static bool TryGetActivePage(out ModSettingsManager.Page page)
    {
        int index = HubState.ActivePage;
        if (index >= 0 && index < ModSettingsManager.Pages.Count)
        {
            page = ModSettingsManager.Pages[index];
            return true;
        }

        page = null!;
        return false;
    }

    private static void RenderListEntries(TextMeshProUGUI[] optionTexts, int listSlots, int start, int total, Func<int, string> labelSelector)
    {
        int row = 0;
        for (int i = start; i < total && row < listSlots; i++, row++)
            SetTextSafe(optionTexts, row, labelSelector(i));

        for (; row < listSlots; row++)
            SetTextSafe(optionTexts, row, string.Empty);
    }

    private static void RenderPaginationFooter(TextMeshProUGUI[] optionTexts, HubLayout layout, int renderedEnd, int total)
    {
        if (layout.NextRow >= 0)
            SetTextSafe(optionTexts, layout.NextRow, renderedEnd < total ? "NEXT" : string.Empty);

        SetTextSafe(optionTexts, layout.BackRow, "BACK");
    }

    private static int ClampPageOffset(int requested, int total)
    {
        if (total <= 0)
            return 0;

        int maxOffset = Math.Max(0, total - 1);
        return Mathf.Clamp(requested, 0, maxOffset);
    }

    private static int GetDesiredNavigationIndex(MainMenuScript menu)
    {
        if (DesiredNavigationIndexField == null)
            return -1;

        object? raw = DesiredNavigationIndexField.GetValue(menu);
        return raw is int value ? value : -1;
    }

    private static void EnsureSlotState(MainMenuScript menu, int index, bool enabled)
    {
        if (menu?.menuController == null)
            return;

        var element = GetMenuElement(menu, index);
        if (element == null)
            return;

        var parentObject = element.transform?.parent?.gameObject;
        parentObject?.SetActive(enabled);

        var elementsList = GetElementsList(menu.menuController);
        if (elementsList == null)
            return;

        // MainMenuScript toggles visibility and keeps the active rows mirrored in DiegeticMenuController.elements.
        // We reproduce the same pattern so navigation and hover logic continue to work as expected.
        if (enabled)
        {
            if (!elementsList.Contains(element))
            {
                int insertIndex = Mathf.Clamp(index, 0, elementsList.Count);
                elementsList.Insert(insertIndex, element);
                element.SetMyController(menu.menuController);
            }

            if (!VirtualCursors.IsCursorVisible(0) && GetDesiredNavigationIndex(menu) == index)
                menu.menuController.HoveredElement = element;
        }
        else
        {
            elementsList.Remove(element);

            if (ReferenceEquals(menu.menuController.HoveredElement, element))
                menu.menuController.HoveredElement = null;
        }
    }

    private static void EnsureSlotEnabled(MainMenuScript menu, int index) => EnsureSlotState(menu, index, true);

    private static void EnsureSlotDisabled(MainMenuScript menu, int index) => EnsureSlotState(menu, index, false);

    // We rely on the vanilla menu exposing preallocated rows and simply toggle them on/off.
    // ValidateMenuRows ensures the prefab still provides the slots we target; otherwise we fail fast.
    private static bool ValidateMenuRows(MainMenuScript menu, int requiredIndex)
    {
        if (menu == null)
            return false;

        int elementsLength = menu.menuElements?.Length ?? 0;
        int optionsLength = menu.optionTexts?.Length ?? 0;

        bool hasElements = elementsLength > requiredIndex;
        bool hasOptions = optionsLength > requiredIndex;

        if ((!hasElements || !hasOptions) && !_rowValidationLogged)
        {
            _rowValidationLogged = true;
            LogFatal(
                $"ModSettingsExtender detected missing menu rows. Expected index {requiredIndex} but found menuElements={elementsLength}, optionTexts={optionsLength}. The base menu prefab likely changed; mods menu injection is disabled.");
        }

        return hasElements && hasOptions;
    }

    private static int VisibleCapacity(MainMenuScript menu)
    {
        var list = GetElementsList(menu?.menuController);
        return Mathf.Max(0, list?.Count ?? 0);
    }

    private static bool InputSuppressed(MainMenuScript menu)
    {
        bool pointer = Controls.MouseButton_PressedGet(0, Controls.MouseElement.RightButton);
        bool right = GetFlag(RightNavigationPressField, menu);
        bool left = GetFlag(LeftNavigationPressField, menu);
        return pointer | right | left;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuScript), "OptionsUpdateText_Desktop")]
    private static void OptionsUpdateText_Desktop_Postfix(MainMenuScript __instance)
    {
        AfterOptionsUpdateText(__instance);
    }

    private static void AfterOptionsUpdateText(MainMenuScript menu)
    {
        if (menu == null)
            return;

        try
        {
            if (GetMenuIndex(menu) == MainMenuScript.MenuIndex.settings)
            {
                EnsureModsRow(menu);
            }
            else
            {
                DisableExtraRow(menu);
            }

            if (IsInHub())
            {
                RenderHub(menu);
            }
            else if (HubState.Current != HubState.View.None && !HasPages())
            {
                HubState.Reset();
            }

            ModSettingsManager.TickKeybindListening();
        }
        catch (Exception ex)
        {
            LogError($"OptionsUpdate postfix failed: {ex}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuScript), "Update")]
    private static void Update_Postfix()
    {
        try
        {
            ModSettingsManager.TickKeybindListening();
        }
        catch (Exception ex)
        {
            LogError($"Keybind listening tick failed: {ex}");
        }
    }

    private static void DisableExtraRow(MainMenuScript menu)
    {
        if (menu == null)
            return;

        int backIndex = GetBackSlotIndex();
        if (!ValidateMenuRows(menu, backIndex))
            return;

        SetTextSafe(menu.optionTexts, backIndex, string.Empty);
        EnsureSlotDisabled(menu, backIndex);
    }

    // We hijack the vanilla back row (and an extra hidden row when Twitch is present) to surface the mods hub.
    // This keeps compatibility with the base menu controller without instantiating new UI prefabs.
    private static void EnsureModsRow(MainMenuScript menu)
    {
        var optionTexts = menu.optionTexts;
        if (optionTexts == null)
            return;

        int modsIndex = GetModsSlotIndex();
        int backIndex = GetBackSlotIndex();
        bool show = HasPages();

        if (!ValidateMenuRows(menu, Math.Max(modsIndex, backIndex)))
            return;

        if (!show)
        {
            // Restore vanilla layout when no registered pages are available.
            SetTextSafe(optionTexts, modsIndex, Translation.Get("MENU_OPTION_BACK"));
            SetTextSafe(optionTexts, backIndex, string.Empty);
            EnsureSlotDisabled(menu, backIndex);
            return;
        }

        if (modsIndex >= optionTexts.Length || backIndex >= optionTexts.Length)
            return;

        string backLabel = optionTexts[modsIndex]?.text ?? Translation.Get("MENU_OPTION_BACK");
        if (string.IsNullOrEmpty(backLabel))
            backLabel = Translation.Get("MENU_OPTION_BACK");

        EnsureSlotEnabled(menu, modsIndex);
        EnsureSlotEnabled(menu, backIndex);
        SetTextSafe(optionTexts, backIndex, backLabel);
        SetTextSafe(optionTexts, modsIndex, "MODS");
    }

    private static void RenderHub(MainMenuScript menu)
    {
        if (!TryGetOptionTexts(menu, out var optionTexts))
            return;

        if (!TryGetHubLayout(menu, out var layout))
            return;

        if (HubState.Current == HubState.View.HubIndex)
        {
            RenderHubIndex(menu, optionTexts, layout);
        }
        else if (HubState.Current == HubState.View.Page && TryGetActivePage(out var page))
        {
            RenderHubPage(menu, optionTexts, layout, page);
        }
    }

    private static void RenderHubIndex(MainMenuScript menu, TextMeshProUGUI[] optionTexts, HubLayout layout)
    {
        if (menu.titleText != null)
            menu.titleText.text = "MOD SETTINGS";

        int start = HubState.HubPageOffset;
        int total = ModSettingsManager.Pages.Count;
        int end = Mathf.Min(start + layout.ListSlots, total);

        RenderListEntries(optionTexts, layout.ListSlots, start, total, index => ModSettingsManager.GetDisplayTitle(ModSettingsManager.Pages[index]));
        RenderPaginationFooter(optionTexts, layout, end, total);
    }

    private static void RenderHubPage(MainMenuScript menu, TextMeshProUGUI[] optionTexts, HubLayout layout, ModSettingsManager.Page page)
    {
        if (menu.titleText != null)
            menu.titleText.text = ModSettingsManager.GetDisplayTitle(page);

        int start = HubState.PageItemOffset;
        int total = page.Items.Count;
        int end = Mathf.Min(start + layout.ListSlots, total);

        RenderListEntries(
            optionTexts,
            layout.ListSlots,
            start,
            total,
            index =>
            {
                string label = page.Items[index].Label?.Invoke() ?? "(item)";
                return label;
            });

        RenderPaginationFooter(optionTexts, layout, end, total);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuScript), nameof(MainMenuScript.Select_Desktop))]
    private static bool Select_Desktop_Prefix(MainMenuScript __instance, MainMenuScript.MenuIndex _menuIndex, int selectionIndex)
    {
        if (IsInHub() && _menuIndex == MainMenuScript.MenuIndex.mainMenu && selectionIndex == 0)
        {
            if (HubState.Current == HubState.View.Page)
            {
                Sound.Play("SoundMenuBack");
                ModSettingsManager.CancelKeybindListening();
                HubState.Current = HubState.View.HubIndex;
                HubState.PageItemOffset = 0;
            }
            else
            {
                Sound.Play("SoundMenuBack");
                HubState.Reset();
                SetDesiredNavigationIndex(__instance, GetModsSlotIndex());
            }

            __instance.OptionsUpdate();
            return false;
        }

        if (IsInHub())
        {
            HandleHubSelection(__instance, selectionIndex);
            __instance.OptionsUpdate();
            return false;
        }

        if (_menuIndex == MainMenuScript.MenuIndex.settings && HasPages())
        {
            int modsSlot = GetModsSlotIndex();
            int backSlot = GetBackSlotIndex();
            bool suppressed = InputSuppressed(__instance);

            if (selectionIndex == modsSlot && !suppressed)
            {
                Sound.Play("SoundMenuSelect");
                HubState.Current = HubState.View.HubIndex;
                HubState.HubPageOffset = 0;
                HubState.ActivePage = -1;
                HubState.PageItemOffset = 0;
                SetDesiredNavigationIndex(__instance, 0);
                __instance.OptionsUpdate();
                return false;
            }

            if (selectionIndex == backSlot && !suppressed)
            {
                Sound.Play("SoundMenuBack");
                SetMenuIndex(__instance, MainMenuScript.MenuIndex.mainMenu);
                int resumeIndex = !Master.IsDemo ? 2 : 3;
                SetDesiredNavigationIndex(__instance, resumeIndex);
                HubState.Reset();
                __instance.OptionsUpdate();
                return false;
            }
        }

        return true;
    }

    private static void HandleHubSelection(MainMenuScript menu, int selectionIndex)
    {
        if (!TryGetHubLayout(menu, out var layout))
            return;

        if (HubState.Current == HubState.View.HubIndex)
        {
            HandleHubIndexSelection(menu, selectionIndex, layout);
        }
        else if (HubState.Current == HubState.View.Page)
        {
            int direction = GetNavigationDirection(menu);
            HandleHubPageSelection(menu, selectionIndex, layout, direction);
        }
    }

    private static void HandleHubIndexSelection(MainMenuScript menu, int selectionIndex, HubLayout layout)
    {
        int start = HubState.HubPageOffset;
        int total = ModSettingsManager.Pages.Count;
        int end = Mathf.Min(start + layout.ListSlots, total);

        if (selectionIndex >= 0 && selectionIndex < layout.ListSlots)
        {
            int idx = start + selectionIndex;
            if (idx >= start && idx < end)
            {
                Sound.Play("SoundMenuSelect");
                HubState.ActivePage = idx;
                HubState.PageItemOffset = 0;
                HubState.Current = HubState.View.Page;
                SetDesiredNavigationIndex(menu, 0);
            }
            return;
        }

        if (selectionIndex == layout.NextRow && end < total)
        {
            Sound.Play("SoundMenuSelect");
            HubState.HubPageOffset = ClampPageOffset(HubState.HubPageOffset + layout.Step, total);
            return;
        }

        if (selectionIndex == layout.BackRow)
        {
            Sound.Play("SoundMenuBack");
            HubState.Reset();
            SetDesiredNavigationIndex(menu, GetModsSlotIndex());
        }
    }

    private static void HandleHubPageSelection(MainMenuScript menu, int selectionIndex, HubLayout layout, int direction)
    {
        if (!TryGetActivePage(out var page))
            return;

        int start = HubState.PageItemOffset;
        int total = page.Items.Count;
        int end = Mathf.Min(start + layout.ListSlots, total);

        if (selectionIndex >= 0 && selectionIndex < layout.ListSlots)
        {
            int idx = start + selectionIndex;
            if (idx >= start && idx < end)
            {
                var item = page.Items[idx];
                bool invoked = direction != 0
                    ? TryInvokeAdjust(page, idx, item, direction)
                    : TryInvokeSelect(page, idx, item);

                if (invoked)
                    Sound.Play("SoundMenuSelect");
            }
            return;
        }

        if (selectionIndex == layout.NextRow && end < total)
        {
            Sound.Play("SoundMenuSelect");
            HubState.PageItemOffset = ClampPageOffset(HubState.PageItemOffset + layout.Step, total);
            return;
        }

        if (selectionIndex == layout.BackRow)
        {
            Sound.Play("SoundMenuBack");
            ModSettingsManager.CancelKeybindListening();
            HubState.Current = HubState.View.HubIndex;
            HubState.PageItemOffset = 0;
            HubState.ActivePage = -1;
            SetDesiredNavigationIndex(menu, 0);
        }
    }

    private static int GetNavigationDirection(MainMenuScript menu)
    {
        bool right = GetFlag(RightNavigationPressField, menu);
        bool left = GetFlag(LeftNavigationPressField, menu);
        int direction = right ? 1 : (left ? -1 : 0);

        if (direction == 0 && Controls.MouseButton_PressedGet(0, Controls.MouseElement.RightButton))
            direction = -1;

        return direction;
    }

    private static bool TryInvokeSelect(ModSettingsManager.Page page, int itemIndex, ModSettingsManager.Item item)
    {
        if (item == null || item.OnSelect == null)
            return true;

        return TryInvokeItemHandler(page, itemIndex, "OnSelect", item.OnSelect);
    }

    private static bool TryInvokeAdjust(ModSettingsManager.Page page, int itemIndex, ModSettingsManager.Item item, int direction)
    {
        if (item == null || item.OnAdjust == null)
            return true;

        return TryInvokeItemHandler(page, itemIndex, "OnAdjust", () => item.OnAdjust(direction));
    }

    private static bool TryInvokeItemHandler(ModSettingsManager.Page page, int itemIndex, string handler, Action invoke)
    {
        try
        {
            invoke();
            return true;
        }
        catch (Exception ex)
        {
            LogHandlerException(handler, page, itemIndex, ex);
            return false;
        }
    }

    private static void LogHandlerException(string handler, ModSettingsManager.Page page, int itemIndex, Exception ex)
    {
        string pageName = page?.Name ?? "(unnamed page)";
        LogError($"Exception during {handler} for page '{pageName}' item index {itemIndex}: {ex}");
    }
}
#pragma warning restore Harmony003
