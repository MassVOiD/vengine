using System.Windows.Forms;

namespace VEngine
{
    public partial class SettingsWindow : Form
    {
        public SettingsWindow(GraphicsSettings settings)
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = settings;
            propertyGrid1.PropertyValueChanged += propertyGrid1_PropertyValueChanged;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var field = propertyGrid1.SelectedObject.GetType().GetProperty(e.ChangedItem.Label.Trim());
            field.SetValue(Game.GraphicsSettings, e.ChangedItem.Value);
        }
    }
}