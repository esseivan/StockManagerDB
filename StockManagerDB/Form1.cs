﻿using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace StockManagerDB
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// filepath to the database
        /// </summary>
        private string filepath = string.Empty;

        /// <summary>
        /// Manage the database
        /// </summary>
        private DBWrapper dbw
        {
            get => _dbw;
            set
            {
                _dbw = value;
                _dbw.OnListModified += Dbw_OnListModified;
            }
        }
        private DBWrapper _dbw = null;

        /// <summary>
        /// The listview where the parts are displayed
        /// </summary>
        private readonly FastDataListView partLV;

        /// <summary>
        /// List of parts in the database
        /// </summary>
        public List<PartClass> parts => dbw.PartsList;

        /// <summary>
        /// Indicate if a DB is open
        /// </summary>
        private bool IsDBOpen => (null != dbw);

        /// <summary>
        /// Update CheckListView when checkItemChanged
        /// </summary>
        private bool UpdateOnCheck = true;

        public Form1()
        {
            InitializeComponent();
            LoggerClass.Init();

            partLV = listviewParts;
            InitListView(partLV);
            InitListView(listviewChecked);

            cbboxFilterType.SelectedIndex = 2;

            LoggerClass.Write("Idle");
        }

        #region Display

        private void UpdateDisplay()
        {
            partLV.DataSource = parts;
            partLV.AutoResizeColumns();
            partLV.Focus();
        }

        private void Dbw_OnListModified(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        #endregion

        #region ListView management

        private void InitListView(FastDataListView listview)
        {
            listview.View = View.Details;
            listview.GridLines = true;
            listview.FullRowSelect = true;
            listview.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            listview.CellEditUseWholeCell = true;

            // Setup columns
            olvcMPN.AspectGetter = delegate (object x) { return ((PartClass)x).MPN; };
            olvcMAN.AspectGetter = delegate (object x) { return ((PartClass)x).Manufacturer; };
            olvcDesc.AspectGetter = delegate (object x) { return ((PartClass)x).Description; };
            olvcCat.AspectGetter = delegate (object x) { return ((PartClass)x).Category; };
            olvcLocation.AspectGetter = delegate (object x) { return ((PartClass)x).Location; };
            olvcStock.AspectGetter = delegate (object x) { return ((PartClass)x).Stock; };
            olvcLowStock.AspectGetter = delegate (object x) { return ((PartClass)x).LowStock; };
            olvcPrice.AspectGetter = delegate (object x) { return ((PartClass)x).Price; };
            olvcSupplier.AspectGetter = delegate (object x) { return ((PartClass)x).Supplier; };
            olvcSPN.AspectGetter = delegate (object x) { return ((PartClass)x).SPN; };

            olvcMPN2.AspectGetter = delegate (object x) { return ((PartClass)x).MPN; };
            olvcMAN2.AspectGetter = delegate (object x) { return ((PartClass)x).Manufacturer; };
            olvcDesc2.AspectGetter = delegate (object x) { return ((PartClass)x).Description; };
            olvcCat2.AspectGetter = delegate (object x) { return ((PartClass)x).Category; };
            olvcLocation2.AspectGetter = delegate (object x) { return ((PartClass)x).Location; };
            olvcStock2.AspectGetter = delegate (object x) { return ((PartClass)x).Stock; };
            olvcLowStock2.AspectGetter = delegate (object x) { return ((PartClass)x).LowStock; };
            olvcPrice2.AspectGetter = delegate (object x) { return ((PartClass)x).Price; };
            olvcSupplier2.AspectGetter = delegate (object x) { return ((PartClass)x).Supplier; };
            olvcSPN2.AspectGetter = delegate (object x) { return ((PartClass)x).SPN; };
        }

        private List<PartClass> GetCheckedParts()
        {
            return partLV.CheckedObjectsEnumerable.Cast<PartClass>().ToList();
        }

        private List<PartClass> GetAll()
        {
            return parts;
        }

        /// <summary>
        /// Return parts that will be validation for the action
        /// </summary>
        /// <returns></returns>
        private List<PartClass> GetPartForProcess()
        {
            if (onlyAffectCheckedPartsToolStripMenuItem.Checked)
                return GetCheckedParts();
            return GetAll();
        }

        #endregion

        /// <summary>
        /// Import parts from excel file into the database
        /// </summary>
        private void ImportFromExcel()
        {
            if (!IsDBOpen)
            {
                MessageBox.Show("No database selected !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "All files|*.*",
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ExcelManager em = new ExcelManager(ofd.FileName);
                List<PartClass> p = em.GetParts();

                if ((null == p) || (0 == p.Count))
                {
                    LoggerClass.Write("No part found");
                    return;
                }

                if (MessageBox.Show($"Please confirm the additions of '{p.Count}' parts to the current databse. This cannot be undone\nContinue ?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                // Add all
                dbw.AddPartRange(p);
            }
        }

        private void importFromExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFromExcel();
        }

        /// <summary>
        /// Create a new empty database
        /// </summary>
        private void CreateNewDatabase()
        {
            SaveFileDialog fsd = new SaveFileDialog()
            {
                Filter = "sqlite|*.sqlite|All files|*.*",
            };
            if (fsd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(fsd.FileName))
                {
                    MessageBox.Show("This file already exists. Please select another one...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                filepath = fsd.FileName;
                dbw = new DBWrapper(filepath);
                dbw.CreateDatabase(true);
            }
        }

        private void newDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewDatabase();
        }

        /// <summary>
        /// Open the specified database
        /// </summary>
        private void OpenDatabase()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "sqlite|*.sqlite|All files|*.*",
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                dbw = new DBWrapper(filepath);
                dbw.LoadDatabase();
            }
        }

        private void openDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDatabase();
        }

        /// <summary>
        /// Filter a text in the part list
        /// </summary>
        /// <param name="txt">Text to filter</param>
        /// <param name="matchKind">Type of filter</param>
        public void Filter(string txt, int matchKind)
        {
            ObjectListView olv = partLV;
            TextMatchFilter filter = null;
            if (!string.IsNullOrEmpty(txt))
            {
                switch (matchKind)
                {
                    case 0:
                    default:
                        filter = TextMatchFilter.Contains(olv, txt);
                        break;
                    case 1:
                        filter = TextMatchFilter.Prefix(olv, txt);
                        break;
                    case 2:
                        filter = TextMatchFilter.Regex(olv, txt);
                        break;
                }
            }

            // Text highlighting requires at least a default renderer
            if (olv.DefaultRenderer == null)
                olv.DefaultRenderer = new HighlightTextRenderer(filter);

            olv.AdditionalFilter = filter;
        }

        private void txtboxFilter_TextChanged(object sender, EventArgs e)
        {
            Filter(((TextBox)sender).Text, this.cbboxFilterType.SelectedIndex);
        }

        private void cbboxFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter(txtboxFilter.Text, this.cbboxFilterType.SelectedIndex);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LoggerClass.Write("Closing... Stopping logger", "Info");
            LoggerClass.logger.Dispose();
        }

        /// <summary>
        /// Delete the checked parts
        /// </summary>
        private void DeleteCheckedParts()
        {
            var checkedParts = GetCheckedParts();

            if (MessageBox.Show($"Please confirm the deletion of '{checkedParts.Count}' parts. This cannot be undone !\nContinue ?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            dbw.RemovePartRange(checkedParts.Select((part) => part.MPN));
        }

        #region Actions

        private void MakeOrder()
        {
            List<PartClass> parts = GetPartForProcess();
            // Select parts with current stock lower than lowStock limit
            var selected = parts.Where((part) => (part.Stock < part.LowStock));

            Console.WriteLine(selected.Count() + " parts to order");
        }

        #endregion

        private void makeOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeOrder();
        }

        private void UpdateCheckedListview()
        {
            if (!UpdateOnCheck)
                return;

            List<PartClass> checkedParts = GetCheckedParts();
            listviewChecked.DataSource = checkedParts;
        }

        private void listviewParts_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateCheckedListview();
        }

        private void checkAllInViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateOnCheck = false;
            //partLV.CheckAll();
            partLV.CheckObjects(partLV.FilteredObjects);
            partLV.Focus();
            UpdateOnCheck = true;
            UpdateCheckedListview();
        }

        private void uncheckAllInViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partLV.CheckObjects(partLV.FilteredObjects);

            UpdateOnCheck = false;
            //partLV.UncheckAll();
            partLV.UncheckObjects(partLV.FilteredObjects);
            partLV.Focus();
            UpdateOnCheck = true;
            UpdateCheckedListview();
        }

        private void DELETECheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCheckedParts();
        }

        private void listviewParts_CellEditFinished(object sender, CellEditEventArgs e)
        {
            PartClass part = e.RowObject as PartClass;
            PartClass.Parameter editedParameter = (PartClass.Parameter)(e.Column.Index);
            string newValue = e.NewValue.ToString();
            if (editedParameter == PartClass.Parameter.UNDEFINED)
            {
                throw new InvalidOperationException("Unable to edit 'undefined'");
            }
            else if (editedParameter == PartClass.Parameter.MPN)
            {
                dbw.RenamePart(part.MPN, newValue);
            }
            else
            {
                // Apply manually the new value
                part.Parameters[editedParameter] = newValue;
                dbw.UpdatePart(part);
            }
        }
    }
}
