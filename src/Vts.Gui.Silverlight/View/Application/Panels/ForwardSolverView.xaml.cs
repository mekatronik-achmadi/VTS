﻿using System.Windows.Controls;

namespace Vts.Gui.Silverlight.View
{
    public partial class ForwardSolverView : UserControl
    {
        public ForwardSolverView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tbx = sender as TextBox;
            if (tbx != null && e.Key == Key.Enter)
                tbx.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
