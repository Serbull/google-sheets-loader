using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GoogleSheets.Values
{
    public class SheetDataValue
    {
        protected readonly string[,] _cells;

        public SheetDataValue(string[,] cells)
        {
            _cells = cells;
        }

        public Vector2Int FindKey(string key, bool errorIfNotFound = true)
        {
            return FindKey(key, Vector2Int.zero, Vector2Int.one * int.MaxValue, errorIfNotFound);
        }

        public Vector2Int FindKey(string key, Vector2Int rangeMin, Vector2Int rangeMax, bool errorIfNotFound = true)
        {
            rangeMax.x = Mathf.Min(rangeMax.x, _cells.GetUpperBound(0) - 1);
            rangeMax.y = Mathf.Min(rangeMax.y, _cells.GetUpperBound(1) - 1);

            for (int row = rangeMin.y; row <= rangeMax.y; row++)
            {
                for (int column = rangeMin.x; column <= rangeMax.x; column++)
                {
                    if (string.Equals(key, _cells[column, row]))
                    {
                        return new Vector2Int(column, row);
                    }
                }
            }

            if (errorIfNotFound)
            {
                Debug.LogError($"Not have cell with key: '{key}'");
            }

            return -Vector2Int.one;
        }

        public Vector2Int[] FindAllKeys(string key)
        {
            return FindAllKeys(key, Vector2Int.zero, int.MaxValue * Vector2Int.one);
        }

        public Vector2Int[] FindAllKeysInRow(string key, int row)
        {
            return FindAllKeys(key, new Vector2Int(0, row), new Vector2Int(int.MaxValue, row));
        }

        public Vector2Int[] FindAllKeys(string key, Vector2Int rangeMin, Vector2Int rangeMax)
        {
            List<Vector2Int> result = new();

            rangeMax.x = Mathf.Min(rangeMax.x, _cells.GetUpperBound(0) - 1);
            rangeMax.y = Mathf.Min(rangeMax.y, _cells.GetUpperBound(1) - 1);

            for (int row = rangeMin.y; row <= rangeMax.y; row++)
            {
                for (int column = rangeMin.x; column <= rangeMax.x; column++)
                {
                    if (string.Equals(key, _cells[column, row]))
                    {
                        result.Add(new Vector2Int(column, row));
                    }
                }
            }

            return result.ToArray();
        }


        public Vector2Int[] GetArrayVertical(string key, bool breakOnNull = true)
        {
            return GetArrayVertical(FindKey(key), int.MaxValue, breakOnNull);
        }

        public Vector2Int[] GetArrayVertical(string key, int cellMaxY, bool breakOnNull = true)
        {
            return GetArrayVertical(FindKey(key), cellMaxY, breakOnNull);
        }

        public Vector2Int[] GetArrayVertical(Vector2Int startCell, bool breakOnNull = true)
        {
            return GetArrayVertical(startCell, int.MaxValue, breakOnNull);
        }

        public Vector2Int[] GetArrayVertical(Vector2Int startCell, int cellMaxY, bool breakOnNull = true)
        {
            var result = new List<Vector2Int>();

            cellMaxY = Mathf.Min(cellMaxY, _cells.GetUpperBound(1) - 1);

            for (int i = startCell.y + 1; i <= cellMaxY; i++)
            {
                string item = _cells[startCell.x, i];
                if (string.IsNullOrEmpty(item))
                {
                    if (breakOnNull)
                        break;
                    else
                        continue;
                }

                result.Add(new Vector2Int(startCell.x, i));
            }

            return result.ToArray();
        }

        public Vector2Int[] GetArrayHorizontal(string key, bool breakOnNull = true)
        {
            return GetArrayHorizontal(FindKey(key), breakOnNull);
        }

        public Vector2Int[] GetArrayHorizontal(Vector2Int startCell, bool breakOnNull = true)
        {
            var result = new List<Vector2Int>();

            for (int i = startCell.x + 1; i < _cells.GetUpperBound(0); i++)
            {
                string item = _cells[i, startCell.y];
                if (string.IsNullOrEmpty(item))
                {
                    if (breakOnNull)
                        break;
                    else
                        continue;
                }

                result.Add(new Vector2Int(i, startCell.y));
            }

            return result.ToArray();
        }
    }

    public abstract class SheetDataValue<T> : SheetDataValue
    {
        public struct RangeValue
        {
            public T value;
            public Vector2Int startCell;
            public Vector2Int endCell;

            public RangeValue(T value, Vector2Int startCell, Vector2Int endCell)
            {
                this.value = value;
                this.startCell = startCell;
                this.endCell = endCell;
            }
        }

        public struct Block
        {
            public T[] Values;
            public Vector2Int StartCell;
            public Vector2Int EndCell;

            public Block(T[] values, Vector2Int startCell, Vector2Int endCell)
            {
                Values = values;
                StartCell = startCell;
                EndCell = endCell;
            }
        }

        public SheetDataValue(string[,] cells) : base(cells) { }

        public abstract T Parse(string str);

        public T GetValue(Vector2Int cell)
        {
            return Parse(_cells[cell.x, cell.y]);
        }

        public T GetValueVertical(string key)
        {
            return GetValue(FindKey(key) + Vector2Int.up);
        }

        public T GetValueHorizontal(string key)
        {
            return GetValue(FindKey(key) + Vector2Int.right);
        }

        public T GetValueHorizontal(string key, Vector2Int rangeMin, Vector2Int rangeMax)
        {
            return GetValue(FindKey(key, rangeMin, rangeMax) + Vector2Int.right);
        }

        #region ARRAY_VERTICAL

        public new T[] GetArrayVertical(string key, bool breakOnNull = true)
        {
            return GetArrayVertical(FindKey(key), breakOnNull);
        }

        public T[] GetArrayVertical(string key, Vector2Int rangeMin, Vector2Int rangeMax, bool breakOnNull = true)
        {
            return GetArrayVertical(FindKey(key, rangeMin, rangeMax), rangeMax.y, breakOnNull);
        }

        public new T[] GetArrayVertical(Vector2Int startCell, bool breakOnNull = true)
        {
            return GetArrayVertical(startCell, int.MaxValue, breakOnNull);
        }

        public new T[] GetArrayVertical(Vector2Int startCell, int cellMaxY, bool breakOnNull = true)
        {
            var cells = base.GetArrayVertical(startCell, cellMaxY, breakOnNull);
            var result = new T[cells.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parse(_cells[cells[i].x, cells[i].y]);
            }

            return result;
        }

        #endregion

        #region ARRAY_HORIZONTAL

        public T[] GetArrayHorizontal(string key, bool breakOnNull = true, bool errorIfNotFoundKey = true)
        {
            return GetArrayHorizontal(key, Vector2Int.zero, Vector2Int.one * int.MaxValue, breakOnNull, errorIfNotFoundKey);
        }

        public T[] GetArrayHorizontal(string key, Vector2Int rangeMin, Vector2Int rangeMax, bool breakOnNull = true, bool errorIfNotFoundKey = true)
        {
            var startCell = FindKey(key, rangeMin, rangeMax, errorIfNotFoundKey);
            if (startCell == -Vector2Int.one)
            {
                return null;
            }

            return GetArrayHorizontal(startCell, breakOnNull);
        }

        public new T[] GetArrayHorizontal(Vector2Int startCell, bool breakOnNull = true)
        {
            var cells = base.GetArrayHorizontal(startCell, breakOnNull);
            var result = new T[cells.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parse(_cells[cells[i].x, cells[i].y]);
            }

            return result;
        }

        public T[] GetAllValuesHorizontal(string key)
        {
            var cells = FindAllKeys(key);
            var result = new T[cells.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parse(_cells[cells[i].x + 1, cells[i].y]);
            }

            return result;
        }

        public T[] GetAllValuesHorizontal(Vector2Int[] keyCells)
        {
            var result = new T[keyCells.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Parse(_cells[keyCells[i].x + 1, keyCells[i].y]);
            }

            return result;
        }

        #endregion

        public RangeValue[] GetRangeArrayVertical(string key)
        {
            var cells = base.GetArrayVertical(key, false);
            var result = new RangeValue[cells.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var endCell = i < result.Length - 1 ? cells[i + 1] - Vector2Int.up : new Vector2Int(cells[i].x, _cells.GetUpperBound(1) - 1);
                result[i] = new(Parse(_cells[cells[i].x, cells[i].y]), cells[i], endCell);
            }

            return result;
        }

        #region BLOCKS

        /// <summary>
        /// |      |  "HP"  |       | > one key
        /// |______|________|_______|
        /// |      |        |       | 
        /// |______|________|_______|
        /// |      |  100   |       | 
        /// |______|________|_______| > 1st block
        /// |      |  200   |       | 
        /// |______|________|_______| 
        /// |      |        |       | > empty field between blocks
        /// |______|________|_______| 
        /// |      |  350   |       |
        /// |______|________|_______| > 2nd block
        /// |      |  425   |       | 
        /// |______|________|_______|
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rangeMin"></param>
        /// <param name="rangeMax"></param>
        /// <param name="errorIfNotFoundKey"></param>
        /// <returns></returns>
        public Block[] GetBlockArrayVertical(string key, Vector2Int rangeMin, Vector2Int rangeMax, bool errorIfNotFoundKey)
        {
            var cells = base.GetArrayVertical(FindKey(key, rangeMin, rangeMax, errorIfNotFoundKey), rangeMax.y, false);

            var result = new List<Block>();

            for (int i = 0; i < cells.Length - 1;)
            {
                var blockLength = 1;
                var values = new List<T>() { GetValue(cells[i]) };

                for (int y = i + 1; y < cells.Length; y++)
                {
                    if (cells[y].y == cells[i].y + blockLength)
                    {
                        values.Add(GetValue(cells[y]));
                        blockLength++;
                        continue;
                    }

                    i += blockLength;
                    break;
                }

                result.Add(new Block(values.ToArray(), cells[i], cells[blockLength]));
            }

            return result.ToArray();
        }

        #endregion

    }

    public sealed class SheetDataInt : SheetDataValue<int>
    {
        public SheetDataInt(string[,] cells) : base(cells) { }

        public override int Parse(string str)
        {
            str = str.Replace(" ", "");

            if (int.TryParse(str, out int result))
                return result;

            Debug.LogError("Cannot parse to int: " + str);
            return 0;
        }
    }

    public sealed class SheetDataFloat : SheetDataValue<float>
    {
        public SheetDataFloat(string[,] cells) : base(cells) { }

        public override float Parse(string str)
        {
            str = str.Replace(" ", "").Replace(",", ".");

            if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;

            Debug.LogError("Cannot parse to float: " + str);
            return 0;
        }
    }

    public sealed class SheetDataString : SheetDataValue<string>
    {
        public SheetDataString(string[,] cells) : base(cells) { }

        public override string Parse(string str)
        {
            return str;
        }
    }
}
