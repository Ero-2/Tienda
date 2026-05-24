using System.Globalization;
using Tienda.Models;

namespace Tienda.Helpers;

public class NotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        string s => !string.IsNullOrEmpty(s),
        int i    => i > 0,
        _        => value is not null
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is bool b && !b;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class FavoriteIconConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is true ? "♥" : "♡";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class StepColorConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        StepStatus.Done   => Color.FromArgb("#CCFF00"),
        StepStatus.Active => Color.FromArgb("#CCFF00"),
        _                 => Color.FromArgb("#2E2E2E")
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class StepIconConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        StepStatus.Done   => "✓",
        StepStatus.Active => "●",
        _                 => "○"
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class StepLineColorConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        StepStatus.Done => Color.FromArgb("#CCFF00"),
        _               => Color.FromArgb("#2E2E2E")
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class StepTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        StepStatus.Pending => Color.FromArgb("#444444"),
        _                  => Colors.White
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class StepSubtextColorConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value switch
    {
        StepStatus.Active => Color.FromArgb("#CCFF00"),
        StepStatus.Pending => Color.FromArgb("#333333"),
        _                  => Color.FromArgb("#555555")
    };
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is true ? Color.FromArgb("#CCFF00") : Color.FromArgb("#444444");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is true ? 1.0 : 0.3;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class NotZeroConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) =>
        value is int i ? i > 0 : value is decimal d ? d > 0 : false;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}

public class IsSelectedPaymentConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c)
    {
        if (value is string selected && p is string option)
            return selected == option;
        return false;
    }
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) =>
        throw new NotImplementedException();
}
