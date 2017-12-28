using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Handles transformation of boolean values.
    /// </summary>
    public class BooleanTransformer : ITypeTransformer<bool, byte>
    {
        /// <inheritdoc />
        public Type LowerType()
        {
            return typeof(byte);
        }

        /// <inheritdoc />
        public Type RaiseType()
        {
            return typeof(bool);
        }

        /// <inheritdoc />
        public byte LowerValue(bool value)
        {
            return value ? (byte)1 : (byte)0;
        }

        /// <inheritdoc />
        public bool RaiseValue(byte value)
        {
            return value > 0;
        }
    }
}
