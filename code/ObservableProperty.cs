﻿using System;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// Refresh changes for bound properties
/// </summary>
/// <typeparam name="T">Type</typeparam>
public class ObservableProperty<T> : INotifyPropertyChanged
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            NotifyPropertyChanged(nameof(Value));
        }
    }
    public void NotifyValue()
    {
        NotifyPropertyChanged(nameof(Value));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    internal void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}