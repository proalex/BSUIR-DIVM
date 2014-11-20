using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SLAE;
using CSML;

namespace lab1
{
    public partial class SLAE : Form
    {
        private Type[] _algorithms;

        public SLAE()
        {
            string[] aRow = {"0", "0", "0"};
            string[] bRow = {"0"};

            InitializeComponent();
            _algorithms = Assembly.GetAssembly(typeof(SLAEAlg)).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SLAEAlg))).ToArray();

            foreach (var algorithm in _algorithms)
            {
                listBox1.Items.Add(algorithm.GetCustomAttribute<AlgNameAttribute>().Name);
            }

            dataGridView1.Rows.Add(aRow);
            dataGridView1.Rows.Add(aRow);
            dataGridView1.Rows.Add(aRow);
            dataGridView3.Rows.Add(bRow);
            dataGridView3.Rows.Add(bRow);
            dataGridView3.Rows.Add(bRow);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[][] aRows =
            {
                new [] {"2,53", "2,36", "1,93"},
                new [] {"3,95", "4,11", "3,66"},
                new [] {"2,78", "2,43", "1,94"}
            };

            string[] bRows =
            {
                "12,66", "21,97", "13,93"
            };

            dataGridView1.Rows.Clear();
            dataGridView3.Rows.Clear();

            foreach (var row in aRows)
            {
                dataGridView1.Rows.Add(row);
            }

            foreach (var row in bRows)
            {
                dataGridView3.Rows.Add(row);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            var currentAlg = listBox1.SelectedItem.ToString();
            string message = "";
            dataGridView2.Rows.Clear();
            dataGridView4.Rows.Clear();
            dataGridView5.Rows.Clear();
            label3.Text = "";
            label3.Update();

            foreach (var algorithm in _algorithms)
            {
                if (algorithm.GetCustomAttribute<AlgNameAttribute>().Name
                    .Equals(currentAlg, StringComparison.Ordinal))
                {
                    var result = new string[3];
                    var solution = (SLAEAlg) Activator
                        .CreateInstance(algorithm, dataGridView1, dataGridView3);

                    if (solution.Solve())
                    {
                        bool isConditionNumber = true;

                        message = solution.Message;
                        dataGridView2.Rows.Add(solution.XStrings);

                        if (solution.LStrings != null)
                        {
                            foreach (var row in solution.LStrings)
                            {
                                dataGridView4.Rows.Add(row);
                            }
                        }

                        if (solution.UStrings != null)
                        {
                            foreach (var row in solution.UStrings)
                            {
                                dataGridView5.Rows.Add(row);
                            }
                        }

                        foreach (var x in solution.XStrings)
                        {
                            try
                            {
                                Math.Round(Convert.ToDouble(x), 8);
                            }
                            catch (FormatException)
                            {
                                isConditionNumber = false;
                                break;
                            }

                        }

                        if (isConditionNumber)
                        {
                            double[,] a = new double[3, 3];
                            int i = 0;

                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                try
                                {
                                    a[i, 0] = Convert.ToDouble(row.Cells["A1"].Value.ToString());
                                    a[i, 1] = Convert.ToDouble(row.Cells["A2"].Value.ToString());
                                    a[i, 2] = Convert.ToDouble(row.Cells["A3"].Value.ToString());
                                }
                                catch (FormatException)
                                {
                                    break;
                                }

                                i++;
                            }

                            double max1 = 0, max2 = 0;

                            for (int j = 0; j < a.GetLength(0); j++)
                            {
                                for (int k = 0; k < a.GetLength(1); k++)
                                {
                                    if (a[j, k] > max1)
                                        max1 = a[j, k];
                                }
                            }

                            Matrix matrix = new Matrix(a);
                            matrix = matrix.Inverse();

                            for (int j = 1; j <= matrix.ColumnCount; j++)
                            {
                                for (int k = 1; k <= matrix.RowCount; k++)
                                {
                                    if (matrix[j, k].Re > max2)
                                        max2 = matrix[j, k].Re;
                                }
                            }

                            label3.Text = Math.Round(max1*max2, 2).ToString();
                            label3.Update();
                        }
                    }
                    else
                    {
                        message = solution.Message;
                    }

                    label1.Text = message;
                    label1.Update();
                    break;
                }
            }
        }
    }
}
