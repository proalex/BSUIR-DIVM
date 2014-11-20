using System;
using System.Windows.Forms;

namespace SLAE
{
    [AlgNameAttribute("Метод Холецкого")]
    public sealed class Cholesky : SLAEAlg
    {
        private double[,] _a = new double[3, 3];
        private double[,] _l = new double[3, 3];
        private double[,] _lT = new double[3, 3];
        private double[] _b = new double[3];
        private bool _inputError;

        public Cholesky(DataGridView a, DataGridView b)
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
            X = new double[3];
        }


        public override bool Solve()
        {
            if (_inputError)
            {
                Message = "Ошибка: данные введены не верно.";
                return false;
            }

            if (!IsSymmetricMx())
                ToSymmetricMx();

            if (!IsPositiveDefinedMx())
            {
                Message = "Систему нельзя решить методом Холецкого.";
                return false;
            }

            Decompose();
            LStrings = new string[3][];

            for (int i = 0; i < _l.GetLength(0); i++)
            {
                LStrings[i] = new string[3];

                for (int j = 0; j < _l.GetLength(1); j++)
                {
                    LStrings[i][j] = Math.Round(_l[i, j], 2).ToString();
                }
            }

            TransposeL();
            ExtractY();
            ExtractX();
            return true;
        }

        private void ToSymmetricMx()
        {
            double[,] aT = new double[3, 3];
            double[,] aTemp = new double[3, 3];
            double[] bTemp = new double[3];

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    aTemp[i, j] = _a[i, j];
                    _a[i, j] = 0;
                }
            }

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                bTemp[i] = _b[i];
                _b[i] = 0;
            }

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    aT[j, i] = aTemp[i, j];
                }
            }

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    for (int k = 0; k < _a.GetLength(0); k++)
                    {
                        _a[i, j] += aT[i, k]*aTemp[k, j];
                    }
                }
            }

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    _b[i] += bTemp[j]*aT[i, j];
                }
            }
        }

        private void ExtractY()
        {
            int bFound = 0;

            for (int i = 0; i < _l.GetLength(0); i++)
            {
                double divider = _l[i, i];

                if (divider == 0)
                    continue;

                for (int j = 0; j < _l.GetLength(0); j++)
                {
                    _l[i, j] = Math.Round(_l[i, j]/divider, 8);
                }

                _b[i] = Math.Round(_b[i]/divider, 8);
            }

            for (int i = 0; i < _l.GetLength(0); i++)
            {
                for (int j = 0; j < _l.GetLength(0); j++)
                {
                    if ((j < bFound + 1) && i != j)
                    {
                        _b[i] -= _l[i, j]*_b[j];
                    }
                }

                _b[i] = Math.Round(_b[i], 8);
                bFound++;
            }
        }

        private void ExtractX()
        {
            int bFound = 0;

            for (int i = 0; i < _lT.GetLength(0); i++)
            {
                double divider = _lT[i, i];

                if (divider == 0)
                    continue;

                for (int j = 0; j < _lT.GetLength(0); j++)
                {
                    _lT[i, j] = Math.Round(_lT[i, j]/divider, 8);
                }

                _b[i] = Math.Round(_b[i]/divider, 8);
            }

            for (int i = _lT.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < _lT.GetLength(0); j++)
                {
                    if ((j > 1 - bFound) && i != j)
                    {
                        _b[i] -= _lT[i, j]*_b[j];
                    }
                }

                _b[i] = Math.Round(_b[i], 2);
                XStrings[i] = _b[i].ToString();
                X[i] = _b[i];
                bFound++;
            }
        }


        private void TransposeL()
        {
            for (int i = 0; i < _l.GetLength(0); i++)
            {
                for (int j = 0; j < _l.GetLength(1); j++)
                {
                    _lT[j, i] = _l[i, j];
                }
            }
        }

        private void Decompose()
        {
            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    if (j < i)
                    {
                        double temp = 0;

                        for (int k = 0; k < j; k++)
                        {
                            temp += _l[i, k]*_l[j, k];
                        }

                        _l[i, j] = (_a[i, j] - temp)/_l[j, j];
                    }
                    else if (i == j)
                    {
                        double temp = 0;

                        for (int k = 0; k < j; k++)
                        {
                            temp += Math.Pow(_l[i, k], 2);
                        }

                        _l[i, j] = Math.Round(Math.Sqrt(_a[i, j] - temp), 8);
                    }
                }
            }
        }

        private bool IsSymmetricMx()
        {
            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    if (_a[i, j] != _a[j, i])
                        return false;
                }
            }

            return true;
        }

        private bool IsPositiveDefinedMx()
        {
            double[] principalMinors = new double[3];

            principalMinors[0] = Math.Round(_a[0, 0], 8);
            principalMinors[1] = Math.Round(_a[0, 0]*_a[1, 1] - _a[0, 1]*_a[1, 0], 8);
            principalMinors[2] = Math.Round(_a[0, 0]*_a[1, 1]*_a[2, 2] + _a[0, 1]*_a[1, 2]*_a[2, 0] + _a[0, 2]*_a[1, 0]*_a[2, 1] -
                                 _a[0, 2]*_a[1, 1]*_a[2, 0] - _a[0, 0]*_a[1, 2]*_a[2, 1] - _a[0, 1]*_a[1, 0]*_a[2, 2], 8);

            for (int i = 0; i < principalMinors.GetLength(0); i++)
            {
                if (principalMinors[i] <= 0)
                    return false;
            }

            return true;
        }
    }
}
