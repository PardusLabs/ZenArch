﻿using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace NuFlexiArch;

[JsonSerializable(typeof(DropdownOption))]
[JsonSerializable(typeof(DropdownStateDto))]
public partial class DropdownSerializerContext : JsonSerializerContext { }

public class DropdownOption
{
    public string? Value { get; set; }
    public string? Label { get; set; }
}

public class DropdownStateDto : CompStateDto
{
    public bool AllowMultiple { get; set; }
    public List<DropdownOption> Options { get; set; } = new();
    // For multi-select, store selected values. For single-select, store 1 element or just the first.
    public List<string> SelectedValues { get; set; } = new();
}

public abstract class ADropdown : AComponent
{
    public abstract void SetAllowMultiple(bool allow);
    public abstract bool GetAllowMultiple();

    public abstract void SetOptions(List<DropdownOption> options);
    public abstract List<DropdownOption> GetOptions();

    public abstract void SetSelectedValues(List<string> values);
    public abstract List<string> GetSelectedValues();

    public override bool SetState(CompStateDto tempDto)
    {
        if (tempDto is DropdownStateDto ddDto)
        {
            SetAllowMultiple(ddDto.AllowMultiple);
            SetOptions(ddDto.Options);
            SetSelectedValues(ddDto.SelectedValues);
            return true;
        }
        throw new ArgumentException("Invalid DTO type for ADropdown.");
    }

    public override CompStateDto GetState()
    {
        var dto = new DropdownStateDto
        {
            AllowMultiple = GetAllowMultiple(),
            Options = GetOptions(),
            SelectedValues = GetSelectedValues()
        };
        return dto;
    }

    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return DropdownSerializerContext.Default.DropdownStateDto;
    }
}