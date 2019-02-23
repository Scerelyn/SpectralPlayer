using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpectralPlayerApp.UIComponents
{
    /// <summary>
    /// Offers the user a visual representation of a background/async task in progress
    /// </summary>
    public partial class BackgroundTaskControl : UserControl
    {
        /// <summary>
        /// The parent Panel instance this BackgroundTaskControl is a child of
        /// </summary>
        private Panel ParentControl { get; set; }

        /// <summary>
        /// Creates a BackgroundTaskControl
        /// </summary>
        /// <param name="parentControl">A Panel instance this should be a child to</param>
        /// <param name="taskDescription">A description of the task this control represents</param>
        public BackgroundTaskControl(Panel parentControl, string taskDescription)
        {
            InitializeComponent();
            this.BackgroundTaskLabel.Content = taskDescription;
            ParentControl = parentControl;
            ParentControl.Children.Add(this);
        }

        /// <summary>
        /// Removes this BackgroundTaskControl from its parent Panel instance
        /// </summary>
        public void Dispose()
        {
            ParentControl.Children.Remove(this);
        }
    }
}
