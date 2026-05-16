using CfgBinEditor.Forms;
using CfgBinEditor.Messages;
using ImGui.Forms.Controls.Base;
using Logic.Domain.Level5Management.Contract.DataClasses;
using System;
using CrossCutting.Core.Contract.EventBrokerage;

namespace CfgBinEditor.Components
{
    public partial class RdbnValueComponent : Component
    {
        private readonly RdbnForm _parentForm;
        private readonly IEventBroker _eventBroker;

        public RdbnValueComponent(RdbnForm parentForm, object[] values, RdbnFieldDeclaration fieldDeclaration, IEventBroker eventBroker)
        {
            InitializeComponent(values, fieldDeclaration);

            _parentForm = parentForm;
            _eventBroker = eventBroker;
        }

        private void SetStringValue(object[] values, int index, string value)
        {
            values[index] = value;

            RaiseFileChanged();
        }

        private void SetBytesValue(object[] values, int index, string value)
        {
            try
            {
                byte[] bytes = Convert.FromHexString(value);
                values[index] = bytes;

                RaiseFileChanged();
            }
            catch
            {
                // Ignore exception
            }
        }

        private void SetBoolValue(object[] values, int index, bool value)
        {
            values[index] = value;

            RaiseFileChanged();
        }

        private void SetNumericValue(object[] values, int index, string value, FieldType fieldType)
        {
            if (!long.TryParse(value, out long numericValue))
                return;

            switch (fieldType)
            {
                case FieldType.Byte:
                    values[index] = (byte)Math.Clamp(numericValue, byte.MinValue, byte.MaxValue);
                    break;

                case FieldType.Short:
                    values[index] = (short)Math.Clamp(numericValue, short.MinValue, short.MaxValue);
                    break;

                case FieldType.Int:
                    values[index] = (int)Math.Clamp(numericValue, int.MinValue, int.MaxValue);
                    break;

                case FieldType.ActType:
                    values[index] = (short)Math.Clamp(numericValue, short.MinValue, short.MaxValue);
                    break;

                case FieldType.Flag:
                    values[index] = (int)Math.Clamp(numericValue, int.MinValue, int.MaxValue);
                    break;

                case FieldType.Hash:
                    values[index] = (uint)Math.Clamp(numericValue, uint.MinValue, uint.MaxValue);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown numeric type {fieldType}.");
            }

            RaiseFileChanged();
        }

        private void SetFloatValue(object[] values, int index, string value)
        {
            if (!float.TryParse(value, out float floatValue))
                return;

            values[index] = Math.Clamp(floatValue, float.MinValue, float.MaxValue);

            RaiseFileChanged();
        }

        private void SetFloatValue(float[] values, int index, string value)
        {
            if (!float.TryParse(value, out float floatValue))
                return;

            values[index] = Math.Clamp(floatValue, float.MinValue, float.MaxValue);

            RaiseFileChanged();
        }

        private void SetShortValue(short[] values, int index, string value)
        {
            if (!short.TryParse(value, out short shortValue))
                return;

            values[index] = Math.Clamp(shortValue, short.MinValue, short.MaxValue);

            RaiseFileChanged();
        }

        private void RaiseFileChanged()
        {
            _eventBroker.Raise(new FileChangedMessage(_parentForm));
        }
    }
}
