﻿using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StockManagerDB.frmProjects;

namespace StockManagerDB
{
    public partial class frmOrder : Form
    {
        private readonly Dictionary<string, Material> PartsToOrder =
            new Dictionary<string, Material>();

        private bool InfosVisible
        {
            get => AppSettings.Settings.Order_ShowInfos;
            set => AppSettings.Settings.Order_ShowInfos = value;
        }
        private bool MoreInfosVisible
        {
            get => AppSettings.Settings.Order_ShowMoreInfos;
            set => AppSettings.Settings.Order_ShowMoreInfos = value;
        }

        private bool init = true;

        public frmOrder()
        {
            InitializeComponent();
            label2.Visible = false;

            ApplySettings();
            ListViewSetColumns();

            init = false;
        }

        public void ApplySettings()
        {
            /**** Font ****/
            //Font newFontNormal = new Font(newFont, FontStyle.Regular);
            if (AppSettings.Settings.AppFont == null)
            {
                AppSettings.ResetToDefault();
            }
            Font newFontNormal = AppSettings.Settings.AppFont; // If user has set bold for all, then set bold for all
            this.Font = this.label1.Font = this.cbbSuppliers.Font = this.textBulkAdd.Font = newFontNormal;
            //this.menuStrip1.Font = this.statusStrip1.Font = newFontNormal;
            // Apply bold fonts
            Font newFontBold = new Font(
                AppSettings.Settings.AppFont,
                FontStyle.Bold | AppSettings.Settings.AppFont.Style
            ); // Add bold style
        }

