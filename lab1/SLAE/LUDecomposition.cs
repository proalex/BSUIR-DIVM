using System;
using System.Windows.Forms;

namespace SLAE
{
    [AlgNameAttribute("LU-разложение")]
    internal class LUDecomposition : SLAEAlg
    {
        private double[,] _a = new double[3, 3];
        private double[] _b = new double[3];
        private bool _inputError;

        public LUDecomposition(DataGridView a, DataGridView b)
        {
            int i = 0;

            foreach (DataGridViewRow row in a.Rows)
            {
                try
                {
                    _a[i, 0] = Math.Round(Convert.ToDouble(row.Cells["A1"].Value.ToString()), 8);
                    _a[i, 1] = Math.Round(Convert.ToDouble(row.Cells["A2"].Value.ToString()), 8);
                    _a[i, 2] = Math.Round(Convert.ToDouble(row.Cells["A3"].Value.ToString()), 8);
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
                    _b[i] = Math.Round(Convert.ToDouble(row.Cells["B"].Value.ToString()), 8);
                }
                catch (FormatException)
                {
                    _inputError = true;
                    break;
                }

                i++;
            }

            XStrings = new string[3];
        }

        public override bool Solve()
        {
            int[] indx = new int[_b.Length];

            if (_inputError)
            {
                Message = "Ошибка: данные введены не верно.";
                return false;
            }

            if (!LUDecompose(ref indx))
            {
                Message = "Систему нельзя решить с помощью LU-разложения.";
                return false;
            }

            LStrings = new string[3][];

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                LStrings[i] = new string[3];

                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    if (j < i)
                        LStrings[i][j] = Math.Round(_a[i, j], 2).ToString();
                    else if (i == j)
                        LStrings[i][j] = "1";
                    else
                        LStrings[i][j] = "0";
                }
            }

            UStrings = new string[3][];

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                UStrings[i] = new string[3];

                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    if (j >= i)
                        UStrings[i][j] = Math.Round(_a[i, j], 2).ToString();
                    else
                        UStrings[i][j] = "0";
                }
            }

            ExtractX(indx);
            return true;
        }

        private bool LUDecompose(ref int[] indx)
        {
            int i, imax = 0, j, k, n = _a.GetLength(0);
            double big, temp;
            double[] vv = new double[n];

            for (i = 0; i < n; i++)
            {
                big = 0.0;
                for (j = 0; j < n; j++)
                {
                    if ((temp = Math.Abs(_a[i, j])) > big)
                        big = temp;
                }

                if (big == 0.0)
                    return false;

                vv[i] = 1.0/big;
            }

            for (k = 0; k < n; k++) 
            {
                big = 0.0;

                for (i = k; i < n; i++)
                {
                    temp = vv[i]*Math.Abs(_a[i, k]);

                    if (temp > big)
                    {
                        big = temp;
                        imax = i;
                    }
                }

                if (k != imax)
                {
                    for (j = 0; j < n; j++)
                    {
                        temp = _a[imax, j];
                        _a[imax, j] = _a[k, j];
                        _a[k, j] = temp;
                    }

                    vv[imax] = vv[k];
                }

                indx[k] = imax;

                for (i = k + 1; i < n; i++)
                {
                    temp = _a[i, k] /= _a[k, k];

                    for (j = k + 1; j < n; j++)
                    {
                        _a[i, j] -= temp*_a[k, j];
                    }
                }
            }

            return true;
        }

        private void ExtractX(int[] indx)
        {
            int n = _a.GetLength(0);
            int i, ii = 0, ip, j;
            double sum = 0;

            for (i = 0; i < n; i++)
            {
                ip = indx[i];
                sum = _b[ip];
                _b[ip] = _b[i];

                if (ii != 0)
                {
                    for (j = ii - 1; j < i; j++)
                    {
                        sum -= _a[i, j]*_b[j];
                    }
                }
                else if (sum != 0.0)
                {
                    ii = i + 1;
                }

                _b[i] = sum;
            }

            for (i = n - 1; i >= 0; i--)
            {
                sum = _b[i];

                for (j = i + 1; j < n; j++)
                {
                    sum -= _a[i, j]*_b[j];
                }

                _b[i] = Math.Round(sum / _a[i, i], 2);
                XStrings[i] = _b[i].ToString();
            }
        }
    }
}