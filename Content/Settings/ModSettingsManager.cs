﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CloverAPI.Classes;
using CloverAPI.Utils;
using System.Linq;

namespace CloverAPI.Content.Data;


/// <summary>
/// Public entry point for other mods that want to surface settings in the CloverPit in-game menu.
/// </summary>
/// <remarks>
/// A minimal example that registers a page with a toggle and a numeric stepper:
/// <code>
/// var enabled = Config.Bind("General", "Enabled", true, "Turns the mod on or off");
/// var extraLives = Config.Bind("General", "ExtraLives", 1, new ConfigDescription("Adds more retries", new AcceptableValueRange&lt;int&gt;(0, 5)));
///
/// ModSettingsRegistry.RegisterPage(this, "My Cool Mod", page =>
/// {
///     page.AddToggle("Enable mod", enabled);
///     page.AddIntStepper("Extra lives", extraLives, min: 0, max: 5);
/// });
/// </code>
/// </remarks>
#nullable enable
public class ModSettingsManager
{
    private const int MaxAutoDecimalPlaces = 6;

    /// <summary>
    /// Controls how left/right input behaves for toggle items.
    /// </summary>
    public enum ToggleAdjustMode
    {
        /// <summary>Pressing left or right flips the boolean value.</summary>
        Toggle,
        /// <summary>Forces the value to off when pressing left and on when pressing right.</summary>
        Directional
    }

    /// <summary>
    /// Represents an item (row) within a settings page.
    /// </summary>
    public sealed class Item
    {
        public Item()
        {
        }

        internal Item(Func<string>? label, Action? onSelect, Action<int>? onAdjust)
        {
            Label = label;
            OnSelect = onSelect;
            OnAdjust = onAdjust;
        }

        /// <summary>
        /// Callback that provides the label text to render for this row.
        /// </summary>
        public Func<string>? Label { get; set; }

        /// <summary>Invoked when the user activates the row (Enter / click).</summary>
        public Action? OnSelect { get; set; }

        /// <summary>
        /// Invoked when the user presses left or right. The parameter is -1 for left and +1 for right.
        /// </summary>
        public Action<int>? OnAdjust { get; set; }
    }

    /// <summary>
    /// Represents a collection of <see cref="Item"/> entries that will be drawn as a single page.
    /// </summary>
    public sealed class Page
    {
        private readonly List<Item> items = new();

        internal Page(string name, ModGuid ownerGuid, ModName ownerName, string normalizedName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Page name must not be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(ownerGuid))
                throw new ArgumentException("Page owner GUID must not be empty.", nameof(ownerGuid));
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Normalized page name must not be empty.", nameof(normalizedName));

            Name = name;
            OwnerGuid = ownerGuid;
            OwnerName = ownerName;
            NormalizedName = normalizedName;
        }

        /// <summary>The display name shown at the top of the page.</summary>
        public string Name { get; }

        internal string OwnerGuid { get; }

        internal string OwnerName { get; }

        internal string NormalizedName { get; }

        /// <summary>The list of rows that will appear underneath the title.</summary>
        public List<Item> Items => items;

        internal Item AddItem(Func<string>? label, Action? onSelect, Action<int>? onAdjust)
        {
            return AddItem(new Item(label, onSelect, onAdjust));
        }

