using System;
using System.Globalization;
using TinyCsvParser.TypeConverter;

namespace TaTooIne.Demo.Models
{
    public class SpecifiedKindDateTimeConverter : ITypeConverter<DateTime>
    {
        private readonly DateTimeKind _dateTimeKind;

        private readonly DateTimeConverter _dateTimeConverter;

        public SpecifiedKindDateTimeConverter(DateTimeKind dateTimeKind)
            : this(string.Empty, dateTimeKind)
        {
        }

        public SpecifiedKindDateTimeConverter(string dateTimeFormat, DateTimeKind dateTimeKind)
            : this(dateTimeFormat, CultureInfo.InvariantCulture, dateTimeKind)
        {
        }

        public SpecifiedKindDateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider,
            DateTimeKind dateTimeKind)
            : this(dateTimeFormat, formatProvider, DateTimeStyles.None, dateTimeKind)
        {
        }

        public SpecifiedKindDateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider,
            DateTimeStyles dateTimeStyles, DateTimeKind dateTimeKind)
        {
            _dateTimeConverter = new DateTimeConverter(dateTimeFormat, formatProvider, dateTimeStyles);
            _dateTimeKind = dateTimeKind;
        }

        public bool TryConvert(string value, out DateTime result)
        {
            if (_dateTimeConverter.TryConvert(value, out result))
            {
                result = DateTime.SpecifyKind(result, _dateTimeKind);

                return true;
            }

            return false;
        }

        public Type TargetType => typeof(DateTime);
    }
}