using Caliburn.Micro;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ScoreboardTest.Models
{
  // TODO: Document this!

  static class CustomConventions
  {
    private static bool _initialised = false;

    public static void Initialise()
    {

      if (_initialised) return;
      _initialised = true;

      ConventionManager.AddElementConvention<FrameworkElement>(
#if WINDOWS_UWP
                Control.IsEnabledProperty,
#else
                UIElement.IsEnabledProperty,
#endif
               "IsEnabled",
               "IsEnabledChanged");

      var baseBindProperties = ViewModelBinder.BindProperties;
      ViewModelBinder.BindProperties =
          (frameWorkElements, viewModels) =>
          {
            foreach (var frameworkElement in frameWorkElements)
            {
              foreach (var propertyName in new[] { $"Is{frameworkElement.Name}Enabled", $"Can{frameworkElement.Name}" })
              {
                var property = viewModels.GetPropertyCaseInsensitive(propertyName);
                if (property != null)
                {
                  var convention = ConventionManager
                    .GetElementConvention(typeof(FrameworkElement));

                  ConventionManager.SetBindingWithoutBindingOverwrite(
                    viewModels,
                    propertyName,
                    property,
                    frameworkElement,
                    convention,
                    convention.GetBindableProperty(frameworkElement));
                }
              }
            }

            return baseBindProperties(frameWorkElements, viewModels);
          };
    }
  }
}
