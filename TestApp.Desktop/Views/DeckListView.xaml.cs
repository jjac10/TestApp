using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestApp.Desktop.Views;

public partial class DeckListView : UserControl
{
    public DeckListView()
    {
        InitializeComponent();
    }

    private void OnDialogBackgroundClick(object sender, MouseButtonEventArgs e)
    {
        // Cerrar el diálogo de opciones de examen al hacer clic en el fondo
        if (DataContext is ViewModels.DeckListViewModel vm)
        {
            vm.CancelExamOptionsCommand.Execute(null);
        }
    }

    private void OnDialogClick(object sender, MouseButtonEventArgs e)
    {
        // Prevenir que el clic en el diálogo cierre el mismo
        e.Handled = true;
    }
}