using System;
using System.Windows.Forms;

namespace SLAE
{
    [AlgNameAttribute("Метод Гаусса")]
    public sealed class Gauss : SLAEAlg
    {
        private double[,] _a = new double[3, 3];
        private double[] _b = new double[3];
        private double[] _x = new double[3];
        private bool _inputError;
        private bool _x3 = false;

        public Gauss(DataGridView a, DataGridView b) : base()
        {
            int i = 0;

            foreach (DataGridViewRow row in a.Rows)
            {
                try
                {
                    _a[i, 0] = Convert.ToDouble(row.Cells["A1"].Value.ToString());
                    _a[i, 1] = Convert.ToDouble(row.Cells["A2"].Value.ToString());
                    _a[i, 2] = Convert.ToDouble(row.Cells["A3"].Value.ToString());
                }
                catch (FormatException)
                {
                    _inputError = true;
                    break;
                }

                i++;
            }

            i = 0;

            foreach (DataGridViewRow row in b.Rows)
            {
                try
                {
                    _b[i] = Convert.ToDouble(row.Cells["B"].Value.ToString());
                }
                catch (FormatException)
                {
                    _inputError = true;
                    break;
                }

                i++;
            }

            XStrings = new string[3];
            X = new double[3];
        }

        public override bool Solve(bool insertPerturbation = false)
        {
            if (_inputError)
            {
                Message = "Ошибка: данные введены не верно.";
                return false;
            }

            if (insertPerturbation)
                _b[2] += 0.01;

            if (_a[0, 0] == 0 && _a[1, 0] == 0 && _a[2, 0] == 0 && _a[2, 1] == 0 && _a[2, 2] == 0 && _b[2] == 0)
            {
                _a[0, 0] = _a[0, 1];
                _a[1, 0] = _a[1, 1];
                _a[0, 1] = _a[0, 2];
                _a[1, 1] = _a[1, 2];
                _x3 = true;
            }

            SortRows();
            ToUpperTriangularMx();

            if (CheckForInconsistentEquations())
            {
                Message = "Система не имеет решения.";
                return false;
            }

            TriangularToIdentityMx();
            ExtractX();

            if ((_x[0] == 1 || Double.IsPositiveInfinity(_x[0])) && Double.IsPositiveInfinity(_x[1])
                && Double.IsPositiveInfinity(_x[2]))
            {
                Message = "Система имеет бесконечное количество решений.";
                return false;
            }

            if (!CheckResult())
            {
                Message = "Система не имеет решения.";
                return false;
            }

            if (_x3)
            {
                XStrings[2] = XStrings[1];
                XStrings[1] = XStrings[0];
                Message += " X1 не влияет на решение.";
                XStrings[0] = "Свободен";
            }

            return true;
        }

        private bool CheckResult()
        {
            for (int i = 0; i < _a.GetLength(0); i++)
            {
                double temp = 0;
                double temp2 = Math.Round(_b[i], 1);
                bool skip = false;

                for (int j = 0; j < _a.GetLength(0); j++)
                {
                    if (!double.IsPositiveInfinity(_x[j]))
                        temp += _a[i, j] * _x[j];
                    else
                        skip = true;
                }

                temp = Math.Round(temp, 1);

                if (temp != temp2 && !skip)
                    return false;
            }

            return true;
        }

        private void SwapRows(int x, int y)
        {
            double temp;

            for (int i = 0; i < _a.GetLength(1); i++)
            {
                temp = _a[x, i];
                _a[x, i] = _a[y, i];
                _a[y, i] = temp;
            }

            temp = _b[x];
            _b[x] = _b[y];
            _b[y] = temp;
        }

        private void SortRows()
        {
            for (int i = 0; i < _a.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < _a.GetLength(0) - i - 1; j++)
                {
                    if (_a[j, 0] < _a[j + 1, 0])
                        SwapRows(j, j + 1);
                }
            }
        }

        private bool CheckForInconsistentEquations()
        {
            for (int i = 0; i < _a.GetLength(0); i++)
            {
                bool allNull = true;

                for (int j = 0; j < _a.GetLength(0); j++)
                {
                    if (_a[i, j] != 0)
                    {
                        allNull = false;
                        break;
                    }
                }

                if (allNull && _b[i] != 0)
                    return true;
            }

            return false;
        }

        private void ToUpperTriangularMx()
        {
            for (int j = 0; j < _a.GetLength(0); j++)
            {
                for (int i = 2; j < i; i--)
                {
                    if (_a[i, j] == 0)
                        continue;

                    double[] temp = new double[_a.GetLength(0) + 1];
                    double multiplier = -(_a[i, j]/_a[i - 1, j]);

                    for (int k = 0; k < _a.GetLength(0); k++)
                        temp[k] = _a[i - 1, k]*multiplier;

                    temp[_a.GetLength(0)] = _b[i - 1]*multiplier;

                    for (int k = 0; k < _a.GetLength(0); k++)
                    {
                        _a[i, k] = Math.Round(_a[i, k] + temp[k], 8);
                    }

                    _b[i] = Math.Round(_b[i] + temp[_a.GetLength(0)], 8);
                }
            }
        }

        private void TriangularToIdentityMx()
        {
            for (int i = 0; i < _a.GetLength(0); i++)
            {
                double divider = _a[i, i];

                if (divider == 0)
                    continue;

                for (int j = 0; j < _a.GetLength(0); j++)
                {
                    _a[i, j] = Math.Round(_a[i, j]/divider, 8);
                }

                _b[i] = Math.Round(_b[i]/divider, 8);
            }
        }

        private bool ExtractX()
        {
            bool allInfinity = true;

            for (int i = _a.GetLength(0) - 1; i >= 0; i--)
            {
                if (_a[i, i] == 0)
                {
                    _x[i] = Double.PositiveInfinity;
                }
                else
                {
                    _x[i] = _b[i];

                    for (int j = 0; j < _a.GetLength(1); j++)
                    {
                        if (j > i)
                        {
                            if (!double.IsPositiveInfinity(_x[j]))
                                _x[i] -= _a[i, j]*_x[j];
                        }
                    }
                }
            }

            for (int i = 0; i < _x.GetLength(0); i++)
            {
                if (!double.IsPositiveInfinity(_x[i]))
                {
                    X[i] = _x[i];
                    XStrings[i] = Math.Round(_x[i], 2).ToString();

                    if (i == 0)
                    {
                        for (int j = i + 1; j < _a.GetLength(1); j++)
                        {
                            if (double.IsPositiveInfinity(_x[j]) && _a[i, j] != 0 && !_x3)
                                XStrings[i] += "-X" + (j + 1);
                            else
                            {
                                allInfinity = false;
                            }
                        }
                    }
                }
                else
                {
                    XStrings[i] = "Бесконечность";
                }
            }

            return !allInfinity;
        }
    }
}
