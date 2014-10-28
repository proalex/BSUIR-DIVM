using System;
using System.Numerics;
using System.Windows.Forms;

namespace SLAE
{
    [AlgNameAttribute("Метод Холецкого (компл.)")]
    public sealed class CholeskyComplex : SLAEAlg
    {
        private Complex[,] _a = new Complex[3, 3];
        private Complex[,] _l = new Complex[3, 3];
        private Complex[,] _lT = new Complex[3, 3];
        private Complex[] _b = new Complex[3];
        private bool _inputError;

        public CholeskyComplex(DataGridView a, DataGridView b)
        {
            int i = 0;

            foreach (DataGridViewRow row in a.Rows)
            {
                try
                {
                    _a[i, 0] = new Complex(Convert.ToDouble(row.Cells["A1"].Value.ToString()), 0);
                    _a[i, 1] = new Complex(Convert.ToDouble(row.Cells["A2"].Value.ToString()), 0);
                    _a[i, 2] = new Complex(Convert.ToDouble(row.Cells["A3"].Value.ToString()), 0);
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
                    _b[i] = new Complex(Convert.ToDouble(row.Cells["B"].Value.ToString()), 0);
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
                    LStrings[i][j] = ComplexToString(_l[i, j]);
                }
            }

            TransposeL();
            ExtractY();
            ExtractX();
            return true;
        }

        private void ToSymmetricMx()
        {
            Complex[,] aT = new Complex[3, 3];
            Complex[,] aTemp = new Complex[3, 3];
            Complex[] bTemp = new Complex[3];

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
                        _a[i, j] += aT[i, k] * aTemp[k, j];
                    }
                }
            }

            for (int i = 0; i < _a.GetLength(0); i++)
            {
                for (int j = 0; j < _a.GetLength(1); j++)
                {
                    _b[i] += bTemp[j] * aT[i, j];
                }
            }
        }

        private void ExtractY()
        {
            int bFound = 0;

            for (int i = 0; i < _l.GetLength(0); i++)
            {
                Complex divider = _l[i, i];

                if (divider == 0)
                    continue;

                for (int j = 0; j < _l.GetLength(0); j++)
                {
                    _l[i, j] = _l[i, j]/divider;
                }

                _b[i] = _b[i]/divider;
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

                _b[i] = _b[i];
                bFound++;
            }
        }

        private void ExtractX()
        {
            int bFound = 0;

            for (int i = 0; i < _lT.GetLength(0); i++)
            {
                Complex divider = _lT[i, i];

                if (divider == 0)
                    continue;

                for (int j = 0; j < _lT.GetLength(0); j++)
                {
                    _lT[i, j] = _lT[i, j]/divider;
                }

                _b[i] = _b[i]/divider;
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

                XStrings[i] = Math.Round(_b[i].Real, 2).ToString();
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
                        Complex temp = 0;

                        for (int k = 0; k < j; k++)
                        {
                            temp += _l[i, k]*_l[j, k];
                        }

                        _l[i, j] = (_a[i, j] - temp)/_l[j, j];
                    }
                    else if (i == j)
                    {
                        Complex temp = 0;

                        for (int k = 0; k < j; k++)
                        {
                            temp += _l[i, k]*_l[i, k];
                        }

                        _l[i, j] = Complex.Sqrt(_a[i, j] - temp);
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

        private string ComplexToString(Complex number)
        {
            string rl, img, result = "";
            double real = Math.Round(number.Real, 2);
            double imaginary = Math.Round(number.Imaginary, 2);

            rl = real.ToString();
            img = imaginary.ToString();

            if (imaginary != 0)
            {
                if (real == 0)
                {
                   result += imaginary + "i";
                }
                else
                {
                    result += real;

                    if (imaginary > 0)
                        result += "+";

                    result += imaginary + "i";
                }
            }
            else
            {
                result = rl;
            }

            return result;
        }

        private bool IsPositiveDefinedMx()
        {
            Complex[] principalMinors = new Complex[3];

            principalMinors[0] = _a[0, 0];
            principalMinors[1] = _a[0, 0] * _a[1, 1] - _a[0, 1] * _a[1, 0];
            principalMinors[2] = _a[0, 0] * _a[1, 1] * _a[2, 2] + _a[0, 1] * _a[1, 2] * _a[2, 0] + _a[0, 2] * _a[1, 0] * _a[2, 1] -
                                 _a[0, 2] * _a[1, 1] * _a[2, 0] - _a[0, 0] * _a[1, 2] * _a[2, 1] - _a[0, 1] * _a[1, 0] * _a[2, 2];

            for (int i = 0; i < principalMinors.GetLength(0); i++)
            {
                if (principalMinors[i].Real == 0)
                    return false;
            }

            return true;
        }
    }
}
