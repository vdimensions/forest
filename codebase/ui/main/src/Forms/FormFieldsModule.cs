using Axle.Modularity;
using Forest.ComponentModel;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    [Module]
    public class FormFieldsModule : IForestViewProvider
    {
        public void RegisterViews(IForestViewRegistry registry)
        {
            registry
                .RegisterFormField<TextInputView, string>()
                .RegisterFormField<ByteInputView, byte>().RegisterFormField<NullableByteInputView, byte?>()
                .RegisterFormField<SByteInputView, sbyte>().RegisterFormField<NullableSByteInputView, sbyte?>()
                .RegisterFormField<Int16InputView, short>().RegisterFormField<NullableInt16InputView, short?>()
                .RegisterFormField<UInt16InputView, ushort>().RegisterFormField<NullableUInt16InputView, ushort?>()
                .RegisterFormField<Int32InputView, int>().RegisterFormField<NullableInt32InputView, int?>()
                .RegisterFormField<UInt32InputView, uint>().RegisterFormField<NullableUInt32InputView, uint?>()
                .RegisterFormField<Int64InputView, long>().RegisterFormField<NullableInt64InputView, long?>()
                .RegisterFormField<UInt64InputView, ulong>().RegisterFormField<NullableUInt64InputView, ulong?>()
                .RegisterFormField<SingleInputView, float>().RegisterFormField<NullableSingleInputView, float?>()
                .RegisterFormField<DoubleInputView, double>().RegisterFormField<NullableDoubleInputView, double?>()
                .RegisterFormField<DecimalInputView, decimal>().RegisterFormField<NullableDecimalInputView, decimal?>()
                ;
        }
    }
}