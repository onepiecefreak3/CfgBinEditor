using System;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using Logic.Domain.Level5Management.Contract.DataClasses;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Veldrid;

namespace CfgBinEditor.Components
{
    public partial class RdbnValueComponent
    {
        private StackLayout _valueLayout;

        private void InitializeComponent(object[] values, RdbnFieldDeclaration fieldDeclaration)
        {
            _valueLayout = new StackLayout
            {
                Alignment = Alignment.Vertical,
                ItemSpacing = 5,
                Size = Size.WidthAlign
            };

            _valueLayout.Items.Add(new Label { Text = fieldDeclaration.Name });

            for (var i = 0; i < fieldDeclaration.Count; i++)
                _valueLayout.Items.Add(GetValueComponent(values, i, fieldDeclaration));
        }

        public override Size GetSize()
        {
            return _valueLayout.Size;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _valueLayout.Update(contentRect);
        }

        protected override void SetTabInactiveCore()
        {
            _valueLayout.SetTabInactive();
        }

        protected override int GetContentHeight(int parentHeight, float layoutCorrection = 1)
        {
            return _valueLayout.GetHeight(parentHeight, layoutCorrection);
        }

        protected override int GetContentWidth(int parentWidth, float layoutCorrection = 1)
        {
            return _valueLayout.GetWidth(parentWidth, layoutCorrection);
        }

        private Component GetValueComponent(object[] values, int index, RdbnFieldDeclaration fieldDeclaration)
        {
            Component result;

            switch (fieldDeclaration.FieldType)
            {
                case FieldType.AbilityData:
                case FieldType.EnhanceData:
                case FieldType.StatusRate:
                    var textBox2 = new TextBox { Text = GetValueText(values[index], fieldDeclaration.FieldType) };
                    textBox2.TextChanged += (s, e) => SetBytesValue(values, index, textBox2.Text);

                    result = textBox2;
                    break;

                case FieldType.Bool:
                    var checkbox = new CheckBox { Checked = (bool)values[index] };
                    checkbox.CheckChanged += (s, e) => SetBoolValue(values, index, checkbox.Checked);

                    result = checkbox;
                    break;

                case FieldType.Byte:
                case FieldType.Short:
                case FieldType.Int:
                case FieldType.ActType:
                case FieldType.Flag:
                case FieldType.Hash:
                    var textBox = new TextBox { Text = GetValueText(values[index], fieldDeclaration.FieldType) };
                    textBox.TextChanged += (s, e) => SetNumericValue(values, index, textBox.Text, fieldDeclaration.FieldType);

                    result = textBox;
                    break;

                case FieldType.Float:
                    var textBox1 = new TextBox { Text = GetValueText(values[index], fieldDeclaration.FieldType) };
                    textBox1.TextChanged += (s, e) => SetFloatValue(values, index, textBox1.Text);

                    result = textBox1;
                    break;

                case FieldType.RateMatrix:
                case FieldType.Position:
                    var layout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };

                    var valueArray = (float[])values[index];
                    for (var i = 0; i < 4; i++)
                    {
                        var textBox3 = new TextBox { Text = GetValueText(valueArray[i], FieldType.Float) };

                        int valueIndex = i;
                        textBox3.TextChanged += (s, e) => SetFloatValue(valueArray, valueIndex, textBox3.Text);

                        layout.Items.Add(textBox3);
                    }

                    result = layout;
                    break;

                case FieldType.String:
                    TextBox textBox4;

                    if (values[index] is string)
                    {
                        textBox4 = new TextBox { Text = GetValueText(values[index], fieldDeclaration.FieldType) };
                        textBox4.TextChanged += (s, e) => SetStringValue(values, index, textBox4.Text);
                    }
                    else
                    {
                        textBox4 = new TextBox { Text = GetValueText(values[index], fieldDeclaration.FieldType) };
                        textBox4.TextChanged += (s, e) => SetNumericValue(values, index, textBox4.Text, FieldType.Hash);
                    }

                    result = textBox4;
                    break;

                case FieldType.DataTuple:
                    var layout1 = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };

                    var valueArray1 = (short[])values[index];
                    for (var i = 0; i < 2; i++)
                    {
                        var textBox3 = new TextBox { Text = GetValueText(valueArray1[i], FieldType.Short) };

                        int valueIndex = i;
                        textBox3.TextChanged += (s, e) => SetShortValue(valueArray1, valueIndex, textBox3.Text);

                        layout1.Items.Add(textBox3);
                    }

                    result = layout1;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown field type {fieldDeclaration.FieldType}.");
            }

            return result;
        }

        public static string GetValueText(object value, FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.AbilityData:
                case FieldType.EnhanceData:
                case FieldType.StatusRate:
                    return $"{Convert.ToHexString((byte[])value)}";

                case FieldType.Byte:
                case FieldType.Short:
                case FieldType.Int:
                case FieldType.ActType:
                case FieldType.Flag:
                case FieldType.Float:
                case FieldType.Hash:
                    return $"{value}";

                case FieldType.String:
                    if (value is string stringValue)
                        return stringValue;

                    return $"{value}";

                default:
                    throw new InvalidOperationException($"Unknown field type {fieldType}.");
            }
        }
    }
}
