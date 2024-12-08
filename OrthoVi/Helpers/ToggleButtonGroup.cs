using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrthoVi.Helpers
{
    internal class ToggleButtonGroup
    {
        public static readonly AttachedProperty<string> GroupNameProperty =
        AvaloniaProperty.RegisterAttached<ToggleButton, string>("GroupName", typeof(ToggleButtonGroup));

        public static void SetGroupName(AvaloniaObject element, string value) =>
            element.SetValue(GroupNameProperty, value);

        public static string GetGroupName(AvaloniaObject element) =>
            element.GetValue(GroupNameProperty);

        static ToggleButtonGroup()
        {
            GroupNameProperty.Changed.AddClassHandler<ToggleButton>((button, args) =>
            {
                if (button is ToggleButton toggleButton)
                {
                    toggleButton.Checked += (s, e) => UncheckOthers(toggleButton);
                }
            });
        }

        private static void UncheckOthers(ToggleButton checkedButton)
        {
            var groupName = GetGroupName(checkedButton);
            if (checkedButton.Parent is Panel parentPanel)
            {
                foreach (var child in parentPanel.Children)
                {
                    if (child is ToggleButton tb &&
                        tb != checkedButton &&
                        GetGroupName(tb) == groupName)
                    {
                        tb.IsChecked = false;
                    }
                }
            }
        }
    }
}
