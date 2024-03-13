using TinyCsvParser.Mapping;

namespace TaTooIne.Demo.Models;

public class CandleCsvMapping : CsvMapping<Candle>
{
    public CandleCsvMapping()
    {
        MapProperty(0, x => x.Time, new SpecifiedKindDateTimeConverter(DateTimeKind.Utc));
        MapProperty(1, x => x.Open);
        MapProperty(2, x => x.High);
        MapProperty(3, x => x.Low);
        MapProperty(4, x => x.Close);
        MapProperty(5, x => x.Volume);
    }
}
