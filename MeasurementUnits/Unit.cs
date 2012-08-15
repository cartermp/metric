﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementUnits
{
    public enum BaseUnit { m = 2, g = 3, s = 5, A = 7, K = 11, cd = 13, mol = 17 }

    public class Unit : IEnumerable<Unit>
    {
        #region Fields & Properties
        internal int Power10 { get; set; }
        internal event PropertyChangedEventHandler PropertyChanged;
        protected int _power;
        protected Prefix _prefix;
        public virtual Prefix Prefix
        {
            get { return _prefix; }
            set { if (PropertyChanged != null) PropertyChanged((value - _prefix) * _power, new PropertyChangedEventArgs("Prefix")); _prefix = value; }
        }
        public virtual int Power { get { return _power; } set { _power = value; } }
        public BaseUnit BaseUnit { get; set; }
        #endregion
        #region Constructors
        protected Unit()
        {
            _power = 1;
            Power10 = 0;
        }
        public Unit(BaseUnit baseUnit) : this()
        {
            this.BaseUnit = baseUnit;
        }
        public Unit(BaseUnit baseUnit, int power) : this(baseUnit)
        {
            this.Power = power;
        }
        public Unit(Prefix prefix, BaseUnit baseUnit) : this(baseUnit)
        {
            this.Prefix = prefix;
        }
        public Unit(Prefix prefix, BaseUnit baseUnit, int power) : this(baseUnit, power)
        {
            this.Prefix = prefix;
        }
        #endregion
        #region Operators
        public static Unit operator +(Unit u1, Unit u2)
        {
            if (u1.IsAddable(u2))
            {
                var newUnit = new Unit(AveragePrefix(u1.Prefix, u2.Prefix), u1.BaseUnit, u1.Power);
                return newUnit;
            }
            else
            {
                throw new GrandmothersAndFrogsException("You can't mix them. You just can't");
            }
        }
        public static Unit operator -(Unit u1, Unit u2)
        {
            return u1 + u2;
        }
        public static Unit operator *(Unit u1, Unit u2)
        {
            if (u1.BaseUnit == u2.BaseUnit && u1.BaseUnit != 0)
            {
                var averagePrefix = AveragePrefix(u1.Prefix, u2.Prefix);
                var newUnit = new Unit(averagePrefix, u1.BaseUnit, u1.Power + u2.Power);
                newUnit.Power10 = u1.DeterminePower10(averagePrefix) + u2.DeterminePower10(averagePrefix);
                return newUnit;
            }
            else
            {
                return ComplexUnit.Multiply(u1, u2);
            }
        }
        public static Unit operator /(Unit u1, Unit u2)
        {
            return u1 * u2.Pow(-1);
        }
        #endregion
        #region Methods
        public static Prefix AveragePrefix(params Prefix[] prefixes)
        {
            var average = prefixes.Average(x => (int)x);
            var averagePrefix = average != 0 ? Enum.GetValues(typeof(Prefix)).Cast<Prefix>().First(x => (int)x >= average) : 0;
            return averagePrefix;
        }

        public static Prefix FindClosestPrefix(int powerOfTen)
        {
            int absolutePower = Math.Abs(powerOfTen);
            Prefix prefix;
            if (absolutePower < 25)
            {
                if (absolutePower % 3 == 0 || absolutePower < 3)
                {
                    prefix = (Prefix)powerOfTen;
                }
                else
                {
                    prefix = Enum.GetValues(typeof(Prefix)).Cast<Prefix>().Where(x => (int)x <= powerOfTen).Max();
                }
            }
            else
            {
                prefix = Math.Sign(powerOfTen) == 1 ? Prefix.Y : Prefix.y;
            }
            return prefix;
        }

        public static Unit Parse(string s)
        {
            return UnitParser.Parse(s);
        }

        public static bool TryParse(string s, out Unit unit)
        {
            try
            {
                unit = UnitParser.Parse(s);
                return true;
            }
            catch
            {
                unit = null;
                return false;
            }
        }

        public virtual bool IsAddable(Unit u)
        {
            if (Power == u.Power && BaseUnit == u.BaseUnit && u is Unit)
            {
                return true;
            }
            return false;
        }

        public virtual Unit HasFactor(Unit unit, ref int factor)
        {
            if (unit.BaseUnit == this.BaseUnit)
            {
                factor = this.Power / unit.Power;
            }
            else factor = 0;
            return this;
        }

        public override string ToString()
        {
            return ToString("");
        }

        public virtual string ToString(string format)
        {
            format = format.ToLower();
            bool fancy = !format.Contains("c");
            return Str.UnitToString(Prefix, BaseUnit.ToString(), Power, fancy);
        }

        public virtual int DeterminePower10(Prefix prefix)
        {
            return Power * ((int)Prefix - (int)prefix);
        }

        public virtual Unit Pow(int power)
        {
            var powered = new Unit(Prefix, BaseUnit, Power * power);
            return powered;
        }

        public virtual IEnumerator<Unit> GetEnumerator()
        {
            yield return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
