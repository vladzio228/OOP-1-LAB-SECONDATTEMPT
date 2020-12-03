using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace Lab1
{
    public partial class MainWindow : Form
    {
        private bool _isCreated;
        private int _rowNumber;
        private int _colNumber;
        public static Dictionary<string, Cell> cellNameToCellObject = new Dictionary<string, Cell>();

        public MainWindow()
        {
            InitializeComponent();
            CreateTable(10, 10);
        }

        private void CreateTable(int cols, int rows)
        {
            _rowNumber = 0;
            _colNumber = 0;
            for (int i = 0; i < cols; ++i)
            {
                string colname = Base26System.Convert(i); //A, B, ... , Z, AA, AB ...
                dataGrid.Columns.Add(colname, colname);
                dataGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                ++_colNumber;
            }

            for (int i = 0; i < rows; ++i)
            {
                dataGrid.Rows.Add();
                dataGrid.Rows[i].HeaderCell.Value = (i).ToString();
                ++_rowNumber;
            }

            for (int i = 0; i < _rowNumber; ++i) //add to dictionary
            {
                for (int j = 0; j < _colNumber; ++j)
                {
                    string cellName = Base26System.Convert(j) + (i).ToString(); //B + 3 = B3
                    Cell cell = new Cell(cellName, i, j);
                    cellNameToCellObject.Add(cellName, cell);
                }
            }
            _isCreated = true;
        }

        private bool FindLoopDependencies(Cell current, Cell initial)
        {
            if (current.cellsItDependsOn.Contains(initial.Name))
            {
                return true;
            }
            foreach (string cellname in current.cellsItDependsOn)
            {
                if (FindLoopDependencies(cellNameToCellObject[cellname], initial))
                {
                    return true;
                }
            }
            return false;
        }

        private void RecalculateDependentCells(Cell changedCell)
        {
            foreach(string dependentCell in changedCell.cellsDependentFromIt)
            {
                cellNameToCellObject[dependentCell].Value = Calculator.Evaluate(cellNameToCellObject[dependentCell].Expression);
                dataGrid[cellNameToCellObject[dependentCell].Col, cellNameToCellObject[dependentCell].Row].Value = cellNameToCellObject[dependentCell].Value.ToString();
                RecalculateDependentCells(cellNameToCellObject[dependentCell]);
            }
        }

        private bool CheckCellsInExpression(Cell cell)
        {
            foreach (string everycell in cellNameToCellObject.Keys) 
            {
                if (cellNameToCellObject[everycell].cellsDependentFromIt.Contains(cell.Name))
                    cellNameToCellObject[everycell].cellsDependentFromIt.Remove(cell.Name);
            }
            cellNameToCellObject[cell.Name].cellsItDependsOn.Clear(); 
            Regex regex = new Regex(@"[A-Z]+([0-9]+)"); 
            cell.Expression = FormulaTextBox.Text;
            MatchCollection matches = regex.Matches(cell.Expression); 
            if (matches.Count > 0)  
            {
                foreach (Match match in matches)
                {
                    if (cellNameToCellObject.ContainsKey(match.ToString())) 
                    {
                        cellNameToCellObject[match.ToString()].cellsDependentFromIt.Add(cell.Name);
                    }
                    else 
                    {
                        foreach(Match cellmatch in matches)
                        {
                            if(cellNameToCellObject.ContainsKey(cellmatch.ToString())) 
                                cellNameToCellObject[cellmatch.ToString()].cellsDependentFromIt.Remove(cell.Name);
                        }
                        MessageBox.Show("You wrote the expression in cell " + match.ToString() + ", that does not exist\n Clearing this cell", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cell.Expression = "";
                        return false;
                    }
                    cellNameToCellObject[cell.Name].cellsItDependsOn.Add(match.ToString());
                }

                if (FindLoopDependencies(cell, cell))
                {
                    MessageBox.Show("Loop in expression\nClearing this cell", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cell.Expression = "";
                    cell.cellsItDependsOn.Clear(); 
                    foreach (Match cellmatch in matches) 
                    {
                        cellNameToCellObject[cellmatch.ToString()].cellsDependentFromIt.Remove(cell.Name);
                    }
                    return false;
                }
            }
            return true;
        }
        private bool CalculateCellValue(Cell cell)
        {
            try
            {
                cell.Value = Calculator.Evaluate(cell.Expression);
            }
            catch(FormatException)
            {
                MessageBox.Show("Wrong using operators","Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cell.Expression = "";
                return false;
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Unsupported symbols in expresion", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cell.Expression = "";
                return false;
            }
            catch(DivideByZeroException)  
            {
                MessageBox.Show("Div by zero", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cell.Expression = "";
                return false;
            }
            catch(Exception e) 
            {
                MessageBox.Show(e.Message);
            }
            return true;
        }
        
        private void dataGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            string cellName = Base26System.Convert(dataGrid.CurrentCell.ColumnIndex) + dataGrid.CurrentCell.RowIndex.ToString();
            if (!_isCreated)
            { 
                return;
            }

            if(cellNameToCellObject[cellName].Expression != "")
            {
                FormulaTextBox.Text = cellNameToCellObject[cellName].Expression;   //cell expression to a FormulaBox
                dataGrid[e.ColumnIndex, e.RowIndex].Value = cellNameToCellObject[cellName].Value.ToString();  //cell value to a grid cell
            }
            else
            {
                FormulaTextBox.Text = "";   //no expression -> no text in FormulaBox
            }
            this.ActiveControl = FormulaTextBox;
            FormulaTextBox.Focus();
            FormulaTextBox.SelectionStart = FormulaTextBox.Text.Length;
        }

        private void FormulaTextBox_Leave(object sender, EventArgs e)  //end of cell expression editing
        {
            //current cell name
            string cellName = Base26System.Convert(dataGrid.CurrentCell.ColumnIndex) + dataGrid.CurrentCell.RowIndex.ToString();
            if (CheckCellsInExpression(cellNameToCellObject[cellName]))  //no recursion and cells in expression exist
            {
                if (CalculateCellValue(cellNameToCellObject[cellName])) //calculating expression for a cell + handling exceptions
                {
                    RecalculateDependentCells(cellNameToCellObject[cellName]);
                    return;
                }
            }
            dataGrid[cellNameToCellObject[cellName].Col, cellNameToCellObject[cellName].Row].Value = "";
        }

        private void rowsAddButton_Click(object sender, EventArgs e)
        {
            ++dataGrid.RowCount;
            dataGrid.Rows[_rowNumber].HeaderCell.Value = _rowNumber.ToString();
            for (int j = 0; j < _colNumber; ++j)  
            {
                string cellName = Base26System.Convert(j) + _rowNumber.ToString(); //A + 3 = A3
                Cell cell = new Cell(cellName, _rowNumber, j);
                cellNameToCellObject.Add(cellName, cell);
            }
            ++_rowNumber;
        }
        private void columnsAddButton_Click(object sender, EventArgs e)
        {
            string colname = Base26System.Convert(_colNumber); //A, B, ... , Z, AA, AB ...
            dataGrid.Columns.Add(colname, colname);
            dataGrid.Columns[_colNumber].SortMode = DataGridViewColumnSortMode.NotSortable;
            for (int j = 0; j < _rowNumber; ++j)  
            {
                string cellName = Base26System.Convert(_colNumber) + j.ToString(); //A + 3 = A3
                Cell cell = new Cell(cellName, j, _colNumber);
                cellNameToCellObject.Add(cellName, cell);
            }
            ++_colNumber;
        }

        private void rowsDeleteButton_Click(object sender, EventArgs e)
        {
            if(_rowNumber == 1)
            {
                MessageBox.Show("Last row", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int last_rowNumber = _rowNumber - 1;
            for (int i = 0; i < _colNumber; ++i)
            {
                string cellToDeleteName = Base26System.Convert(i) + last_rowNumber.ToString();
                if(cellNameToCellObject[cellToDeleteName].Expression != "")
                {
                    DialogResult result = MessageBox.Show("In cell exists some data\n" +
                        "Do you want to delete this?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result == DialogResult.No)
                    {
                        return;
                    }
                }
                if (cellNameToCellObject[cellToDeleteName].cellsDependentFromIt.Count != 0)
                {
                    MessageBox.Show("The cell from row is used in expression", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            for (int i = 0; i < _colNumber; ++i)
            {
                string cellToDeleteName = Base26System.Convert(i) + last_rowNumber.ToString();
                cellNameToCellObject.Remove(cellToDeleteName);
                foreach (string everycell in cellNameToCellObject.Keys) 
                {
                    if (cellNameToCellObject[everycell].cellsDependentFromIt.Contains(cellToDeleteName))
                        cellNameToCellObject[everycell].cellsDependentFromIt.Remove(cellToDeleteName);
                }
            }
            dataGrid.Rows.RemoveAt(last_rowNumber);
            --_rowNumber;
        }

        private void columnsDeleteButton_Click(object sender, EventArgs e)
        {
            if (_colNumber == 1)
            {
                MessageBox.Show("Last column", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int last_colNumber = _colNumber - 1;
            for (int i = 0; i < _rowNumber; ++i)
            {
                string cellToDeleteName = Base26System.Convert(last_colNumber) + i.ToString();
                if (cellNameToCellObject[cellToDeleteName].Expression != "")
                {
                    DialogResult result = MessageBox.Show("In cell exists some data\n" +
                        "Do you want to delete this?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }
                if (cellNameToCellObject[cellToDeleteName].cellsDependentFromIt.Count != 0)
                {
                    MessageBox.Show("The cell from row is used in expression", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            for (int i = 0; i < _rowNumber; ++i)
            {
                string cellToDeleteName = Base26System.Convert(last_colNumber) + i.ToString();
                cellNameToCellObject.Remove(cellToDeleteName);
                foreach (string everycell in cellNameToCellObject.Keys)
                {
                    if (cellNameToCellObject[everycell].cellsDependentFromIt.Contains(cellToDeleteName))
                        cellNameToCellObject[everycell].cellsDependentFromIt.Remove(cellToDeleteName);
                }
            }
            dataGrid.Columns.RemoveAt(last_colNumber);
            --_colNumber;
        }

        private void MenuStripHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Лабораторна робота №1, варіант 19\n\n" +
                "Студента групи К-24, Петріва Владислава\n\n" +               
                "Реалізовані операції в парсері за допомогою ANTLR:\n" +
                " +; -; *; /; \n" +
                "mod(x; y); div(x; y); \n" +
                "inc(x); dec(y); \n" +
                "max(x,y), min(x,y)\n",
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "IDDQD_Spreadsheet|*.anikdot";
                saveFileDialog.Title  = "Save .anikdot file";
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;
            FileStream fs = (FileStream)saveFileDialog.OpenFile();
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(_colNumber);
            sw.WriteLine(_rowNumber);

            foreach (Cell cell in cellNameToCellObject.Values)
            {
                sw.WriteLine(cell.Col);
                sw.WriteLine(cell.Row);
                sw.WriteLine(cell.Expression);
                sw.WriteLine(cell.Value);

                sw.WriteLine(cell.cellsItDependsOn.Count);
                foreach (string cellName in cell.cellsItDependsOn)
                {
                    sw.WriteLine(cellName);
                }

                sw.WriteLine(cell.cellsDependentFromIt.Count);
                foreach (string cellName in cell.cellsDependentFromIt)
                {
                    sw.WriteLine(cellName);
                }
            }
            sw.Close();
            fs.Close();
            MessageBox.Show("Spreadsheet succesfully saved", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MyTableSavedFile|*.anikdot";
            openFileDialog.Title  = "Open a .anikdot file";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            StreamReader sr = new StreamReader(openFileDialog.FileName);
            cellNameToCellObject.Clear();
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();
            _isCreated = false;
            int.TryParse(sr.ReadLine(), out int cols);
            int.TryParse(sr.ReadLine(), out int rows);
            CreateTable(cols, rows);
            string A0expr;
            for(int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < cols; ++c)
                {
                    int colNum = Convert.ToInt32(sr.ReadLine());
                    int rowNum = Convert.ToInt32(sr.ReadLine());
                    string currentCellName = Base26System.Convert(colNum) + (rowNum+1).ToString();
                    string expression = sr.ReadLine();
                    if (c == 0 || r == 0)
                    {
                        A0expr = expression;
                    }
                    double value = Convert.ToDouble(sr.ReadLine());

                    if(expression != "")
                    {
                        cellNameToCellObject[currentCellName].Expression = expression;
                        cellNameToCellObject[currentCellName].Value = value;
                    }
                    int itDependOnCount = Convert.ToInt32(sr.ReadLine());
                    for(int i = 0; i < itDependOnCount; ++i)
                    {
                        string cellItDependsOn = sr.ReadLine();
                        cellNameToCellObject[currentCellName].cellsItDependsOn.Add(cellItDependsOn);
                    }

                    int dependentFromItCount = Convert.ToInt32(sr.ReadLine());
                    for(int i = 0; i < dependentFromItCount; ++i)
                    {
                        string dependentCell = sr.ReadLine();
                        cellNameToCellObject[currentCellName].cellsDependentFromIt.Add(dependentCell);
                    }
                    if(expression != "")
                    {
                        dataGrid[colNum, rowNum].Value = value;
                    }
                }
            }
            FormulaTextBox.Text = cellNameToCellObject["A0"].Expression;
            sr.Close();
        }
        private void MenuStripFileSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void MenuStripFileOpen_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("The table was not saved. Save it?", "Warning!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if(result == DialogResult.Yes)
            {
                MenuStripFileSave.PerformClick();
                OpenFile();
            }
            else if(result == DialogResult.No)
            {
                OpenFile();

            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("The table was not saved." +
                "Save it?", "Warning!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                MenuStripFileSave.PerformClick();
            }
            else if (result == DialogResult.No)
            {
             
            }
            else if(result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            
        }
    }
    
}
