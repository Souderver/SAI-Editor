﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SAI_Editor.Classes;

namespace SAI_Editor.Forms.SearchForms
{
    public partial class MultiSelectForm<T> : Form where T : struct, IConvertible
    {
        private readonly ListViewColumnSorter lvwColumnSorter = new ListViewColumnSorter();
        private readonly TextBox textBoxToChange = null;

        public MultiSelectForm(TextBox textBoxToChange)
        {
            InitializeComponent();

            this.textBoxToChange = textBoxToChange;

            listViewSelectableItems.Columns.Add(typeof(T).Name, 235, HorizontalAlignment.Left);

            foreach (var en in Enum.GetNames(typeof(T)))
                listViewSelectableItems.Items.Add("").SubItems.Add(en);

            if (textBoxToChange != null)
            {
                long bitmask = XConverter.ToInt64(textBoxToChange.Text);
                bool anyFlag = false;

                foreach (ListViewItem item in listViewSelectableItems.Items)
                {
                    foreach (var en in Enum.GetNames(typeof(T)))
                    {
                        if (en.Equals(item.SubItems[1].Text))
                        {
                            object enu = Enum.Parse(typeof(T), en);

                            if ((bitmask & Convert.ToInt64(enu)) == Convert.ToInt64(enu))
                            {
                                anyFlag = true;
                                item.Checked = true;
                            }
                        }
                    }
                }

                if (!anyFlag)
                    foreach (ListViewItem item in listViewSelectableItems.Items.Cast<ListViewItem>().Where(item => item.Index > 0))
                        item.Checked = false;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            List<Enum> vals = Enum.GetValues(typeof(T)).OfType<Enum>().ToList();
            long mask = (from ListViewItem item in listViewSelectableItems.CheckedItems from en in Enum.GetNames(typeof(T)) where en.Equals(item.SubItems[1].Text) select Convert.ToInt64(Enum.Parse(typeof(T), en))).Sum();

            if (textBoxToChange != null)
            {
                textBoxToChange.Text = mask.ToString();
                Close();
            }
            else
            {
                DialogResult result = MessageBox.Show(String.Format("The bitmask of all selected items together is {0}. Do you wish to close the form?", mask), "Outcome", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    Close();
            }
        }

        private void MultiSelectForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void listViewSelectableItems_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            //! TODO: Fix this. It's also called when the form loads and for some reason this if-check passes...
            //if (listViewSelectableItems.Items[0].Checked)
            //{
            //    foreach (ListViewItem item in listViewSelectableItems.Items)
            //        if (item.Index > 0)
            //            item.Checked = false;
            //}
            //else
            {
                if (listViewSelectableItems.CheckedItems.Count <= 0)
                    listViewSelectableItems.Items[0].Checked = true;

                if (e.Item.Checked && e.Item.Index > 0)
                    listViewSelectableItems.Items[0].Checked = false;
            }
        }

        private void listViewSelectableItems_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var myListView = (ListView)sender;
            myListView.ListViewItemSorter = lvwColumnSorter;

            if (e.Column != lvwColumnSorter.SortColumn)
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
                lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            myListView.Sort();
        }
    }
}
