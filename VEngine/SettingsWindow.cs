using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var field = propertyGrid1.SelectedObject.GetType().GetProperty(e.ChangedItem.Label.Trim());
            field.SetValue(GLThread.GraphicsSettings, e.ChangedItem.Value);
        }
    }
}