        internal Item AddItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            items.Add(item);
            return item;
        }
    }

    /// <summary>
    /// Fluent helper that makes it simple to populate a <see cref="Page"/> with rows.
    /// </summary>
    public sealed class PageBuilder
    {
        private static readonly IReadOnlyList<float> DefaultMultiplierSteps = Array.AsReadOnly(new[] { 0f, 0.5f, 1f, 2f, 3f, 4f });

        private readonly Page page;

        internal PageBuilder(Page page)
        {
            this.page = page ?? throw new ArgumentNullException(nameof(page));
        }

        /// <summary>
        /// The underlying page that will be registered with the hub.
        /// </summary>
        public Page BuiltPage => page;

        /// <summary>
        /// Adds an arbitrary item to the page. Useful when you want full control over behavior.
        /// </summary>
        public PageBuilder AddItem(Func<string>? label, Action? onSelect = null, Action<int>? onAdjust = null)
        {
            page.AddItem(label, onSelect, onAdjust);
            return this;
        }

        /// <summary>
        /// Adds a toggle that reacts to both selection (A/Enter) and left/right input.
        /// This core overload works with any boolean source by supplying read/write delegates.
        /// </summary>
        public PageBuilder AddToggle(
            string label,
            Func<bool> getter,
            Action<bool> setter,
            string onLabel = "On",
            string offLabel = "Off",
            Action<bool>? onChanged = null,
            ToggleAdjustMode adjustMode = ToggleAdjustMode.Toggle)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            string Format() => $"{label}: {(getter() ? onLabel : offLabel)}";

            void SetValue(bool value) => UpdateIfChanged(getter, setter, value, onChanged);

            return AddItem(
                Format,
                () => SetValue(!getter()),
                dir =>
                {
                    int direction = NormalizeDirection(dir);
                    if (direction == 0)
                        return;

                    if (adjustMode == ToggleAdjustMode.Directional)
                    {
                        SetValue(direction > 0);
                        return;
                    }

                    SetValue(!getter());
                });
        }

        /// <summary>
        /// Convenience overload of <see cref="AddToggle(string, Func{bool}, Action{bool}, string, string, Action{bool}, ToggleAdjustMode)"/>
        /// for wiring up a <see cref="ConfigEntry{T}"/> without manually providing delegates. Use this when you already have a
        /// BepInEx <c>ConfigEntry&lt;bool&gt;</c> and want to surface it directly. Defaults to "On"/"Off" labels.
        /// </summary>
        public PageBuilder AddToggle(
            string label,
            ConfigEntry<bool> entry,
            string onLabel = "On",
            string offLabel = "Off",
            Action<bool>? onChanged = null,
            ToggleAdjustMode adjustMode = ToggleAdjustMode.Toggle)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return AddToggle(label, () => entry.Value, value => entry.Value = value, onLabel, offLabel, onChanged, adjustMode);
        }

        /// <summary>
        /// Shorthand for a simple on/off toggle. Wrapper over <see cref="AddToggle(string, ConfigEntry{bool}, string, string, Action{bool}?, ToggleAdjustMode)"/>.
        /// </summary>
        public PageBuilder OnOff(
            string label,
            Func<bool> getter,
            Action<bool> setter,
            string onLabel = "On",
            string offLabel = "Off",
            Action<bool>? onChanged = null,
            ToggleAdjustMode adjustMode = ToggleAdjustMode.Toggle)
        {
            return AddToggle(label, getter, setter, onLabel, offLabel, onChanged, adjustMode);
        }

        /// <summary>
        /// Shorthand overload for <see cref="OnOff(string, Func{bool}, Action{bool}, string, string, Action{bool}?, ToggleAdjustMode)"/> when working with config entries.
        /// </summary>
        public PageBuilder OnOff(
            string label,
            ConfigEntry<bool> entry,
            string onLabel = "On",
            string offLabel = "Off",
            Action<bool>? onChanged = null,
            ToggleAdjustMode adjustMode = ToggleAdjustMode.Toggle)
        {
            return OnOff(label, () => entry.Value, value => entry.Value = value, onLabel, offLabel, onChanged, adjustMode);
        }

        /// <summary>
        /// Adds an integer value stepper. Selection increments, left/right decrements or increments.
        /// This is the primary overload that accepts delegates so you can source the value from anywhere.
        /// </summary>
        /// <param name="step">How much to add or subtract on each change.</param>
        /// <param name="min">Optional lower bound.</param>
        /// <param name="max">Optional upper bound.</param>
        /// <param name="wrap">
        /// When true and both bounds are set, stepping beyond a bound wraps around to the opposite bound.
        /// </param>
        /// <param name="normalizer">
        /// Optional hook to massage/clamp the value after stepping (useful for applying custom rounding).
        /// </param>
        /// <param name="valueFormatter">
        /// Converts the raw integer into the string that appears alongside the label.
        /// </param>
        /// <param name="onChanged">Called whenever the value changes.</param>
        public PageBuilder AddIntStepper(
            string label,
            Func<int> getter,
            Action<int> setter,
            int step = 1,
            int? min = null,
            int? max = null,
            bool wrap = false,
            Func<int, int>? normalizer = null,
            Func<int, string>? valueFormatter = null,
            Action<int>? onChanged = null)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
            if (min.HasValue && max.HasValue && min.Value > max.Value)
                throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than or equal to min.");

            int ApplyBounds(int value)
            {
                if (wrap && min.HasValue && max.HasValue && min.Value <= max.Value)
                {
                    int stepSize = Math.Max(1, step);
                    int range = (max.Value - min.Value) + stepSize;
                    if (range > 0)
                    {
                        int offset = ((value - min.Value) % range + range) % range;
                        return min.Value + offset;
                    }
                }

                if (min.HasValue && value < min.Value)
                    value = min.Value;
                if (max.HasValue && value > max.Value)
                    value = max.Value;

                return value;
            }

            int Normalize(int value)
            {
                int result = ApplyBounds(value);

                if (normalizer != null)
                    result = ApplyBounds(normalizer(result));

                return result;
            }

            void SetValue(int rawValue)
            {
                int target = Normalize(rawValue);
                UpdateIfChanged(getter, setter, target, onChanged);
            }

            string Format()
            {
                int value = getter();
                string suffix = valueFormatter?.Invoke(value) ?? value.ToString();
                return $"{label}: {suffix}";
            }

            return AddItem(
                Format,
                () => SetValue(getter() + step),
                dir =>
                {
                    int direction = NormalizeDirection(dir);
                    if (direction == 0)
                        return;

                    SetValue(getter() + (direction * step));
                });
        }

        /// <summary>
        /// Convenience overload of <see cref="AddIntStepper(string, Func{int}, Action{int}, int, int?, int?, bool, Func{int, int}?, Func{int, string}?, Action{int}?)"/>
        /// that connects directly to a <see cref="ConfigEntry{T}"/>. Handy when you already have a BepInEx <c>ConfigEntry&lt;int&gt;</c>.
        /// </summary>
        public PageBuilder AddIntStepper(
            string label,
            ConfigEntry<int> entry,
            int step = 1,
            int? min = null,
            int? max = null,
            bool wrap = false,
            Func<int, int>? normalizer = null,
            Func<int, string>? valueFormatter = null,
            Action<int>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return AddIntStepper(label, () => entry.Value, value => entry.Value = value, step, min, max, wrap, normalizer, valueFormatter, onChanged);
        }

        /// <summary>
        /// Friendly alias for <see cref="AddIntStepper(string, Func{int}, Action{int}, int, int?, int?, bool, Func{int, int}?, Func{int, string}?, Action{int}?)"/>
        /// when you want a basic integer range with optional bounds.
        /// </summary>
        public PageBuilder Int(
            string label,
            Func<int> getter,
            Action<int> setter,
            int? min = null,
            int? max = null,
            int step = 1,
            bool wrap = false,
            Func<int, int>? normalizer = null,
            Func<int, string>? valueFormatter = null,
            Action<int>? onChanged = null)
        {
            return AddIntStepper(label, getter, setter, step, min, max, wrap, normalizer, valueFormatter, onChanged);
        }

        /// <summary>
        /// Config-friendly overload of <see cref="Int(string, Func{int}, Action{int}, int?, int?, int, bool, Func{int, int}?, Func{int, string}?, Action{int}?)"/>.
        /// </summary>
        public PageBuilder Int(
            string label,
            ConfigEntry<int> entry,
            int? min = null,
            int? max = null,
            int step = 1,
            bool wrap = false,
            Func<int, int>? normalizer = null,
            Func<int, string>? valueFormatter = null,
            Action<int>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Int(label, () => entry.Value, value => entry.Value = value, min, max, step, wrap, normalizer, valueFormatter, onChanged);
        }

        /// <summary>
        /// Adds a percentage stepper. The label automatically renders the value with a <c>%</c> suffix.
        /// </summary>
        public PageBuilder Percent(
            string label,
            Func<int> getter,
            Action<int> setter,
            int minPercent = 0,
            int maxPercent = 100,
            int step = 5,
            bool wrap = false,
            bool showPercent = true,
            float scale = 1f,
            Action<int>? onChanged = null)
        {
            if (maxPercent < minPercent)
                throw new ArgumentOutOfRangeException(nameof(maxPercent), "maxPercent must be greater than or equal to minPercent.");

            string Formatter(int value)
            {
                int actual = (int)(value * scale);
                return showPercent ? $"{actual}%" : actual.ToString();
            }
            return AddIntStepper(label, getter, setter, step, minPercent, maxPercent, wrap, valueFormatter: Formatter, onChanged: onChanged);
        }

        /// <summary>
        /// Config-friendly overload of <see cref="Percent(string, Func{int}, Action{int}, int, int, int, bool, Action{int}?)"/>.
        /// </summary>
        public PageBuilder Percent(
            string label,
            ConfigEntry<int> entry,
            int minPercent = 0,
            int maxPercent = 100,
            int step = 5,
            bool wrap = false,
            bool showPercent = true,
            float scale = 1f,
            Action<int>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Percent(label, () => entry.Value, value => entry.Value = value, minPercent, maxPercent, step, wrap, showPercent, scale, onChanged);
        }

        /// <summary>
        /// Adds a percentage stepper that supports fractional values. Values are formatted with a <c>%</c> suffix.
        /// </summary>
        public PageBuilder Percent(
            string label,
            Func<float> getter,
            Action<float> setter,
            float minPercent = 0f,
            float maxPercent = 100f,
            float step = 5f,
            bool wrap = false,
            int decimalPlaces = 1,
            bool showPercent = true,
            float scale = 1f,
            Action<float>? onChanged = null)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));
            if (step <= 0f)
                throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "decimalPlaces must be zero or greater.");

            float currentValue = getter();
            int autoDecimalPlaces = GetRequiredDecimalPlaces(minPercent, maxPercent, step, currentValue);
            int effectiveDecimalPlaces = Math.Max(decimalPlaces, autoDecimalPlaces);

            int localScale = Pow10(effectiveDecimalPlaces);

            int Scale(float value)
            {
                decimal scaled = (decimal)value * localScale;
                return (int)Math.Round(scaled, MidpointRounding.AwayFromZero);
            }

            float Unscale(int value) => (float)value / localScale;

            int stepScaled = Scale(step);
            if (stepScaled <= 0)
                throw new ArgumentOutOfRangeException(nameof(step), "Step is too small for the configured decimalPlaces.");

            int minScaled = Scale(minPercent);
            int maxScaled = Scale(maxPercent);
            if (maxScaled < minScaled)
                throw new ArgumentOutOfRangeException(nameof(maxPercent), "maxPercent must be greater than or equal to minPercent.");

            string FormatPercentLabel(int scaledValue)
            {
                float actual = Unscale(scaledValue) * scale;
                string format = effectiveDecimalPlaces > 0 ? $"F{effectiveDecimalPlaces}" : "F0";
                string text = actual.ToString(format, CultureInfo.InvariantCulture);
                if (effectiveDecimalPlaces > 0)
                    text = text.TrimEnd('0').TrimEnd('.');
                return showPercent ? $"{text}%" : text;
            }

            Action<int>? wrappedOnChanged = null;
            if (onChanged != null)
            {
                wrappedOnChanged = value => onChanged(Unscale(value));
            }

            return AddIntStepper(
                label,
                () => Scale(getter()),
                value => setter(Unscale(value)),
                step: stepScaled,
                min: minScaled,
                max: maxScaled,
                wrap: wrap,
                valueFormatter: FormatPercentLabel,
                onChanged: wrappedOnChanged);
        }

        /// <summary>
        /// Config-friendly overload of <see cref="Percent(string, Func{float}, Action{float}, float, float, float, bool, int, Action{float}?)"/>.
        /// </summary>
        public PageBuilder Percent(
            string label,
            ConfigEntry<float> entry,
            float minPercent = 0f,
            float maxPercent = 100f,
            float step = 5f,
            bool wrap = false,
            int decimalPlaces = 1,
            bool showPercent = true,
            float scale = 1f,
            Action<float>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Percent(label, () => entry.Value, value => entry.Value = value, minPercent, maxPercent, step, wrap, decimalPlaces, showPercent, scale, onChanged);
        }

        /// <summary>
        /// Adds a multiplier selector backed by common 0x-4x options.
        /// </summary>
        public PageBuilder Multiplier(
            string label,
            Func<float> getter,
            Action<float> setter,
            float minMultiplier,
            float maxMultiplier,
            float step,
            bool wrap = false,
            int decimalPlaces = 2,
            Func<float, string>? valueFormatter = null,
            Action<float>? onChanged = null)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));
            if (step <= 0f)
                throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
            if (decimalPlaces < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "decimalPlaces must be zero or greater.");

            float currentValue = getter();
            int autoDecimalPlaces = GetRequiredDecimalPlaces(minMultiplier, maxMultiplier, step, currentValue);
            int effectiveDecimalPlaces = Math.Max(decimalPlaces, autoDecimalPlaces);

            int scale = Pow10(effectiveDecimalPlaces);

            int Scale(float value)
            {
                decimal scaled = (decimal)value * scale;
                return (int)Math.Round(scaled, MidpointRounding.AwayFromZero);
            }

            float Unscale(int value) => (float)value / scale;

            int stepScaled = Scale(step);
            if (stepScaled <= 0)
                throw new ArgumentOutOfRangeException(nameof(step), "Step is too small for the configured decimalPlaces.");

            int minScaled = Scale(minMultiplier);
            int maxScaled = Scale(maxMultiplier);
            if (maxScaled < minScaled)
                throw new ArgumentOutOfRangeException(nameof(maxMultiplier), "maxMultiplier must be greater than or equal to minMultiplier.");

            string FormatMultiplierLabel(int scaledValue)
            {
                float actual = Unscale(scaledValue);
                if (valueFormatter != null)
                    return valueFormatter(actual);

                string format = effectiveDecimalPlaces > 0 ? $"F{effectiveDecimalPlaces}" : "F0";
                string text = actual.ToString(format, CultureInfo.InvariantCulture);
                if (effectiveDecimalPlaces > 0)
                    text = text.TrimEnd('0').TrimEnd('.');
                return $"{text}x";
            }

            Action<int>? wrappedOnChanged = null;
            if (onChanged != null)
            {
                wrappedOnChanged = value => onChanged(Unscale(value));
            }

            return AddIntStepper(
                label,
                () => Scale(getter()),
                value => setter(Unscale(value)),
                step: stepScaled,
                min: minScaled,
                max: maxScaled,
                wrap: wrap,
                valueFormatter: FormatMultiplierLabel,
                onChanged: wrappedOnChanged);
        }

        /// <summary>
        /// Convenience overload of <see cref="Multiplier(string, Func{float}, Action{float}, float, float, float, bool, int, Func{float, string}?, Action{float}?)"/> for config entries.
        /// </summary>
        public PageBuilder Multiplier(
            string label,
            ConfigEntry<float> entry,
            float minMultiplier,
            float maxMultiplier,
            float step,
            bool wrap = false,
            int decimalPlaces = 2,
            Func<float, string>? valueFormatter = null,
            Action<float>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Multiplier(label, () => entry.Value, value => entry.Value = value, minMultiplier, maxMultiplier, step, wrap, decimalPlaces, valueFormatter, onChanged);
        }

        /// <summary>
        /// Adds a multiplier selector backed by a fixed list of options.
        /// </summary>
        public PageBuilder Multiplier(
            string label,
            Func<float> getter,
            Action<float> setter,
            IReadOnlyList<float>? options = null,
            Func<float, string>? valueFormatter = null,
            Action<float>? onChanged = null)
        {
            IReadOnlyList<float> values = options ?? DefaultMultiplierSteps;

            int optionDecimalPlaces = valueFormatter == null ? GetRequiredDecimalPlaces(values) : 0;

            string formatter(float value)
            {
                if (valueFormatter != null)
                    return valueFormatter(value);

                string format = optionDecimalPlaces > 0 ? $"F{optionDecimalPlaces}" : "F0";
                string text = value.ToString(format, CultureInfo.InvariantCulture);
                if (optionDecimalPlaces > 0)
                    text = text.TrimEnd('0').TrimEnd('.');
                return $"{text}x";
            }

            return Cycle(label, getter, setter, values, formatter, onChanged);
        }

        /// <summary>
        /// Convenience overload of <see cref="Multiplier(string, Func{float}, Action{float}, IReadOnlyList{float}?, Func{float, string}?, Action{float}?)"/> for config entries.
        /// </summary>
        public PageBuilder Multiplier(
            string label,
            ConfigEntry<float> entry,
            IReadOnlyList<float>? options = null,
            Func<float, string>? valueFormatter = null,
            Action<float>? onChanged = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Multiplier(label, () => entry.Value, value => entry.Value = value, options, valueFormatter, onChanged);
        }

        /// <summary>
        /// Adds a cycle selector that loops through a predefined list of values.
        /// </summary>
        public PageBuilder Cycle<T>(
            string label,
            Func<T> getter,
            Action<T> setter,
            IReadOnlyList<T> values,
            Func<T, string>? valueFormatter = null,
            Action<T>? onChanged = null,
            IEqualityComparer<T>? comparer = null)
        {
            return CycleInternal(label, getter, setter, values, valueFormatter, onChanged, comparer);
        }

        /// <summary>
        /// Adds a cycle selector using a params array of values.
        /// </summary>
        public PageBuilder Cycle<T>(
            string label,
            Func<T> getter,
            Action<T> setter,
            params T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return CycleInternal(label, getter, setter, Array.AsReadOnly(values), null, null, null);
        }

        /// <summary>
        /// Config-friendly overload of <see cref="Cycle{T}(string, Func{T}, Action{T}, IReadOnlyList{T}, Func{T, string}?, Action{T}?, IEqualityComparer{T}?)"/>.
        /// </summary>
        public PageBuilder Cycle<T>(
            string label,
            ConfigEntry<T> entry,
            IReadOnlyList<T> values,
            Func<T, string>? valueFormatter = null,
            Action<T>? onChanged = null,
            IEqualityComparer<T>? comparer = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            return Cycle(label, () => entry.Value, value => entry.Value = value, values, valueFormatter, onChanged, comparer);
        }

        /// <summary>
        /// Config-friendly overload that accepts a params array of values.
        /// </summary>
        public PageBuilder Cycle<T>(
            string label,
            ConfigEntry<T> entry,
            params T[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return Cycle(label, entry, Array.AsReadOnly(values));
        }

        /// <summary>
        /// Finalizes the page. Doesn't actually do anything, as pages are registered immediately upon creation. It's
        /// just here for code clarity.
        /// </summary>
        public void Build() { }

        private PageBuilder CycleInternal<T>(
            string label,
            Func<T> getter,
            Action<T> setter,
            IReadOnlyList<T> values,
            Func<T, string>? valueFormatter,
            Action<T>? onChanged,
            IEqualityComparer<T>? comparer)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count == 0)
                throw new ArgumentException("At least one value must be supplied.", nameof(values));

            IEqualityComparer<T> equalityComparer = comparer ?? EqualityComparer<T>.Default;

            int FindIndex(T candidate)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (equalityComparer.Equals(values[i], candidate))
                        return i;
                }

                return -1;
            }

            string RenderValue(T value)
            {
                if (valueFormatter != null)
                    return valueFormatter(value);

                if (value is IFormattable formattable)
                    return formattable.ToString(null, CultureInfo.InvariantCulture);

                return value?.ToString() ?? string.Empty;
            }

            string Format()
            {
                T value = getter();
                return $"{label}: {RenderValue(value)}";
            }

            void Advance(int direction)
            {
                int step = NormalizeDirection(direction);
                if (step == 0 || values.Count == 1)
                    return;

                T current = getter();
                int index = FindIndex(current);

                int nextIndex;
                if (index < 0)
                {
                    nextIndex = step > 0 ? 0 : values.Count - 1;
                }
                else
                {
                    nextIndex = (index + step + values.Count) % values.Count;
                }

                T next = values[nextIndex];
                UpdateIfChanged(getter, setter, next, onChanged);
            }

            return AddItem(
                Format,
                () => Advance(+1),
                dir => Advance(dir));
        }

        private static int NormalizeDirection(int value)
        {
            if (value == 0)
                return 0;

            return value > 0 ? 1 : -1;
        }

        private static int Pow10(int exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent));

            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result = checked(result * 10);
            }

            return result;
        }

        private static int GetRequiredDecimalPlaces(IReadOnlyList<float> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            int required = 0;
            for (int i = 0; i < values.Count; i++)
            {
                int places = CountDecimalPlaces(values[i]);
                if (places > required)
                    required = places;
            }

            return required;
        }

        private static int GetRequiredDecimalPlaces(params float[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            int required = 0;
            foreach (float value in values)
            {
                int places = CountDecimalPlaces(value);
                if (places > required)
                    required = places;
            }

            return required;
        }

        private static int CountDecimalPlaces(float value)
        {
            decimal dec = decimal.Round((decimal)value, MaxAutoDecimalPlaces, MidpointRounding.AwayFromZero);
            dec = Math.Abs(dec);

            for (int places = 0; places <= MaxAutoDecimalPlaces; places++)
            {
                decimal scaled = dec * Pow10(places);
                if (decimal.Truncate(scaled) == scaled)
                    return places;
            }

            return MaxAutoDecimalPlaces;
        }

        private static void UpdateIfChanged<T>(Func<T> getter, Action<T> setter, T value, Action<T>? onChanged)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            T current = getter();
            if (EqualityComparer<T>.Default.Equals(current, value))
                return;

            setter(value);
            onChanged?.Invoke(value);
        }
    }

    private static readonly List<Page> pages = new();
    private static readonly Dictionary<string, Dictionary<string, Page>> pagesByOwner = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, HashSet<string>> ownersByDisplayName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Exposes the collection of registered pages (used by the hub to render everything).
    /// </summary>
    public static IReadOnlyList<Page> Pages => pages;

    internal static string GetDisplayTitle(Page page)
    {
        if (page == null)
            return string.Empty;

        string name = page.Name ?? string.Empty;
        string normalized = page.NormalizedName ?? NormalizePageName(name);

        if (!ownersByDisplayName.TryGetValueNoCase(normalized, out var owners) || owners.Count <= 1)
            return name;

        string ownerLabel = page.OwnerName;
        if (string.IsNullOrWhiteSpace(ownerLabel))
            ownerLabel = page.OwnerGuid;

        const int maxOwnerLabelLength = 8;
        if (!string.IsNullOrEmpty(ownerLabel) && ownerLabel.Length > maxOwnerLabelLength)
            ownerLabel = ownerLabel.Substring(0, maxOwnerLabelLength);

        return string.IsNullOrWhiteSpace(ownerLabel) ? name : $"{name} ({ownerLabel})";
    }

    private static Page AddOrReplacePage(Page page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));
        if (string.IsNullOrWhiteSpace(page.OwnerGuid))
            throw new ArgumentException("Page must have an owner GUID.", nameof(page));
        if (string.IsNullOrWhiteSpace(page.NormalizedName))
            throw new ArgumentException("Page must have a normalized name.", nameof(page));

        TrackDisplayName(page);

        string ownerGuid = page.OwnerGuid;
        string normalizedName = page.NormalizedName;

        if (!pagesByOwner.TryGetValueNoCase(ownerGuid, out var ownerPages))
        {
            ownerPages = new Dictionary<string, Page>(StringComparer.OrdinalIgnoreCase);
            pagesByOwner[ownerGuid] = ownerPages;
        }
        
        if (ownerPages.TryGetValueNoCase(normalizedName, out var existing))
        {
            int index = pages.IndexOf(existing);
            if (index >= 0)
            {
                pages.RemoveAt(index);
                pages.Insert(index, page);
            }
            else
            {
                pages.Add(page);
            }

            ownerPages[normalizedName] = page;
            return page;
        }

        ownerPages[normalizedName] = page;
        pages.Add(page);
        return page;
    }

    private static void TrackDisplayName(Page page)
    {
        string normalizedName = page.NormalizedName ?? NormalizePageName(page.Name ?? string.Empty);
        string ownerGuid = page.OwnerGuid ?? string.Empty;

        if (!ownersByDisplayName.TryGetValueNoCase(normalizedName, out var owners))
        {
            owners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ownersByDisplayName[normalizedName] = owners;
        }

        if (!string.IsNullOrWhiteSpace(ownerGuid))
            owners.Add(ownerGuid);
    }

    private static string NormalizePageName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        string trimmed = name.Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("Page name must not be empty.", nameof(name));

        return trimmed;
    }

    private static Page CreatePageShell(BaseUnityPlugin owner, string name)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        string normalizedName = NormalizePageName(name);
        string displayName = name.Trim();

        return new Page(displayName, owner, owner, normalizedName);
    }

    /// <summary>
    /// Creates a page and immediately invokes <paramref name="configure"/> (if provided) so you can populate it inline.
    /// </summary>
    public static PageBuilder RegisterPage(BaseUnityPlugin owner, string name, Action<PageBuilder>? configure = null)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        
        var page = CreatePageShell(owner, name);
        var builder = new PageBuilder(page);

        try
        {
            if (configure != null)
            {
                configure(builder);
            }
        }
        catch (Exception ex)
        {
            LogError($"Exception while configuring settings page '{page.Name}': {ex}");
            throw;
        }

        AddOrReplacePage(page);
        return builder;
    }

    /// <summary>
    /// Legacy overload that infers the owner from the calling assembly. Prefer <see cref="RegisterPage(BaseUnityPlugin, string, Action{PageBuilder})" />.
    /// </summary>
    [Obsolete("Use RegisterPage(BaseUnityPlugin, string, Action<PageBuilder>) instead. This overload infers the owner from the calling assembly, which is fragile and may break in certain scenarios.")]
    public static PageBuilder RegisterPage(string name, Action<PageBuilder> configure)
    {
        var owner = ResolveOwnerForLegacyCall(Assembly.GetCallingAssembly());
        return RegisterPage(owner, name, configure);
    }
    
    /// <summary>
    /// Creates a page and populates it based on the config entries of the specified plugin, then invokes <paramref name="configure"/> so you can populate remaining items inline.
    /// </summary>
    public static PageBuilder RegisterPageFromConfig(BaseUnityPlugin owner, string name, string[]? ignoredKeys = null, string[]? ignoredCategories = null, Action<PageBuilder>? configure = null)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var page = CreatePageShell(owner, name);
        var builder = new PageBuilder(page);
        PopulateFromConfig(builder, owner, ignoredKeys, ignoredCategories);

        try
        {
            if (configure != null)
            {
                configure(builder);
            }
        }
        catch (Exception ex)
        {
            LogError($"Exception while configuring settings page '{page.Name}': {ex}");
            throw;
        }

        AddOrReplacePage(page);
        return builder;
    }
    
    /// <summary>
    /// Legacy overload that infers the owner from the calling assembly. Prefer <see cref="RegisterPage(BaseUnityPlugin, string, string[], string[], Action{PageBuilder})" />.
    /// </summary>
    [Obsolete("Use RegisterPageFromConfig(BaseUnityPlugin, string, string[], string[], Action<PageBuilder>) instead. This overload infers the owner from the calling assembly, which is fragile and may break in certain scenarios.")]
    public static PageBuilder RegisterPageFromConfig(string name, string[] ignoredKeys, string[] ignoredCategories, Action<PageBuilder>? configure = null)
    {
        var owner = ResolveOwnerForLegacyCall(Assembly.GetCallingAssembly());
        return RegisterPageFromConfig(owner, name, ignoredKeys, ignoredCategories, configure);
    }
    
    private static void PopulateFromConfig(PageBuilder builder, BaseUnityPlugin owner, string[]? ignoredKeys, string[]? ignoredCategories)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        var entries = owner.Config?.Where(e =>
        {
            if (e.Key?.Key == null)
                return false;
            
            if (ignoredKeys != null && ignoredKeys.Contains(e.Key.Key, StringComparer.OrdinalIgnoreCase))
                return false;

            if (ignoredCategories != null && ignoredCategories.Contains(e.Key.Section, StringComparer.OrdinalIgnoreCase))
                return false;

            return true;
        }).ToList();

        if (entries == null || !entries.Any())
            return;

        foreach (var entry in entries)
        {
            try
            {
                var v = entry.Value.BoxedValue;
                if (v is bool)
                {
                    builder.OnOff(entry.Key.Key, (ConfigEntry<bool>)entry.Value);
                }
                else if (v is int)
                {
                    int? min = null;
                    int? max = null;
                    var range = entry.Value.Description.AcceptableValues;
                    if (range is AcceptableValueRange<int> intRange)
                    {
                        min = intRange.MinValue;
                        max = intRange.MaxValue;
                    }
                    builder.Int(entry.Key.Key, (ConfigEntry<int>)entry.Value, min, max);
                }
                else if (v is float)
                {
                    float min = 0;
                    float max = 100f;
                    var range = entry.Value.Description.AcceptableValues;
                    if (range is AcceptableValueRange<float> floatRange)
                    {
                        min = floatRange.MinValue;
                        max = floatRange.MaxValue;
                    }
                    GetScaleValues(min, max, out float step, out int decimalPlaces, out bool isPercent, out float scale);

                    builder.Percent(entry.Key.Key, (ConfigEntry<float>)entry.Value, min, max, step, decimalPlaces: decimalPlaces, scale: scale);
                }

                else if (v is string)
                {
                    var configEntry = (ConfigEntry<string>)entry.Value;
                    if (configEntry.Description?.AcceptableValues is AcceptableValueList<string> options && options.AcceptableValues != null && options.AcceptableValues.Length > 0)
                    {
                        builder.Cycle(entry.Key.Key, configEntry, options.AcceptableValues);
                    }
                }
                else if (v is Enum)
                {
                    var configEntry = (ConfigEntry<Enum>)entry.Value;
                    var enumType = configEntry.Value.GetType();
                    var enumValues = Enum.GetValues(enumType).Cast<Enum>().ToArray();
                    builder.Cycle(entry.Key.Key, configEntry, enumValues);
                }
                // Other types are not supported by default and ignored.
            }
            catch (Exception ex)
            {
                LogError($"Exception while adding config entry '{entry.Value.Definition.Section}.{entry.Value.Definition.Key}' to settings page '{builder.BuiltPage.Name}': {ex}");
            }
        }
    }
    
    private static void GetScaleValues(float min, float max, out float step, out int decimalPlaces, out bool isPercent, out float scale)
    {
        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        // Determine if the range is suitable for percentage display.
        var isRawPct = min is >= 0f and < 50f && max is > 50f and <= 500f;
        var isFloatPct = min is >= 0f and < 0.5f && max is > 0.5f and <= 5f;
        isPercent = isRawPct || isFloatPct;
        scale = isFloatPct ? 100f : 1f;
        var delta = max - min;
        var roughStep = delta / 20f;
        step = MathUtils.RoundToNearestSignificantFive(roughStep);
        decimalPlaces = Math.Max(0, Math.Min(4, Mathf.RoundToInt(Mathf.Log10(1f / step))));
    }   

    /// <summary>
    /// Adds a new item to the supplied page. Usually you want to use the higher-level helpers on <see cref="PageBuilder"/>.
    /// </summary>
    public static Item RegisterItem(Page page, Func<string>? label, Action? onSelect = null, Action<int>? onAdjust = null)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        return page.AddItem(label, onSelect, onAdjust);
    }

    private static BaseUnityPlugin ResolveOwnerForLegacyCall(Assembly caller)
    {
        if (caller == null)
            throw new ArgumentNullException(nameof(caller));

        BaseUnityPlugin? owner = TryResolveOwnerFromAssembly(caller);
        if (owner != null)
            return owner;

        throw new InvalidOperationException("Unable to infer the plugin owning this settings page. Call the overload that accepts a BaseUnityPlugin instance.");
    }

    private static BaseUnityPlugin? TryResolveOwnerFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            return null;

        foreach (var entry in Chainloader.PluginInfos)
        {
            var info = entry.Value;
            if (info?.Instance == null)
                continue;

            if (info.Instance.GetType().Assembly == assembly)
                return info.Instance;
        }

        return null;
    }
}