using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SLAE;

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
                        double[] xDiff = new double[3];
                        double bDiff;
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
                            for (int i = 0; i < solution.X.GetLength(0); i++)
                            {
                                xDiff[i] = solution.X[i];
                            }

                            solution = (SLAEAlg)Activator
                                .CreateInstance(algorithm, dataGridView1, dataGridView3);
                            solution.Solve(true);
                            bDiff = Math.Abs((Math.Round(Convert.ToDouble(dataGridView3.Rows[0].Cells[0].Value.ToString()), 8) + 0.01) / 0.01);

                            for (int i = 0; i < solution.X.GetLength(0); i++)
                            {
                                xDiff[i] = Math.Abs(solution.X[i] - xDiff[i]);
                                xDiff[i] *= bDiff;
                            }

                            for (int i = xDiff.GetLength(0) - 1; i > 0; i--)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    if (xDiff[j] > xDiff[j + 1])
                                    {
                                        double temp = xDiff[j + 1];
                                        xDiff[j + 1] = xDiff[j];
                                        xDiff[j] = temp;
                                    }
                                }
                            }

                            label3.Text = Math.Round(xDiff[0], 2) + " >= Мера обусловленности <= "
                                + Math.Round(xDiff[xDiff.GetLength(0) - 1], 2) + Environment.NewLine;

                            if (xDiff[0] > 10)
                                label3.Text += "Система плохо обусловлена.";
                            else
                                label3.Text += "Система хорошо обусловлена.";

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
