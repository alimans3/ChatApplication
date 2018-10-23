using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[ContentProperty(nameof(NamedSize))]
public class NamedSizeExtension : IMarkupExtension
{
    public NamedSize NamedSize { get; set; }

    public Type TargetType { get; set; } = typeof(Label);

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return Device.GetNamedSize(NamedSize, TargetType);
    }
}
