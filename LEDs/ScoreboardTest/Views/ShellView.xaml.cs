using Coding4Fun.Toolkit.Controls;
using ScoreboardTest.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ScoreboardTest.Views
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class ShellView : Page
  {
    public ShellView()
    {
      // First time execution, initialize the logger 
      InitializeComponent();

      // Issues with 'Error Object reference not set to an instance of an object'
      // this.ColourPicker.ColorChanged += ColourPicker_ColorChanged;

      //< !--< Controls:ColorPicker
      //          Name = "ColourPicker"
      //        HorizontalAlignment = "Stretch"
      //        VerticalAlignment = "Stretch"
      //        Grid.Row = "0" Grid.Column = "1" Grid.RowSpan = "3" Grid.ColumnSpan = "1" /> -->}
      var colorPicker = new ColorPicker();
      Grid.SetRow(colorPicker, 0);
      Grid.SetColumn(colorPicker, 1);
      Grid.SetRowSpan(colorPicker, 3);
      Grid.SetColumn(colorPicker, 1);
      colorPicker.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
      colorPicker.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
      colorPicker.ColorChanged += (sender, color) =>
      {
        if (DataContext is ShellViewModel)
          ((ShellViewModel)DataContext).ColourChanged(color);
      };

      PageGrid.Children.Add(colorPicker);
    }
  }
}