        /// <summary>
        /// Initialisation of the listviews
        /// </summary>
        /// <param name="listview"></param>
        private void ListViewSetColumns()
        {
            // Setup columns
            olvcMPN.AspectGetter = delegate (object x)
            {
                return ((Material)x).MPN;
            };
            olvcQuantity.AspectGetter = delegate (object x)
            {
                return ((Material)x).Quantity;
            };
            olvcMAN.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Manufacturer;
            };
            olvcDesc.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Description;
            };
            olvcCat.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Category;
            };
            olvcLocation.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Location;
            };
            olvcStock.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Stock;
            };
            olvcLowStock.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.LowStock;
            };
            olvcPrice.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Price;
            };
            olvcSupplier.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Supplier;
            };
            olvcSPN.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.SPN;
            };
            olvcTotalPrice.AspectGetter = delegate (object x)
            {
                return ((Material)x).PartLink?.Price * ((Material)x).Quantity;
            };

            // Make the decoration
            RowBorderDecoration rbd = new RowBorderDecoration
            {
                BorderPen = new Pen(Color.FromArgb(128, Color.DeepSkyBlue), 2),
                BoundsPadding = new Size(1, 1),
                CornerRounding = 4.0f
            };

            // Put the decoration onto the hot item
            listviewMaterials.HotItemStyle = new HotItemStyle
            {
                BackColor = Color.Azure,
                Decoration = rbd
            };
        }

        private void UpdateBulkAddText()
        {
            if (cbbSuppliers.SelectedIndex == -1)
                return;

            IEnumerable<Material> filteredParts;

            if (cbbSuppliers.SelectedIndex == 0)
            {
                filteredParts = PartsToOrder.Values;
            }
            else
            {
                string supplier = cbbSuppliers.SelectedItem.ToString();

                filteredParts = PartsToOrder
                    .Where(
                        (x) =>
                            x.Value.PartLink?.Supplier.Equals(
                                supplier,
                                StringComparison.InvariantCultureIgnoreCase
                            ) ?? false
                    )
                    .Select((x) => x.Value);
            }

            string bulkText = string.Join(
                "\n",
                filteredParts.Select((m) => $"{m.QuantityStr}, {m.PartLink?.SPN ?? "Undefined"}")
            );

            textBulkAdd.Text = bulkText;
        }

        private void PartsHaveChanged()
        {
            Cursor = Cursors.WaitCursor;
            listviewMaterials.DataSource = PartsToOrder.Values.ToList();
            UpdateBulkAddText();
            Cursor = Cursors.Default;
        }

        public void SetSuppliers(IEnumerable<string> suppliers)
        {
            init = true;
            cbbSuppliers.Items.Clear();
            cbbSuppliers.Items.Add("All");
            cbbSuppliers.Items.AddRange(suppliers.ToArray());
            if (cbbSuppliers.Items.Count > 0)
                cbbSuppliers.SelectedIndex = 1;
            init = false;

            UpdateBulkAddText();
        }

        /// <summary>
        /// Add parts to order to the list according to lowstock and stock parameters
        /// </summary>
        /// <param name="parts"></param>
        public void AddPartsToOrder(IEnumerable<Part> parts)
        {
            foreach (Part part in parts)
            {
                float qty = part.LowStock - part.Stock;
                if (qty < 0)
                    continue; // No order to do for this part

                if (PartsToOrder.ContainsKey(part.MPN))
                {
                    // Add to existing
                    PartsToOrder[part.MPN].Quantity += qty;
                }
                else
                {
                    PartsToOrder[part.MPN] = new Material() { MPN = part.MPN, Quantity = qty, };
                }
            }
            PartsHaveChanged();
        }

        /// <summary>
        /// Add materials to order according to quantites
        /// </summary>
        /// <param name="parts"></param>
        public void AddPartsToOrder(IEnumerable<Material> materials)
        {
            foreach (Material mat in materials)
            {
                float qty = mat.Quantity;
                if (PartsToOrder.ContainsKey(mat.MPN))
                {
                    // Add to existing
                    PartsToOrder[mat.MPN].Quantity += qty;
                }
                else
                {
                    PartsToOrder[mat.MPN] = new Material() { MPN = mat.MPN, Quantity = qty, };
                }
            }
            PartsHaveChanged();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Ask confirmation
            if (MessageBox.Show("Confirm that you want to clear ALL this list.\nClear all ?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            PartsToOrder.Clear();
            PartsHaveChanged();
        }

        private void frmOrder_Load(object sender, EventArgs e)
        {
            olvcMAN.IsVisible = olvcLocation.IsVisible = olvcCat.IsVisible = InfosVisible;
            olvcDesc.IsVisible = olvcMPN.IsVisible = MoreInfosVisible;
            listviewMaterials.RebuildColumns();
        }

        /// <summary>
        /// Delete selected parts
        /// </summary>
        private void deleteSelection()
        {
            IEnumerable<Material> selected = listviewMaterials.SelectedObjects.Cast<Material>();

            foreach (Material item in selected)
            {
                if (PartsToOrder.ContainsKey(item.MPN))
                {
                    PartsToOrder.Remove(item.MPN);
                }
            }

            PartsHaveChanged();
        }

        private void deleteSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteSelection();
        }

        private void cbbSuppliers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (init)
            {
                return;
            }

            UpdateBulkAddText();
        }

        private void copyMPNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the selected part
            if (!(listviewMaterials.SelectedObject is Material mat))
            {
                return;
            }

            if (!mat.HasPartLink)
            {
                return;
            }

            mat.PartLink.CopyMPNToClipboard();
        }

        private void openSupplierUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the selected part
            if (!(listviewMaterials.SelectedObject is Material mat))
            {
                return;
            }

            if (!mat.HasPartLink)
            {
                return;
            }

            mat.PartLink.OpenSupplierUrl();
        }

        private void listviewMaterials_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            // When rightclicking a cell, copy the MPN of the corresponding row
            if (!(e.Model is Material))
            {
                return;
            }

            contextMenuStrip1.Show(Cursor.Position);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBulkAdd.Text))
            {
                return;
            }
            Clipboard.SetText(textBulkAdd.Text);
            label2.Visible = true;
            statusTimeoutTimer.Start();
        }

        private void statusTimeoutTimer_Tick(object sender, EventArgs e)
        {
            statusTimeoutTimer.Stop();
            label2.Visible = false;
        }


        /// <summary>
        /// Called when a cell is edited
        /// </summary>
        private void ApplyEdit(CellEditEventArgs e)
        {
            // Get the unedited part version
            Material item = e.RowObject as Material;
            // Get the edited parameter and value
            string newValue = e.NewValue.ToString();

            item.QuantityStr = newValue;

            PartsHaveChanged();
        }

        private void UpdateInfos()
        {
            InfosVisible = !InfosVisible;
            olvcMAN.IsVisible = olvcLocation.IsVisible = olvcCat.IsVisible = InfosVisible;
            listviewMaterials.RebuildColumns();
        }
        private void UpdateMoreInfos()
        {
            MoreInfosVisible = showInfosToolStripMenuItem.Checked;
            olvcDesc.IsVisible = olvcMPN.IsVisible = MoreInfosVisible;
            listviewMaterials.RebuildColumns();
        }

        private void listviewMaterials_CellEditFinished(object sender, CellEditEventArgs e)
        {
            ApplyEdit(e);
        }

        private void resizeColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listviewMaterials.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void showInfosToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInfos();
        }

        private void showMoreInfosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateMoreInfos();
        }

        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PartsHaveChanged();
        }
    }
}
