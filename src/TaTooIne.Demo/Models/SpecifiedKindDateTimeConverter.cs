using System.Globalization;
using TinyCsvParser.TypeConverter;

namespace TaTooIne.Demo.Models;

public class SpecifiedKindDateTimeConverter(
    string dateTimeFormat,
    IFormatProvider formatProvider,
    DateTimeStyles dateTimeStyles,
    DateTimeKind dateTimeKind)
    : ITypeConverter<DateTime>
{
    private readonly DateTimeConverter _dateTimeConverter = new(dateTimeFormat, formatProvider, dateTimeStyles);

    public SpecifiedKindDateTimeConverter(DateTimeKind dateTimeKind)
        : this(String.Empty, dateTimeKind)
    {
    }

    public SpecifiedKindDateTimeConverter(string dateTimeFormat, DateTimeKind dateTimeKind)
        : this(dateTimeFormat, CultureInfo.InvariantCulture, dateTimeKind)
    {
    }

    public SpecifiedKindDateTimeConverter(
        string dateTimeFormat,
        IFormatProvider formatProvider,
        DateTimeKind dateTimeKind)
        : this(dateTimeFormat, formatProvider, DateTimeStyles.None, dateTimeKind)
    {
    }

    public bool TryConvert(string value, out DateTime result)
    {
        if (_dateTimeConverter.TryConvert(value, out result))
        {
            result = DateTime.SpecifyKind(result, dateTimeKind);

            return true;
        }

        return false;
    }

    public Type TargetType => typeof(DateTime);
}
