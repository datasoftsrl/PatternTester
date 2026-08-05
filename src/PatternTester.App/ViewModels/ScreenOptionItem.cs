using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PatternTester.App.ViewModels;

public class ScreenOptionItem : INotifyPropertyChanged
{
    public ScreenOptionItem(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public string DisplayValue => Value.ToString("00");

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
