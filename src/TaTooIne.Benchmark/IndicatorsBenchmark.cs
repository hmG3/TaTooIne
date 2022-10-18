using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TALib;
using Tulip;

namespace TaTooIne.Benchmark;

public class IndicatorsBenchmark
{
    private Memory<double> _openDataMemory;
    private Memory<double> _highDataMemory;
    private Memory<double> _lowDataMemory;
    private Memory<double> _closeDataMemory;
    private Memory<double> _volumeDataMemory;

    private Dictionary<int, double[][]> _tulipInputsDic;
    private Dictionary<int, double[]> _tulipOptionsDic;
    private Dictionary<int, double[][]> _tulipOutputsDic;

    private Dictionary<int, double[][]> _talibInputsDic;
    private Dictionary<int, double[]> _talibOptionsDic;
    private Dictionary<int, double[][]> _talibOutputsDic;

    [Params(400000)] public int InputSize;

    [GlobalSetup(Target = "Tulip")]
    public void GlobalTulipSetup()
    {
        GenerateInputs();
        SetupTulipInputs();
    }

    [GlobalSetup(Target = "TALib")]
    public void GlobalTALibSetup()
    {
        GenerateInputs();
        SetupTalibInputs();
    }

    [Benchmark]
    [ArgumentsSource(nameof(TulipSource))]
    public void Tulip(Indicator indicator, int order)
    {
        var returnCode = indicator.Run(_tulipInputsDic[order], _tulipOptionsDic[order], _tulipOutputsDic[order]);
        if (returnCode != 0)
        {
            throw new Exception("Return code not 0");
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(TALibSource))]
    public void TALib(Function indicator, int order)
    {
        var returnCode = indicator.Run(_talibInputsDic[order], _talibOptionsDic[order], _talibOutputsDic[order]);
        if (returnCode != Core.RetCode.Success)
        {
            throw new Exception("Return code not 0");
        }
    }

    public IEnumerable<object[]> TulipSource()
    {
        return Indicators.All
            //.Where(indicator => !indicator.Name.Equals("msw", StringComparison.OrdinalIgnoreCase))
            .Select((indicator, index) => new object[] { indicator, index });
    }

    public IEnumerable<object[]> TALibSource()
    {
        return Functions.All.Select((function, index) => new object[] { function, index });
    }

    private void GenerateInputs()
    {
        var random = new Random(Environment.TickCount);

        _openDataMemory = new Memory<double>(new double[InputSize]);
        _highDataMemory = new Memory<double>(new double[InputSize]);
        _lowDataMemory = new Memory<double>(new double[InputSize]);
        _closeDataMemory = new Memory<double>(new double[InputSize]);
        _volumeDataMemory = new Memory<double>(new double[InputSize]);

        var openDataSpan = _openDataMemory.Span;
        var highDataSpan = _highDataMemory.Span;
        var lowDataSpan = _lowDataMemory.Span;
        var closeDataSpan = _closeDataMemory.Span;
        var volumeDataSpan = _volumeDataMemory.Span;

        for (var i = 0; i < InputSize; ++i)
        {
            var diff1 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff2 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff3 = random.NextDouble() * 0.5;
            var diff4 = random.NextDouble() * 0.5;
            var vol = random.NextDouble() * 10000 + 500;

            if (i != 0)
            {
                openDataSpan[i] = openDataSpan[i - 1] + diff1;
                volumeDataSpan[i] = vol;
            }

            closeDataSpan[i] = openDataSpan[i] + diff2;
            highDataSpan[i] = openDataSpan[i] > closeDataSpan[i]
                ? openDataSpan[i] + diff3
                : closeDataSpan[i] + diff3;
            lowDataSpan[i] = openDataSpan[i] < closeDataSpan[i]
                ? openDataSpan[i] - diff4
                : closeDataSpan[i] - diff4;
        }
    }

    private void SetupTulipInputs()
    {
        var indicatorsCount = Indicators.All.Count();

        _tulipInputsDic = new Dictionary<int, double[][]>(indicatorsCount);
        _tulipOptionsDic = new Dictionary<int, double[]>(indicatorsCount);
        _tulipOutputsDic = new Dictionary<int, double[][]>(indicatorsCount);

        for (var i = 0; i < indicatorsCount; i++)
        {
            var indicator = Indicators.All.ElementAt(i);

            PrepareInputs(indicator, i, _tulipInputsDic, _tulipOptionsDic, _tulipOutputsDic);
        }
    }

    private void SetupTalibInputs()
    {
        var functionsCount = Functions.All.Count();

        _talibInputsDic = new Dictionary<int, double[][]>(functionsCount);
        _talibOptionsDic = new Dictionary<int, double[]>(functionsCount);
        _talibOutputsDic = new Dictionary<int, double[][]>(functionsCount);

        for (var i = 0; i < functionsCount; i++)
        {
            var function = Functions.All.ElementAt(i);

            PrepareInputs(function, i, _talibInputsDic, _talibOptionsDic, _talibOutputsDic);
        }
    }

    private void PrepareInputs(dynamic indicator,
        int indicatorIndex,
        IDictionary<int, double[][]> inputsDic,
        IDictionary<int, double[]> optionsDic, IDictionary<int, double[][]> outputsDic)
    {
        var inputs = new double[indicator.Inputs.Length][];
        var outputs = new double[indicator.Outputs.Length][];
        var options = new double[indicator.Options.Length];

        for (var j = 0; j < indicator.Inputs.Length; j++)
        {
            var input = indicator.Inputs[j];
            switch (input.ToLower())
            {
                case "open":
                    if (MemoryMarshal.TryGetArray<double>(_openDataMemory, out var openDataArray))
                    {
                        inputs[j] = openDataArray.Array;
                    }

                    break;
                case "high":
                    if (MemoryMarshal.TryGetArray<double>(_highDataMemory, out var highDataArray))
                    {
                        inputs[j] = highDataArray.Array;
                    }

                    break;
                case "low":
                    if (MemoryMarshal.TryGetArray<double>(_lowDataMemory, out var lowDataArray))
                    {
                        inputs[j] = lowDataArray.Array;
                    }

                    break;
                case "close":
                case "real":
                    if (MemoryMarshal.TryGetArray<double>(_closeDataMemory, out var closeDataArray))
                    {
                        inputs[j] = closeDataArray.Array;
                    }

                    break;
                case "volume":
                    if (MemoryMarshal.TryGetArray<double>(_volumeDataMemory, out var volumeDataArray))
                    {
                        inputs[j] = volumeDataArray.Array;
                    }

                    break;
            }
        }

        for (var j = 0; j < indicator.Outputs.Length; j++)
        {
            outputs[j] = new double[InputSize];
        }

        inputsDic.Add(indicatorIndex, inputs);
        optionsDic.Add(indicatorIndex, options);
        outputsDic.Add(indicatorIndex, outputs);
    }
}
