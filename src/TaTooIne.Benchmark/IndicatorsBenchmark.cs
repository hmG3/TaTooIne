using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TALib;
using Tulip;

namespace TaTooIne.Benchmark;

/// <remarks>
/// Greatly inspired by https://github.com/TulipCharts/tulipindicators/blob/master/benchmark.c
/// </remarks>
public class IndicatorsBenchmark
{
    private IMemoryOwner<double> _inputsMemoryPool = null!;

    private Dictionary<int, double[][]> _tulipInputs = null!;
    private Dictionary<int, double[]> _tulipOptions = null!;
    private Dictionary<int, double[][]> _tulipOutputs = null!;

    private Dictionary<int, double[][]> _talibInputs = null!;
    private Dictionary<int, double[]> _talibOptions = null!;
    private Dictionary<int, double[][]> _talibOutputs = null!;

    private int _tulipPeriodOption = 3;
    private int _talibPeriodOption = 3;

    [Params(210, 2100, 21000)] public int InputSize;

    [GlobalSetup(Target = "Tulip")]
    public void GlobalTulipSetup()
    {
        GenerateInputs();
        SetupTulipInputs();
    }

    [IterationSetup(Target = "Tulip")]
    public void TulipIterationSetup()
    {
        _tulipPeriodOption++;

        var currentIndicator =
            (Indicator) GetType().GetField("__argField0", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(this)!;
        var currentIndicatorIndex =
            (int) GetType().GetField("__argField1", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(this)!;
        var currentIndicatorOptions = _tulipOptions[currentIndicatorIndex];
        if (currentIndicatorOptions.Length == 0)
        {
            return;
        }

        currentIndicatorOptions[0] = _talibPeriodOption;
        switch (currentIndicator.Name.ToLower())
        {
            case "adosc":
            case "apo":
            case "kvo":
            case "ppo":
            case "vosc":
            case "macd":
            case "vidya":
                currentIndicatorOptions[1] = _tulipPeriodOption + 10;
                switch (currentIndicator.Name.ToLower())
                {
                    case "macd":
                        currentIndicatorOptions[2] = _tulipPeriodOption + 1;
                        break;
                    case "vidya":
                        currentIndicatorOptions[2] = 0.2;
                        break;
                }

                break;
            case "bbands":
                currentIndicatorOptions[1] = 1;
                break;
            case "mama":
                currentIndicatorOptions[0] = 0.5;
                currentIndicatorOptions[1] = 0.05;
                break;
            case "psar":
                currentIndicatorOptions[0] = 1.0 / _tulipPeriodOption;
                currentIndicatorOptions[1] = currentIndicatorOptions[0] * 10;
                break;
            case "stoch":
                currentIndicatorOptions[1] = 3;
                currentIndicatorOptions[2] = 4;
                break;
            case "ultosc":
                currentIndicatorOptions[1] = _tulipPeriodOption * 2;
                currentIndicatorOptions[2] = _tulipPeriodOption * 4;
                break;
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(TulipSource))]
    public void Tulip(Indicator Indicator, int order)
    {
        var returnCode = Indicator.Run(_tulipInputs[order], _tulipOptions[order], _tulipOutputs[order]);
        if (returnCode != 0)
        {
            throw new Exception("Return code not 0");
        }
    }

    public static IEnumerable<object[]> TulipSource() =>
        Indicators.All.Select((indicator, index) => new object[] { indicator, index });

    [GlobalCleanup(Target = "Tulip")]
    public void GlobalTulipCleanup() => _inputsMemoryPool.Dispose();

    [GlobalSetup(Target = "TALib")]
    public void GlobalTALibSetup()
    {
        GenerateInputs();
        SetupTALib();
        SetupTALibInputs();
    }

    [IterationSetup(Target = "TALib")]
    public void TALibIterationSetup()
    {
        _talibPeriodOption++;

        var currentFunction =
            (Function) GetType().GetField("__argField0", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(this)!;
        var currentFunctionIndex =
            (int) GetType().GetField("__argField1", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(this)!;
        var currentFunctionOptions = _talibOptions[currentFunctionIndex];
        if (currentFunctionOptions.Length == 0)
        {
            return;
        }

        currentFunctionOptions[0] = _talibPeriodOption;
        switch (currentFunction.Name.ToLower())
        {
            case "adosc":
            case "kvo":
            case "apo":
            case "ppo":
            case "macd":
            case "mavp":
                currentFunctionOptions[1] = _talibPeriodOption + 10;
                switch (currentFunction.Name.ToLower())
                {
                    case "apo":
                    case "ppo":
                        currentFunctionOptions[2] = (int) Core.MAType.Ema;
                        break;
                    case "macd":
                        currentFunctionOptions[2] = _talibPeriodOption + 1;
                        break;
                }

                break;
            case "bbands":
                currentFunctionOptions[1] = 1.0;
                currentFunctionOptions[2] = 1.0;
                break;
            case "macdext":
                currentFunctionOptions[2] = _talibPeriodOption + 10;
                currentFunctionOptions[4] = _talibPeriodOption + 1;
                break;
            case "macdfix":
                currentFunctionOptions[0] = _talibPeriodOption + 1;
                break;
            case "mama":
                currentFunctionOptions[0] = 0.5;
                currentFunctionOptions[1] = 0.05;
                break;
            case "sar":
                currentFunctionOptions[0] = 1.0 / _talibPeriodOption;
                currentFunctionOptions[1] = currentFunctionOptions[0] * 10;
                break;
            case "stoch":
                currentFunctionOptions[1] = 3;
                currentFunctionOptions[3] = 4;
                break;
            case "stochf":
                currentFunctionOptions[1] = 3;
                break;
            case "stochrsi":
                currentFunctionOptions[1] = _talibPeriodOption;
                currentFunctionOptions[2] = _talibPeriodOption;
                currentFunctionOptions[3] = 1;
                break;
            case "ultosc":
                currentFunctionOptions[1] = _talibPeriodOption * 2;
                currentFunctionOptions[2] = _talibPeriodOption * 4;
                break;
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(TALibSource))]
    public void TALib(Function Indicator, int order)
    {
        var returnCode = Indicator.Run(_talibInputs[order], _talibOptions[order], _talibOutputs[order]);
        if (returnCode != Core.RetCode.Success)
        {
            throw new Exception("Return code not 0");
        }
    }

    public static IEnumerable<object[]> TALibSource() =>
        Functions.All.Select((function, index) => new object[] { function, index });

    [GlobalCleanup(Target = "TALib")]
    public void GlobalTALibCleanup() => _inputsMemoryPool.Dispose();

    private void GenerateInputs()
    {
        var random = new Random(Environment.TickCount);

        _inputsMemoryPool = MemoryPool<double>.Shared.Rent(InputSize * 5);

        var inputsMemory = _inputsMemoryPool.Memory.Span;

        var openDataOffset = InputSize * 0;
        var highDataOffset = InputSize * 1;
        var lowDataOffset = InputSize * 2;
        var closeDataOffset = InputSize * 3;
        var volumeDataOffset = InputSize * 4;

        inputsMemory[openDataOffset] = 100;

        for (var i = 0; i < InputSize; ++i)
        {
            var diff1 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff2 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff3 = random.NextDouble() * 0.5;
            var diff4 = random.NextDouble() * 0.5;
            var vol = random.NextDouble() * 10000 + 500;

            if (i > 0)
            {
                inputsMemory[openDataOffset + i] = inputsMemory[openDataOffset + i - 1] + diff1;
            }

            inputsMemory[closeDataOffset + i] = inputsMemory[openDataOffset + i] + diff2;

            inputsMemory[highDataOffset + i] =
                inputsMemory[openDataOffset + i] > inputsMemory[closeDataOffset + i]
                    ? inputsMemory[openDataOffset + i] + diff3
                    : inputsMemory[closeDataOffset + i] + diff3;

            inputsMemory[lowDataOffset + i] =
                inputsMemory[openDataOffset + i] < inputsMemory[closeDataOffset + i]
                    ? inputsMemory[openDataOffset + i] - diff4
                    : inputsMemory[closeDataOffset + i] - diff4;

            inputsMemory[volumeDataOffset + i] = vol;

            Trace.Assert(inputsMemory[openDataOffset + i] <= inputsMemory[highDataOffset + i]);
            Trace.Assert(inputsMemory[closeDataOffset + i] <= inputsMemory[highDataOffset + i]);

            Trace.Assert(inputsMemory[openDataOffset + i] >= inputsMemory[lowDataOffset + i]);
            Trace.Assert(inputsMemory[closeDataOffset + i] >= inputsMemory[lowDataOffset + i]);

            Trace.Assert(inputsMemory[highDataOffset + i] >= inputsMemory[lowDataOffset + i]);
            Trace.Assert(inputsMemory[highDataOffset + i] >= inputsMemory[openDataOffset + i]);
            Trace.Assert(inputsMemory[highDataOffset + i] >= inputsMemory[closeDataOffset + i]);

            Trace.Assert(inputsMemory[lowDataOffset + i] <= inputsMemory[openDataOffset + i]);
            Trace.Assert(inputsMemory[lowDataOffset + i] <= inputsMemory[closeDataOffset + i]);
        }

        // HACK: obv uses volume[0] as starting value and ti obv uses 0 as starting value.
        inputsMemory[volumeDataOffset] = 0;
    }

    private void SetupTulipInputs()
    {
        var indicatorsCount = Indicators.All.Count();

        _tulipInputs = new Dictionary<int, double[][]>(indicatorsCount);
        _tulipOptions = new Dictionary<int, double[]>(indicatorsCount);
        _tulipOutputs = new Dictionary<int, double[][]>(indicatorsCount);

        for (var i = 0; i < indicatorsCount; i++)
        {
            var indicator = Indicators.All.ElementAt(i);

            var (inputs, options, outputs) = PrepareParams(indicator);
            _tulipInputs.Add(i, inputs);
            _tulipOptions.Add(i, options);
            _tulipOutputs.Add(i, outputs);
        }
    }

    private void SetupTALib()
    {
        var candleDefaultConfig = new
        {
            Period = 10,

            BodyNone = 0.05,
            BodyShort = 0.5,
            BodyLong = 1.4,

            WickNone = 0.05,
            WickLong = 0.6,

            Near = 0.3
        };

        Core.SetCandleSettings(
            Core.CandleSettingType.BodyDoji, Core.RangeType.HighLow, candleDefaultConfig.Period,
            candleDefaultConfig.BodyNone);

        Core.SetCandleSettings(
            Core.CandleSettingType.BodyShort, Core.RangeType.RealBody, candleDefaultConfig.Period,
            candleDefaultConfig.BodyShort);

        Core.SetCandleSettings(
            Core.CandleSettingType.BodyLong, Core.RangeType.RealBody, candleDefaultConfig.Period,
            candleDefaultConfig.BodyLong);

        Core.SetCandleSettings(
            Core.CandleSettingType.ShadowVeryShort, Core.RangeType.HighLow, candleDefaultConfig.Period,
            candleDefaultConfig.WickNone);

        Core.SetCandleSettings(
            Core.CandleSettingType.ShadowLong, Core.RangeType.RealBody, candleDefaultConfig.Period,
            candleDefaultConfig.WickLong);

        Core.SetCandleSettings(
            Core.CandleSettingType.Near, Core.RangeType.HighLow, candleDefaultConfig.Period,
            candleDefaultConfig.Near);
    }

    private void SetupTALibInputs()
    {
        var functionsCount = Functions.All.Count();

        _talibInputs = new Dictionary<int, double[][]>(functionsCount);
        _talibOptions = new Dictionary<int, double[]>(functionsCount);
        _talibOutputs = new Dictionary<int, double[][]>(functionsCount);

        for (var i = 0; i < functionsCount; i++)
        {
            var function = Functions.All.ElementAt(i);

            var (inputs, options, outputs) = PrepareParams(function);
            _talibInputs.Add(i, inputs);
            _talibOptions.Add(i, options);
            _talibOutputs.Add(i, outputs);
        }
    }

    private (double[][] inputs, double[] options, double[][] outputs) PrepareParams(dynamic indicator)
    {
        var inputs = new double[indicator.Inputs.Length][];
        var outputs = new double[indicator.Outputs.Length][];
        var options = new double[indicator.Options.Length];

        var inputsMemory = _inputsMemoryPool.Memory;
        var openDataPointer = InputSize * 0;
        var highDataPointer = InputSize * 1;
        var lowDataPointer = InputSize * 2;
        var closeDataPointer = InputSize * 3;
        var volumeDataPointer = InputSize * 4;

        for (var j = 0; j < indicator.Inputs.Length; j++)
        {
            var input = indicator.Inputs[j];
            switch (input.ToLower())
            {
                case "open":
                    if (MemoryMarshal.TryGetArray<double>(inputsMemory.Slice(openDataPointer, InputSize),
                            out var openDataArray))
                    {
                        inputs[j] = openDataArray.AsSpan().ToArray();
                    }

                    break;
                case "high":
                    if (MemoryMarshal.TryGetArray<double>(inputsMemory.Slice(highDataPointer, InputSize),
                            out var highDataArray))
                    {
                        inputs[j] = highDataArray.AsSpan().ToArray();
                    }

                    break;
                case "low":
                    if (MemoryMarshal.TryGetArray<double>(inputsMemory.Slice(lowDataPointer, InputSize),
                            out var lowDataArray))
                    {
                        inputs[j] = lowDataArray.AsSpan().ToArray();
                    }

                    break;
                case "close":
                case "real":
                    if (MemoryMarshal.TryGetArray<double>(inputsMemory.Slice(closeDataPointer, InputSize),
                            out var closeDataArray))
                    {
                        inputs[j] = closeDataArray.AsSpan().ToArray();
                    }

                    break;
                case "volume":
                    if (MemoryMarshal.TryGetArray<double>(inputsMemory.Slice(volumeDataPointer, InputSize),
                            out var volumeDataArray))
                    {
                        inputs[j] = volumeDataArray.AsSpan().ToArray();
                    }

                    break;
                case "periods":
                    var inputData = new double[InputSize];
                    var random = new Random();
                    for (var i = 0; i < InputSize; i++)
                    {
                        inputData[i] = random.Next(30);
                    }
                    inputs[j] = inputData;
                    break;
            }
        }

        for (var j = 0; j < indicator.Outputs.Length; j++)
        {
            outputs[j] = new double[InputSize];
        }

        return (inputs, options, outputs);
    }
}
