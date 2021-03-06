﻿using SequenceApp.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SequenceApp
{
    public partial class Form1 : Form, iForm1
    {
        bool iForm1.didConnect
        {
            set
            {
                if(value == false)
                {
                    MessageBox.Show("Failed to connect to: " + comComboBox.Text, "Error: Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else
                {
                    button1.Enabled = true;
                    connectButton.Text = "Disconnect";
                }
            }
        }
        // Bitmap FADE_IMAGE = new Bitmap(@"D:\Desktop\Active HW\Senior Project\Frontend Assets\sine_wave.png");                                          
        // Bitmap FLASH_IMAGE = new Bitmap(@"D:\Desktop\Active HW\Senior Project\Frontend Assets\square_wave.png");

        const int NUM_ROW_CELLS = 50;

        public Form1()
        {
            InitializeComponent();

            // initialize export button to be disabled
            button1.Enabled = false;

            // initialize Serial Initialize
            var ports = SerialPort.GetPortNames();
            comComboBox.DataSource = ports;

            // initialize rows/columns
            seqDataGridView.DefaultCellStyle.SelectionForeColor = Color.Gray;
            seqDataGridView.RowCount = 4;
            seqDataGridView.AllowDrop = true;
            var columns = new DataGridViewCustomColumn[NUM_ROW_CELLS];
            for (int i = 0; i < NUM_ROW_CELLS; i++)
            {
                columns[i]= new DataGridViewCustomColumn();
                columns[i].Width = seqDataGridView.Rows[0].Height + 2;
            }
            
            //basePictureBox.Image = new Bitmap(@"D:\Desktop\Active HW\Senior Project\square_wave.png");

            seqDataGridView.Columns.AddRange(columns);
            seqDataGridView.Columns.RemoveAt(0);
            //seqDataGridView.ColumnCount = 50;
            for (int r = 0; r < seqDataGridView.RowCount; r++)
            {
                for (int c = 0; c < seqDataGridView.ColumnCount; c++)
                {
                    //Bitmap image = FLASH_IMAGE;
                    //image.SetResolution(10, 10);
                    DataGridViewCustomCell cell = (DataGridViewCustomCell)seqDataGridView.Rows[r].Cells[c];
                    cell.mode = "Fade";
                    seqDataGridView.Rows[r].Cells[c].Style.BackColor = Color.Black;
                }
            }
            // initial formating
            seqDataGridView.RowHeadersWidth = seqDataGridView.RowHeadersWidth + 8;

            seqDataGridView.Rows[0].HeaderCell.Value = "R";
            seqDataGridView.Rows[1].HeaderCell.Value = "G";
            seqDataGridView.Rows[2].HeaderCell.Value = "B";
            seqDataGridView.Rows[3].HeaderCell.Value = "C";

            seqDataGridView.AllowUserToResizeColumns = false;
            seqDataGridView.AllowUserToResizeRows = false;
        }

        // helper methods

        private void recalculateColor(int row, int column)
        {
            /*
            if (row == 3)
            {
                seqDataGridView.Rows[0].Cells[column].Style.BackColor = Color.Black;
                seqDataGridView.Rows[1].Cells[column].Style.BackColor = Color.Black;
                seqDataGridView.Rows[2].Cells[column].Style.BackColor = Color.Black;
            }
            */
            if (row != 3)
            {
                int R, G, B;
                R = G = B = 0;
                R = seqDataGridView.Rows[0].Cells[column].Style.BackColor == Color.White ? 0 : seqDataGridView.Rows[0].Cells[column].Style.BackColor.R;
                G = seqDataGridView.Rows[1].Cells[column].Style.BackColor == Color.White ? 0 : seqDataGridView.Rows[1].Cells[column].Style.BackColor.G;
                B = seqDataGridView.Rows[2].Cells[column].Style.BackColor == Color.White ? 0 : seqDataGridView.Rows[2].Cells[column].Style.BackColor.B;
                seqDataGridView.Rows[3].Cells[column].Style.BackColor = Color.FromArgb(R, G, B);
            }
        }

        /*
        private void recalculateMode(CellModeEvaluation evaluation)
        {
            foreach (int col in evaluation.normalTouchedRows)
            {
                List<string> modes = new List<string> {((DataGridViewCustomCell)seqDataGridView.Rows[0].Cells[col]).mode,
                                                      ((DataGridViewCustomCell)seqDataGridView.Rows[1].Cells[col]).mode,
                                                      ((DataGridViewCustomCell)seqDataGridView.Rows[2].Cells[col]).mode };
                // only 1 mode represented in all 3 rows for that column
                if (modes.Distinct().ToList().Count == 1)
                {
                    DataGridViewCustomCell cell = ((DataGridViewCustomCell)seqDataGridView.Rows[3].Cells[col]);
                    cell.mode = modes[0];
                    cell.Value = modes[0] == "Fade" ? FADE_IMAGE :
                                                      FLASH_IMAGE;
                }
            }
            foreach (int col in evaluation.colorTouchedRows)
            {
                DataGridViewCustomCell colorCustomCell = (DataGridViewCustomCell)seqDataGridView.Rows[3].Cells[col];
                for(int i = 0; i < 3; i ++)
                {
                    DataGridViewCustomCell cell = ((DataGridViewCustomCell)seqDataGridView.Rows[i].Cells[col]);
                    cell.mode = colorCustomCell.mode;
                    cell.Value = cell.mode == "Fade" ? FADE_IMAGE :
                                                       FLASH_IMAGE;
                }
            }
        }
        */
        // UI Handlers

        public event Action<CellData[,]> ExportClicked;
        public event Action<string> ConnectClicked;
        public event Action<bool> ProgramChecked;
        public event Action<int> SlotChanged;

        private void removeButton_Click(object sender, EventArgs e)
        {
            if(seqDataGridView.SelectedCells.Count > 0)
            {
                foreach(DataGridViewCell cell in seqDataGridView.SelectedCells)
                {
                    cell.Style.BackColor = Color.Black;
                    recalculateColor(cell.RowIndex, cell.ColumnIndex);
                }
            }
        }

        private void setColorButton_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            colorPictureBox.BackColor = colorDialog1.Color;
        }

        private void seqDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (seqDataGridView.SelectedCells.Count > 1)
            {
                var selected = seqDataGridView.SelectedCells;
                bool isCRow = false;
                int selectedIndex = selected.Count - 1;

                DataGridViewCustomCell baseCell = (DataGridViewCustomCell) seqDataGridView.SelectedCells[seqDataGridView.SelectedCells.Count - 1];

                if (selected[selectedIndex].Style.BackColor != seqDataGridView.DefaultCellStyle.BackColor)
                {
                    if (selected[selectedIndex].RowIndex == 3)
                        isCRow = true;
                    
                    foreach (DataGridViewCell c in seqDataGridView.SelectedCells)
                    {                                                                    //  c.RowIndex == selected[selectedIndex].RowIndex
                        DataGridViewCustomCell cell = (DataGridViewCustomCell)c;
                        Color color = seqDataGridView.Rows[selected[selectedIndex].RowIndex].Cells[selected[selectedIndex].ColumnIndex].Style.BackColor;
                        if (!isCRow)
                        {
                            if(cell.RowIndex == baseCell.RowIndex)
                            {
                                cell.Style.BackColor = baseCell.Style.BackColor;//color;
                                cell.mode = baseCell.mode;
                                cell.rate = baseCell.rate;
                            }
                        }
                        else
                        {
                            int col = c.ColumnIndex;
                            DataGridViewCustomCell redCell      = (DataGridViewCustomCell)seqDataGridView.Rows[0].Cells[col];
                            DataGridViewCustomCell greenCell    = (DataGridViewCustomCell)seqDataGridView.Rows[1].Cells[col];
                            DataGridViewCustomCell blueCell     = (DataGridViewCustomCell)seqDataGridView.Rows[2].Cells[col];
                            DataGridViewCustomCell colorCell    = (DataGridViewCustomCell)seqDataGridView.Rows[3].Cells[col];
                            redCell.Style.BackColor     = Color.FromArgb(baseCell.Style.BackColor.R, 0, 0);
                            redCell.mode = baseCell.mode;
                            redCell.rate = baseCell.rate;
                            greenCell.Style.BackColor   = Color.FromArgb(0, baseCell.Style.BackColor.G, 0);
                            greenCell.mode = baseCell.mode;
                            greenCell.rate = baseCell.rate;
                            blueCell.Style.BackColor    = Color.FromArgb(0, 0, baseCell.Style.BackColor.B);
                            blueCell.mode = baseCell.mode;
                            blueCell.rate = baseCell.rate;
                            colorCell.Style.BackColor = baseCell.Style.BackColor;
                            colorCell.mode = baseCell.mode;
                            colorCell.rate = baseCell.rate;


                            /*
                            c.Style.BackColor = selected[selectedIndex].Style.BackColor;
                            seqDataGridView.Rows[0].Cells[col].Style.BackColor = Color.FromArgb(c.Style.BackColor.R, 0, 0);
                            seqDataGridView.Rows[1].Cells[col].Style.BackColor = Color.FromArgb(0, c.Style.BackColor.G, 0);
                            seqDataGridView.Rows[2].Cells[col].Style.BackColor = Color.FromArgb(0, 0, c.Style.BackColor.B);
                            seqDataGridView.Rows[3].Cells[col].Style.BackColor = color;
                            */
                        }
                        recalculateColor(c.RowIndex, c.ColumnIndex);
                    }
                }
            }
        }

        // drag drop color

        private void colorPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            colorPictureBox.DoDragDrop($"{colorPictureBox.BackColor.R} {colorPictureBox.BackColor.G} {colorPictureBox.BackColor.B}", DragDropEffects.Move | DragDropEffects.Move);
        }

        private void seqDataGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Move;
            } else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void seqDataGridView_DragDrop(object sender, DragEventArgs e)
        {

            Point clientPoint = seqDataGridView.PointToClient(new Point(e.X, e.Y));

            if (e.Data.GetData(DataFormats.Text).ToString() == "B")
            {
                int column = seqDataGridView.HitTest(clientPoint.X, clientPoint.Y).ColumnIndex;
                int row = seqDataGridView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
                int intensity = Convert.ToInt32(numericUpDown1.Value);
                switch (row)
                {
                    case 0:
                        seqDataGridView.Rows[row].Cells[column].Style.BackColor = Color.FromArgb(intensity, 0, 0);
                        break;
                    case 1:
                        seqDataGridView.Rows[row].Cells[column].Style.BackColor = Color.FromArgb(0, intensity, 0);
                        break;
                    case 2:
                        seqDataGridView.Rows[row].Cells[column].Style.BackColor = Color.FromArgb(0, 0, intensity);
                        break;
                    case 3:
                        seqDataGridView.Rows[0].Cells[column].Style.BackColor = Color.FromArgb(intensity, 0, 0); //R
                        seqDataGridView.Rows[1].Cells[column].Style.BackColor = Color.FromArgb(0, intensity, 0); //G
                        seqDataGridView.Rows[2].Cells[column].Style.BackColor = Color.FromArgb(0, 0, intensity); //B
                        seqDataGridView.Rows[3].Cells[column].Style.BackColor = Color.FromArgb(intensity, intensity, intensity);
                        break;
                }
                recalculateColor(row, column);

            } else
            {

                int column = seqDataGridView.HitTest(clientPoint.X, clientPoint.Y).ColumnIndex;
                string[] rgbs = e.Data.GetData(DataFormats.Text).ToString().Split(null);

                if(column > 0)
                {
                    seqDataGridView.Rows[0].Cells[column].Style.BackColor = Color.FromArgb(Int32.Parse(rgbs[0]), 0, 0); //R
                    seqDataGridView.Rows[1].Cells[column].Style.BackColor = Color.FromArgb(0, Int32.Parse(rgbs[1]), 0); //G
                    seqDataGridView.Rows[2].Cells[column].Style.BackColor = Color.FromArgb(0, 0, Int32.Parse(rgbs[2])); //B
                    seqDataGridView.Rows[3].Cells[column].Style.BackColor = Color.FromArgb(Int32.Parse(rgbs[0]), Int32.Parse(rgbs[1]), Int32.Parse(rgbs[2]));
                }
            }
        }

        private void colorPictureBox_DoubleClick(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            colorPictureBox.BackColor = colorDialog1.Color;
        }

        // advanced form. Auto generate doesn't change when I fix the name....
        private void button1_Click(object sender, EventArgs e)
        {
            int rate;
            string mode;
            bool setAll = false;
            if (seqDataGridView.SelectedCells.Count == 1)
            {
                DataGridViewCustomCell customCell = (DataGridViewCustomCell)seqDataGridView.SelectedCells[0];
                rate = customCell.rate;
                mode = customCell.mode;
                setAll = customCell.RowIndex == 3 ? true : false;
            } else if(seqDataGridView.SelectedCells.Count > 1)
            {
                DataGridViewCustomCell customCell = (DataGridViewCustomCell)seqDataGridView.SelectedCells[seqDataGridView.SelectedCells.Count-1];
                rate = customCell.rate;
                mode = customCell.mode;
            } else
            {
                return; //exit if nothing is selected
            }


            var popup = new AdvancedSettingsForm(rate, mode);
            popup.ShowDialog();
            if(popup.DialogResult == DialogResult.OK && seqDataGridView.SelectedCells.Count > 0)
            {
                CellModeEvaluation evaluation = new CellModeEvaluation();
                for (int i = 0; i < seqDataGridView.SelectedCells.Count; i++)
                {
                    DataGridViewCustomCell cell = (DataGridViewCustomCell)seqDataGridView.SelectedCells[i];
                    cell.rate = popup.rate;
                    cell.mode = popup.mode;

                    if (cell.RowIndex == 3)
                        evaluation.colorTouchedRows.Add(cell.ColumnIndex);
                    else
                        evaluation.normalTouchedRows.Add(cell.ColumnIndex);
                }
                evaluation.colorTouchedRows = evaluation.colorTouchedRows.Distinct().ToList();
                evaluation.normalTouchedRows = evaluation.normalTouchedRows.Distinct().ToList();
                //recalculateMode(evaluation);
            }
        }

        // drag drop base
        private void basePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            basePictureBox.DoDragDrop("B", DragDropEffects.Move | DragDropEffects.Move);
        }


        // export handler
        private void button1_Click_1(object sender, EventArgs e)
        {
            CellData[,] cells = new CellData[3, NUM_ROW_CELLS];
            
            for (int row_i = 0; row_i < 3; row_i++)
            {

                DataGridViewRow row = seqDataGridView.Rows[row_i];
                for(int cell_i = 0; cell_i < row.Cells.Count; cell_i++)
                {
                    DataGridViewCustomCell customCell = (DataGridViewCustomCell) row.Cells[cell_i];
                    int rate = customCell.rate;
                    string mode = customCell.mode;
                    int[] intensities = new int[] { customCell.Style.BackColor.R, customCell.Style.BackColor.G, customCell.Style.BackColor.B };
                    cells[row_i, cell_i] = new CellData { rate = rate, mode = mode, intensity = intensities[row_i], color = row_i};
                }
            }
            ExportClicked(cells);
        }

        private void comComboBox_DropDown(object sender, EventArgs e)
        {
            var ports = SerialPort.GetPortNames();
            comComboBox.DataSource = ports;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ConnectClicked(comComboBox.Text);
        }


        // SlotComboBoxChanged(slotComboBox.Text);
        // not needed for intensity value on single slot
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void programCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ProgramChecked(programCheckBox.Checked);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            SlotChanged(Convert.ToInt32(numericUpDown2.Value));
        }
    }
}
