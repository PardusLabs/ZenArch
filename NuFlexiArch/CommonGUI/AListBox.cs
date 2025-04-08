﻿using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace NuFlexiArch;

[JsonSerializable(typeof(ListBoxItem))]
[JsonSerializable(typeof(ListBoxStateDto))]
public partial class ListBoxSerializerContext : JsonSerializerContext { }

public class ListBoxItem
{
    public string? Value { get; set; }
    public string? Label { get; set; }
}

public class ListBoxStateDto : CompStateDto
{
    public bool AllowMultiple { get; set; }
    public List<ListBoxItem> Items { get; set; } = new();
    public List<string> SelectedValues { get; set; } = new();
}

public abstract class AListBox : AComponent
{
    public abstract void SetAllowMultiple(bool allow);
    public abstract bool GetAllowMultiple();

    public abstract void SetItems(List<ListBoxItem> items);
    public abstract List<ListBoxItem> GetItems();

    public abstract void SetSelectedValues(List<string> values);
    public abstract List<string> GetSelectedValues();

    public override bool SetState(CompStateDto tempDto)
    {
        if (tempDto is ListBoxStateDto lbDto)
        {
            SetAllowMultiple(lbDto.AllowMultiple);
            SetItems(lbDto.Items);
            SetSelectedValues(lbDto.SelectedValues);
            return true;
        }
        throw new ArgumentException("Invalid DTO type for AListBox.");
    }

    public override CompStateDto GetState()
    {
        var dto = new ListBoxStateDto
        {
            AllowMultiple = GetAllowMultiple(),
            Items = GetItems(),
            SelectedValues = GetSelectedValues()
        };
        return dto;
    }

    public override JsonTypeInfo GetJsonTypeInfo()
    {
        return ListBoxSerializerContext.Default.ListBoxStateDto;
    }
}