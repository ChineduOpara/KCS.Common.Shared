using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KCS.Common.Shared
{
    public static class ControlsExtensions
    {
        public static Form GetForm(Type type)
        {
            Form form = null;
            foreach (Form f in Application.OpenForms)
            {
                if (f.GetType().Equals(type))
                {
                    if (f.WindowState == FormWindowState.Minimized)
                    {
                        f.WindowState = FormWindowState.Normal;
                    }
                    return f;
                }
            }

            return form;
        }

        /// <summary>
        /// Gets the number of instances of a Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetControlCount<T>(this Control parent)
        {
            int count = 0;

            foreach (Control c in parent.Controls)
            {
                if (c is T) count++;
            }

            return count;
        }

        /// <summary>
        /// Created by 9OPARA7. Retrieves all the controls from a parent, including nested controls.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <returns>List of controls.</returns>
        public static List<Control> GetControls(this Control parent, bool includeNested)
        {
            List<Control> list = new List<Control>();
            foreach (Control ctrl in parent.Controls)
            {
                list.Add(ctrl);
                if (includeNested)
                {
                    list.AddRange(GetControls(ctrl, true));
                }
            }
            return list;
        }

        public static List<Control> GetControls(this Control parent, bool includeNested, List<Type> controlTypes)
        {
            return parent.GetControls(includeNested, controlTypes, new List<string>());
        }

        /// <summary>
        /// Created by 9OPARA7. Retrieves all the controls from a parent, including nested controls.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="controlNames">List of fully-qualified control names to include.</param>
        /// <returns>List of controls.</returns>
        public static List<Control> GetControls(this Control parent, bool includeNested, List<string> controlNames)
        {
            string ctrlName;
            List<Control> list = new List<Control>();

            foreach (Control ctrl in parent.Controls)
            {
                ctrlName = ctrl.GetFullyQualifiedName();

                // The control must be one of the allowed types, and one of the included controls.
                if (!string.IsNullOrEmpty(ctrlName) && (controlNames.Count == 0 || controlNames.Contains(ctrlName)))
                {
                    list.Add(ctrl);
                }
                if (includeNested)
                {
                    list.AddRange(ctrl.GetControls(true, controlNames));
                }
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Retrieves all the controls of particular types from a parent, including nested controls.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="types">List of acceptable control types. They have to be exact types or direct descendants.</param>
        /// <param name="controlNames">List of fully-qualified control names to include.</param>
        /// <returns>List of controls.</returns>
        public static List<Control> GetControls(this Control parent, bool includeNested, List<Type> types, List<string> controlNames)
        {
            Type type;
            string ctrlName;
            List<Control> list = new List<Control>();

            foreach (Control ctrl in parent.Controls)
            {
                type = ctrl.GetType();
                ctrlName = ctrl.GetFullyQualifiedName();

                // The control must be one of the allowed types, and one of the included controls.
                if ((types.Count == 0 || types.Contains(type) || types.Contains(type.BaseType)) && ((controlNames.Count == 0 || controlNames.Contains(ctrlName))) && !string.IsNullOrEmpty(ctrlName))
                {
                    list.Add(ctrl);
                }
                if (includeNested)
                {
                    list.AddRange(ctrl.GetControls(true, types, controlNames));
                }
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Gets a list of all the control types inside a given Control.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <returns></returns>
        public static List<Type> GetControlTypes(this Control parent, bool includeNested)
        {
            Type type;
            List<Type> list = new List<Type>();

            foreach (Control ctrl in parent.Controls)
            {
                type = ctrl.GetType();

                if (!list.Contains(type))
                {
                    list.Add(type);
                    if (includeNested)
                    {
                        list.AddRange(ctrl.GetControlTypes(true));
                    }
                }
            }
            return list;
        }

        public static string GetFullyQualifiedName(this ToolStripItem toolStripItem)
        {
            string name = toolStripItem.Name;
            string parentName;

            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var parent = toolStripItem.GetCurrentParent();
            if (parent != null)
            {
                parentName = parent.GetFullyQualifiedName();
                if (!string.IsNullOrEmpty(parentName))
                {
                    name = string.Format("{0}.{1}", parentName, name);
                }
            }
            return name;
        }

        /// <summary>
        /// Created by 9OPARA7. Constructs the fully-qualified name of a control, in the "parent.child" format.
        /// This ensures that the name as we use it, is unique.
        /// </summary>
        /// <param name="control">Control whose name is to be built.</param>
        /// <returns>A fully-qualified name.</returns>
        public static string GetFullyQualifiedName(this Control control)
        {
            string name = control.Name;
            string parentName;

            if (string.IsNullOrEmpty(name))
            {
                //name = "empty";
                return string.Empty;
            }

            if (control.Parent != null)
            {
                parentName = control.Parent.GetFullyQualifiedName();
                if (!string.IsNullOrEmpty(parentName))
                {
                    name = string.Format("{0}.{1}", parentName, name);
                }
            }
            return name;
        }

        /// <summary>
        /// Sends a mouse event to a control.
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="pt"></param>
        /// <param name="mouseEvent"></param>
        public static void SendMouseClick(this Control ctrl, Point pt, Win32API.User32.MouseEvents mouseEvent)
        {
            Win32API.User32.MouseEvent(mouseEvent.ToNumber<long>(), pt.X, pt.Y, 0, 0);
        }

        /// <summary>
        /// Sends a left-click to the center of a control.
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="pt"></param>
        public static void SendMouseClick(this Control ctrl)
        {
            int x = (ctrl.Location.X + ctrl.Width) / 2;
            int y = ctrl.Location.Y + ctrl.Height / 2;
            Point pt = ctrl.PointToScreen(new Point(x, y));
            Win32API.User32.MouseEvent((int)Win32API.User32.MouseEvents.LeftClick, pt.X, pt.Y, 0, 0);
        }

        public static void CenterInOwner(this Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(form.Owner.Location.X + (form.Owner.Width - form.Width) / 2, form.Owner.Location.Y + (form.Owner.Height - form.Height) / 2);
        }

        /// <summary>
        /// Created by 9OPARA7. Centers this control in its parent.
        /// </summary>
        /// <param name="ctrl"></param>
        public static void CenterInParent(this Control ctrl)
        {
            ctrl.CenterInParentHorizontal();
            ctrl.CenterInParentVertical();
        }

        /// <summary>
        /// Created by 9OPARA7. Horizontally centers this control in its parent.
        /// </summary>
        /// <param name="ctrl"></param>
        public static void CenterInParentHorizontal(this Control ctrl)
        {
            if (ctrl.Parent != null)
            {
                ctrl.Left = (ctrl.Parent.Width / 2) - (ctrl.Width / 2);
            }
        }

        /// <summary>
        /// Created by 9OPARA7. Vertically centers this control in its parent.
        /// </summary>
        /// <param name="ctrl"></param>
        public static void CenterInParentVertical(this Control ctrl)
        {
            if (ctrl.Parent != null)
            {
                ctrl.Top = (ctrl.Parent.Height / 2) - (ctrl.Height / 2);
            }
        }

        /// <summary>
        /// Created by 9OPARA7. Determines the default property of a Control, and returns the value.
        /// </summary>
        /// <param name="ctrl">Control whose value will be retrieved.</param>
        /// <returns>The default value of the control.</returns>
        public static object GetValue(this Control ctrl)
        {
            PropertyInfo propInfo = null;
            Type type = ctrl.GetType();
            string valuePropertyName;
            string ctrlName = ctrl.GetFullyQualifiedName();

            // ListBoxes are handled in a special way			
            if (type == typeof(ListBox) || type.IsSubclassOf(typeof(ListBox)))
            {
                if (type == typeof(ListBox))
                {
                    ListBox lst = (ListBox)ctrl;
                    List<int> selectedIndices = new List<int>();
                    foreach (int index in lst.SelectedIndices)
                    {
                        selectedIndices.Add(index);
                    }
                    return selectedIndices;
                }

                // CheckedListBoxes are handled in a special way
                if (type == typeof(CheckedListBox) || type.IsSubclassOf(typeof(CheckedListBox)))
                {
                    CheckedListBox clst = (CheckedListBox)ctrl;
                    List<int> checkedIndices = new List<int>();
                    foreach (int index in clst.CheckedIndices)
                    {
                        checkedIndices.Add(index);
                    }
                    return checkedIndices;
                }
            }

            // ListViews are handled in a special way (selected ListViewItems).
            // TODO: Account for Checked Items?
            if (type == typeof(ListView) || type.IsSubclassOf(typeof(ListView)))
            {
                List<int> selectedIndices = new List<int>();
                foreach (int index in ((ListView)ctrl).SelectedIndices)
                {
                    selectedIndices.Add(index);
                }
                return selectedIndices;
            }

            // Handle TreeViews in a special way (selected node or checked nodes)
            if (type == typeof(TreeView) || type.IsSubclassOf(typeof(TreeView)))
            {
                List<string> checkedNodeFullPaths = new List<string>();
                TreeView tvw = (TreeView)ctrl;
                if (tvw.CheckBoxes)							// If checkboxes are on, set the checked nodes
                {
                    foreach (TreeNode node in tvw.GetCheckedNodes())
                    {
                        checkedNodeFullPaths.Add(node.FullPath);
                    }
                    return checkedNodeFullPaths;
                }
                else                                        // If checkboxes are off, set the selected node.
                {
                    if (tvw.SelectedNode != null)
                    {
                        return tvw.SelectedNode.FullPath;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            // If the value setting wasn't done by a special way, then do it the "regular" way.
            // Store the fully qualified name, along with the appropriate property.
            valuePropertyName = ctrl.GetValuePropertyName();
            if (string.IsNullOrEmpty(valuePropertyName))
            {
                return null;
            }
            else
            {
                //propInfo = type.GetProperty(valuePropertyName);
                propInfo = type.GetValuePropertyInfo(valuePropertyName);
                if (propInfo != null)
                {
                    return propInfo.GetValue(ctrl, null);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Created by 9OPARA7. Loop through all child controls and add the user-entered settings, based on the control's type.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="collection">Collection containing keys.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static void SetValues(this Control parent, Dictionary<string, object> collection, bool includeNested)
        {
            SetValues(parent, collection, includeNested, new List<Type>(), new List<string>());
        }

        /// <summary>
        /// Created by 9OPARA7. Loop through specified child controls and sets their values.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="collection">Collection containing keys.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="controlNames">List of fully-qualified control names to include.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static void SetValues(this Control parent, Dictionary<string, object> collection, bool includeNested, List<string> controlNames)
        {
            SetValues(parent, collection, includeNested, new List<Type>(), controlNames);
        }

        /// <summary>
        /// Created by 9OPARA7. Loop through specified child controls, of the specified types, and sets their values.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="collection">Collection containing keys.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="types">List of acceptable control types.</param>
        /// <param name="controlNames">List of fully-qualified control names to include.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static void SetValues(this Control parent, Dictionary<string, object> collection, bool includeNested, List<Type> types, List<string> controlNames)
        {
            Control ctrl;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            List<Control> controls = parent.GetControls(includeNested, types, controlNames);

            foreach (KeyValuePair<string, object> pair in collection)
            {
                // Find the control that matches the setting
                var query = from control in controls
                            where string.Compare(pair.Key, control.GetFullyQualifiedName(), true) == 0
                            select control;

                // If the control was found, assign the value to it.
                if (query.Count() > 0)
                {
                    ctrl = query.First();
                    try
                    {
                        if (pair.Value != null)
                        {
                            ctrl.SetValue(pair.Value);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Created by 9OPARA7. Determines the default property of a Control, and sets the value.
        /// </summary>
        /// <param name="ctrl">Control whose value will be set.</param>
        /// <param name="value">Value to assign to the control.</param>
        public static void SetValue(this Control ctrl, object value)
        {
            bool handled = false;
            PropertyInfo propInfo;
            Type type = ctrl.GetType();
            string valuePropertyName = ctrl.GetValuePropertyName();

            if (string.IsNullOrEmpty(valuePropertyName))
            {
                return;
            }
            propInfo = type.GetValuePropertyInfo(valuePropertyName);
            //PropertyInfo propInfo = type.GetProperty(ctrl.GetValuePropertyName());

            // ListBoxes are handled in a special way			
            if (type == typeof(ListBox) || type.IsSubclassOf(typeof(ListBox)))
            {
                if (type == typeof(ListBox))
                {
                    ListBox lst = (ListBox)ctrl;
                    List<int> selectedIndices = (List<int>)value;
                    if (lst.Items.Count > 0)
                    {
                        foreach (int index in selectedIndices)
                        {
                            if (!(index < lst.Items.Count))
                                continue;
                            try
                            {
                                lst.SetSelected(index, true);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                // CheckedListBoxes are handled in a special way
                if (type == typeof(CheckedListBox) || type.IsSubclassOf(typeof(CheckedListBox)))
                {
                    CheckedListBox clst = (CheckedListBox)ctrl;
                    List<int> checkedIndices = (List<int>)value;
                    if (clst.Items.Count > 0)
                    {
                        foreach (int index in checkedIndices)
                        {
                            if (!(index < clst.Items.Count))
                                continue;
                            try
                            {
                                clst.SetItemChecked(index, true);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                // We've handled it
                handled = true;
            }

            // ListViews are handled in a special way.
            // TODO: Account for Checked Items?
            if (type == typeof(ListView) || type.IsSubclassOf(typeof(ListView)) && value != null)
            {
                ListView lvw = (ListView)ctrl;
                List<int> selectedIndices = (List<int>)value;
                foreach (int index in selectedIndices)
                {
                    try
                    {
                        lvw.Items[index].Selected = true;
                    }
                    catch
                    {
                    }
                }

                // We've handled it
                handled = true;
            }

            // TreeViews are handled in a special way (selected node or checked nodes).
            if (type == typeof(TreeView) || type.IsSubclassOf(typeof(TreeView)) && value != null)
            {
                TreeView tvw = (TreeView)ctrl;
                List<string> fullPaths;
                List<TreeNode> checkedNodes;
                if (tvw.CheckBoxes)				// If checkboxes are on, set the checked nodes
                {
                    checkedNodes = new List<TreeNode>();
                    fullPaths = (List<string>)value;
                    checkedNodes.AddRange(tvw.SetCheckedNodes(fullPaths));

                    // Ensure that all the checked nodes are visible
                    foreach (TreeNode tvn in checkedNodes)
                    {
                        tvn.EnsureVisible();
                    }
                }
                else                            // If checkboxes are off, set the selected node.
                {
                    tvw.SelectedNode = tvw.GetNode(value.ToString(), tvw.Nodes);
                    if (tvw.SelectedNode != null)
                    {
                        tvw.SelectedNode.EnsureVisible();
                    }
                }

                // We've handled it
                handled = true;
            }

            // If the value setting wasn't done by a special way, then do it the "regular" way.
            if (!handled && propInfo != null)
            {
                propInfo.SetValue(ctrl, value, null);
            }
        }

        /// <summary>
        /// Remove all items belonging to the given group.
        /// </summary>
        /// <param name="lvw"></param>
        /// <param name="lvwGroup"></param>
        public static void Clear(this ListView lvw, ListViewGroup lvwGroup)
        {
            int index = 0;
            if (lvw.Items.Count > 0)
            {
                do
                {
                    ListViewItem lvi = lvw.Items[index];
                    if (lvi.Group == lvwGroup)
                    {
                        lvw.Items.Remove(lvi);
                    }
                    else
                    {
                        index++;
                    }
                } while (index < lvw.Items.Count);
            }
        }

        /// <summary>
        /// Created by 9OPARA7. Gets the name of the default property of a Control, based on its type.
        /// This method works for the major controls. All other controls (ListView, TreeView, etc) can be handled specially,
        /// in the Control.GetValue and Control.SetValue extension methods.
        /// </summary>
        /// <remarks>
        /// For the TreeView, when the Checkboxes property is set, then the value returned should be a collection of
        /// full paths to each checked TreeNode. Otherwise, the value is simply the fullpath of the selected TreeNode.
        /// </remarks>
        /// <param name="parent">Control to query.</param>
        /// <returns>A property name.</returns>
        public static string GetValuePropertyName(this Control parent)
        {
            Type type = parent.GetType();

            //if (type == typeof(CollapsiblePanel) || type.IsSubclassOf(typeof(CollapsiblePanel)))
            //{
            //    return "IsExpanded";
            //}
            if (string.Compare(type.Name, "CollapsiblePanel", true) == 0)		// Use the name because the raw Type is not available here
            {
                return "IsExpanded";
            }

            if (type == typeof(Form) || type.IsSubclassOf(typeof(Form)))
            {
                return "Bounds";
            }

            if (type == typeof(TextBox) || type.IsSubclassOf(typeof(TextBox)))
            {
                return "Text";
            }

            if (type == typeof(ComboBox) || type.IsSubclassOf(typeof(ComboBox)))
            {
                ComboBox cb = parent as ComboBox;
                if (cb.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    return "SelectedValue";
                }
                else
                {
                    return "Text";
                }
            }

            if (type == typeof(ListView) || type.IsSubclassOf(typeof(ListView)))
            {
                return "(Selected Items)";
            }

            if (type == typeof(TreeView) || type.IsSubclassOf(typeof(TreeView)))
            {
                TreeView tvw = (TreeView)parent;
                if (tvw.CheckBoxes)
                {
                    return "(Checked Nodes)";
                }
                else
                {
                    return "SelectedNode";
                }
            }

            if (type == typeof(ListBox) || type.IsSubclassOf(typeof(ListBox)))
            {
                if (type == typeof(ListBox))
                {
                    return "(Selected Items)";
                }

                if (type == typeof(CheckedListBox) || type.IsSubclassOf(typeof(CheckedListBox)))
                {
                    return "(Checked Items)";
                }
            }

            if (type == typeof(CheckBox) || type.IsSubclassOf(typeof(CheckBox)))
            {
                return "Checked";
            }

            if (type == typeof(RadioButton) || type.IsSubclassOf(typeof(RadioButton)))
            {
                return "Checked";
            }

            if (type == typeof(DateTimePicker) || type.IsSubclassOf(typeof(DateTimePicker)))
            {
                return "Value";
            }

            if (type == typeof(MaskedTextBox) || type.IsSubclassOf(typeof(MaskedTextBox)))
            {
                return "Text";
            }

            if (type == typeof(MonthCalendar) || type.IsSubclassOf(typeof(MonthCalendar)))
            {
                return "SelectionRange";
            }

            if (type == typeof(NumericUpDown) || type.IsSubclassOf(typeof(NumericUpDown)))
            {
                return "Value";
            }

            if (type == typeof(PictureBox) || type.IsSubclassOf(typeof(PictureBox)))
            {
                return "ImageLocation";
            }

            if (type == typeof(RichTextBox) || type.IsSubclassOf(typeof(RichTextBox)))
            {
                return "Text";
            }

            if (type == typeof(WebBrowser) || type.IsSubclassOf(typeof(WebBrowser)))
            {
                return "Uri";
            }

            if (type == typeof(SplitContainer) || type.IsSubclassOf(typeof(SplitContainer)))
            {
                return "SplitterDistance";
            }

            if (type == typeof(TabControl) || type.IsSubclassOf(typeof(TabControl)))
            {
                return "SelectedIndex";
            }

            return string.Empty;
        }

        /// <summary>
        /// Created by 9OPARA7. Loop through all child controls and gets current data.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static Dictionary<string, object> GetValues(this Control parent, bool includeNested)
        {
            return GetValues(parent, includeNested, new List<string>(), new List<Type>());
        }

        /// <summary>
        /// Created by 9OPARA7. Loops through specified child controls and gets their data.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="controlNames">Names of controls to include.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static Dictionary<string, object> GetValues(this Control parent, bool includeNested, List<string> controlNames)
        {
            return GetValues(parent, includeNested, controlNames, new List<Type>());
        }

        /// <summary>
        /// Created by 9OPARA7. Loops through specified child controls, of the specified type, and gets current data.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="includeNested">If true, the search includes nested controls.</param>
        /// <param name="types">List of acceptable types.</param>
        /// <param name="controlNames">Names of controls to include.</param>
        /// <returns>Dictionary of control values, keyed by the controls' name.</returns>
        public static Dictionary<string, object> GetValues(this Control parent, bool includeNested, List<string> controlNames, List<Type> types)
        {
            Type type;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            List<Control> controls = parent.GetControls(includeNested, types, controlNames);
            string ctrlName;

            // TODO: Add support for multiple properties?
            // TODO: Add support for menu items (For menu items, we want only the Checked property)
            // TODO: Add support for toolstrip items (embedded)

            foreach (Control ctrl in controls)
            {
                type = ctrl.GetType();
                ctrlName = ctrl.GetFullyQualifiedName();

                // The control must be one of the allowed types, and one of the included controls.
                if ((types.Count == 0 || types.Contains(type)) && (controlNames.Contains(ctrlName)) && !dictionary.ContainsKey(ctrlName))
                {
                    dictionary.Add(ctrlName, ctrl.GetValue());
                }
            }

            return dictionary;
        }

        #region
        /// <summary>
        /// Checks All CheckedListBox items
        /// </summary>
        /// <param name="clst"></param>
        public static void CheckAllItems(this CheckedListBox clst)
        {
            for (int i = 0; i < clst.Items.Count; i++)
            {
                clst.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// Unchecks All CheckedListBox items
        /// </summary>
        /// <param name="?"></param>
        public static void UncheckAllItems(this CheckedListBox clst)
        {
            for (int i = 0; i < clst.Items.Count; i++)
            {
                clst.SetItemChecked(i, false);
            }
        }
        #endregion        

        #region Extension methods for the TreeView
        /// <summary>
        /// Created by GGEORGIEV.Checks if there are selected nodes
        /// </summary>
        /// <param name="treeView">Parent TreeView.</param>
        /// <returns>TRUE/FALSE</returns>
        public static bool IsAnyNodeChecked(this TreeView treeView)
        {
            bool returnValue = false;
            foreach (TreeNode node in treeView.Nodes)
            {
                if (node.Checked)
                {
                    returnValue = true;
                    break;
                }
                returnValue = node.IsAnyNodeChecked();
                if (returnValue)
                {
                    break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Checks if there are checked nodes
        /// </summary>
        /// <param name="parent">Parent TreeNode.</param>
        /// <returns>TRUE/FALSE</returns>
        public static bool IsAnyNodeChecked(this TreeNode parent)
        {
            bool returnValue = false;
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Checked)
                {
                    returnValue = true;
                    break;
                }

                returnValue = node.IsAnyNodeChecked();
                if (returnValue)
                {
                    break;
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Created by 9OPARA7. Selects a node, given a path.
        /// </summary>
        /// <param name="tvw">Target TreeView.</param>
        /// <param name="path">FullPath of node to select.</param>
        /// <param name="nodes">A collection of TreeNodes.</param>
        /// <returns>Node matching given FullPath.</returns>
        public static TreeNode GetNode(this TreeView tvw, string path, TreeNodeCollection nodes)
        {
            string[] parts = path.Split('\\');

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Text == parts[0])
                {
                    if (nodes[i].Nodes.Count == 0 || parts.Length == 1)
                        return nodes[i];

                    return GetNode(tvw, path.Remove(0, parts[0].Length + 1), nodes[i].Nodes);
                }
            }
            return null;
        }

        public static List<TreeNode> GetParentNodes(this TreeNode tvn)
        {
            List<TreeNode> list = new List<TreeNode>();

            if (tvn.Parent != null)
            {
                list.Add(tvn.Parent);
                list.AddRange(tvn.Parent.GetParentNodes());
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Gets all the checked nodes from a TreeView. It does a deep search.
        /// </summary>
        /// <param name="treeView">Parent TreeView.</param>
        /// <returns>List of checked TreeNodes.</returns>
        public static List<TreeNode> GetCheckedNodes(this TreeView treeView)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in treeView.Nodes)
            {
                if (node.Checked)
                {
                    list.Add(node);
                }
                list.AddRange(node.GetCheckedNodes());
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Gets all the [flattened] checked nodes under a TreeNode.
        /// </summary>
        /// <param name="parent">Parent Node.</param>
        /// <returns>List of checked TreeNodes.</returns>
        public static List<TreeNode> GetCheckedNodes(this TreeNode parent)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Checked)
                {
                    list.Add(node);
                }
                list.AddRange(node.GetCheckedNodes());
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Checks all the child nodes that match the given paths.
        /// </summary>
        /// <param name="tvw">Parent TreeView.</param>
        /// <param name="fullPaths">List of full paths to the nodes to be checked.</param>
        /// <returns>List of checked TreeNodes.</returns>
        public static List<TreeNode> SetCheckedNodes(this TreeView tvw, List<string> fullPaths, bool autoExpand = true)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in tvw.Nodes)
            {
                if (fullPaths.Contains(node.FullPath))
                {
                    node.Checked = true;
                    list.Add(node);
                    if (autoExpand)
                    {
                        node.EnsureVisible();
                    }
                }
                list.AddRange(node.SetCheckedNodes(fullPaths));
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Checks all the child nodes that match the given paths.
        /// </summary>
        /// <param name="parent">Parent Node.</param>
        /// <param name="fullPaths">List of full paths to the nodes to be checked.</param>
        /// <returns>List of checked TreeNodes.</returns>
        public static List<TreeNode> SetCheckedNodes(this TreeNode parent, List<string> fullPaths)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in parent.Nodes)
            {
                if (fullPaths.Contains(node.FullPath))
                {
                    node.Checked = true;
                    list.Add(node);
                }
                list.AddRange(node.SetCheckedNodes(fullPaths));
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Gets a list of all the leaf nodes.
        /// </summary>
        /// <param name="tvw">Parent TreeView.</param>
        /// <returns>List of TreeNodes.</returns>
        public static List<TreeNode> GetLeafNodes(this TreeView tvw)
        {
            return tvw.GetNodes().Where(x => x.Nodes.Count == 0).ToList();
        }

        /// <summary>
        /// Created by 9OPARA7. Gets a flattened list of all the nodes.
        /// </summary>
        /// <param name="tvw">Parent TreeView.</param>
        /// <returns>List of TreeNodes.</returns>
        public static List<TreeNode> GetNodes(this TreeView tvw)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in tvw.Nodes)
            {
                list.Add(node);
                list.AddRange(node.GetNodes());
            }
            return list;
        }

        /// <summary>
        /// Created by 9OPARA7. Gets a flattened list of all the nodes under a TreeNode.
        /// </summary>
        /// <param name="parent">Parent Node.</param>
        /// <returns>List of TreeNodes.</returns>
        public static List<TreeNode> GetNodes(this TreeNode parent)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode node in parent.Nodes)
            {
                list.Add(node);
                list.AddRange(node.GetNodes());
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Created by 9OPARA7. Gets if the control is in design mode, or if any of its parents are in design mode.
        /// </summary>
        /// <remarks>It took me a while to find a system that works. DO NOT MESS WITH THIS METHOD.</remarks>
        public static bool IsDesignerHosted(this Control ctrl)
        {
            if (ctrl != null)
            {
                if (ctrl.Site != null)
                {
                    if (ctrl.Site.DesignMode == true)
                    {
                        return true;
                    }
                    else
                    {
                        return IsDesignerHosted(ctrl.Parent);
                    }
                }
                else
                {
                    return IsDesignerHosted(ctrl.Parent);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
