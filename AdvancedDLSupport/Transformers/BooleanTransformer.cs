using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Handles transformation of boolean values.
    /// </summary>
    public class BooleanTransformer : ITypeTransformer<bool, int>
    {
        /// <inheritdoc />
        public Type LowerType()
        {
            return typeof(int);
        }

        /// <inheritdoc />
        public Type RaiseType()
        {
            return typeof(bool);
        }

        /// <inheritdoc />
        public int LowerValue(bool value)
        {
            return value ? 1 : 0;
        }

        /// <inheritdoc />
        public bool RaiseValue(int value)
        {
            return value > 0;
        }
    }
}
