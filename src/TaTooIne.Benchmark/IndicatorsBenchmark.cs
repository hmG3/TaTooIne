using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TALib;
using Tulip;

namespace TaTooIne.Benchmark;

public class IndicatorsBenchmark
{
    private Random _random;

    private Memory<double> _openData;
    private Memory<double> _highData;
    private Memory<double> _lowData;
    private Memory<double> _closeData;
    private Memory<double> _volumeData;

    private Dictionary<int, double[][]> _inputsDic;
    private Dictionary<int, double[][]> _outputsDic;
    private Dictionary<int, double[]> _optionsDic;

    [Params(400000)] public int InputSize = 400000;

    [GlobalSetup]
    public void GlobalSetup()
    {
        PrepareMemory();
    }

    [Benchmark]
    [ArgumentsSource(nameof(TulipSource))]
    public void Tulip(Indicator indicator, int order)
    {
        var returnCode = indicator.Run(_inputsDic[order], _optionsDic[order], _outputsDic[order]);
        if (returnCode != 0)
        {
            throw new Exception("Return code not 0");
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(TALibSource))]
    public void TALib(Function indicator, int order)
    {
        var a = _inputsDic[order];
        var b = _optionsDic[order];
        var c = _outputsDic[order];

        /*var returnCode = indicator.Run(_inputsDic[order], _optionsDic[order], _outputsDic[order]);
        if (returnCode != 0)
        {
            throw new Exception("Return code not 0");
        }*/
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

    private void PrepareMemory()
    {
        _random = new Random(Environment.TickCount);

        _openData = new Memory<double>(new double[InputSize]);
        _highData = new Memory<double>(new double[InputSize]);
        _lowData = new Memory<double>(new double[InputSize]);
        _closeData = new Memory<double>(new double[InputSize]);
        _volumeData = new Memory<double>(new double[InputSize]);

        GenerateInputs(InputSize);

        var indicatorsCount = Indicators.All.Count();

        _inputsDic = new Dictionary<int, double[][]>(indicatorsCount);
        _outputsDic = new Dictionary<int, double[][]>(indicatorsCount);
        _optionsDic = new Dictionary<int, double[]>(indicatorsCount);

        for (var i = 0; i < indicatorsCount; i++)
        {
            var indicator = Indicators.All.ElementAt(i);

            var inputs = new double[indicator.Inputs.Length][];
            var outputs = new double[indicator.Outputs.Length][];
            var options = new double[indicator.Options.Length];

            for (var j = 0; j < indicator.Inputs.Length; j++)
            {
                var input = indicator.Inputs[j];
                switch (input)
                {
                    case "open":
                        if (MemoryMarshal.TryGetArray<double>(_openData, out var openDataArray))
                        {
                            inputs[j] = openDataArray.Array!;
                        }

                        break;
                    case "high":
                        if (MemoryMarshal.TryGetArray<double>(_highData, out var highDataArray))
                        {
                            inputs[j] = highDataArray.Array!;
                        }

                        break;
                    case "low":
                        if (MemoryMarshal.TryGetArray<double>(_lowData, out var lowDataArray))
                        {
                            inputs[j] = lowDataArray.Array!;
                        }

                        break;
                    case "close":
                    case "real":
                        if (MemoryMarshal.TryGetArray<double>(_closeData, out var closeDataArray))
                        {
                            inputs[j] = closeDataArray.Array!;
                        }

                        break;
                    case "volume":
                        if (MemoryMarshal.TryGetArray<double>(_volumeData, out var volumeDataArray))
                        {
                            inputs[j] = volumeDataArray.Array!;
                        }

                        break;
                }
            }

            for (var j = 0; j < indicator.Outputs.Length; j++)
            {
                outputs[j] = new double[InputSize];
            }

            _inputsDic.Add(i, inputs);
            _outputsDic.Add(i, outputs);
            _optionsDic.Add(i, options);
        }
    }

    private void GenerateInputs(int inputSize)
    {
        for (var i = 0; i < inputSize; ++i)
        {
            var diff1 = (_random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff2 = (_random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff3 = _random.NextDouble() * 0.5;
            var diff4 = _random.NextDouble() * 0.5;
            var vol = _random.NextDouble() * 10000 + 500;

            if (i != 0)
            {
                _openData.Span[i] = _openData.Span[i - 1] + diff1;
                _volumeData.Span[i] = vol;
            }

            _closeData.Span[i] = _openData.Span[i] + diff2;
            _highData.Span[i] = _openData.Span[i] > _closeData.Span[i]
                ? _openData.Span[i] + diff3
                : _closeData.Span[i] + diff3;
            _lowData.Span[i] = _openData.Span[i] < _closeData.Span[i]
                ? _openData.Span[i] - diff4
                : _closeData.Span[i] - diff4;
        }
    }
}
