using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TALib;
using Tulip;

namespace TaTooIne.Benchmark;

public class IndicatorsBenchmark
{
    private IMemoryOwner<double> _inputsMemoryPool = null!;

    private Dictionary<int, double[][]> _tulipInputs = null!;
    private Dictionary<int, double[]> _tulipOptions = null!;
    private Dictionary<int, double[][]> _tulipOutputs = null!;

    private Dictionary<int, double[][]> _talibInputs = null!;
    private Dictionary<int, double[]> _talibOptions = null!;
    private Dictionary<int, double[][]> _talibOutputs = null!;

    [Params(21000)] public int InputSize;

    [GlobalSetup(Target = "Tulip")]
    public void GlobalTulipSetup()
    {
        GenerateInputs();
        SetupTulipInputs();
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
        Indicators.All
            //.Where(indicator => !indicator.Name.Equals("msw", StringComparison.OrdinalIgnoreCase))
            .Select((indicator, index) => new object[] { indicator, index });

    [GlobalCleanup(Target = "Tulip")]
    public void GlobalTulipCleanup()
    {
        _inputsMemoryPool.Dispose();
    }

    [GlobalSetup(Target = "TALib")]
    public void GlobalTALibSetup()
    {
        GenerateInputs();
        SetupTALibInputs();
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
    public void GlobalTALibCleanup()
    {
        _inputsMemoryPool.Dispose();
    }

    private void GenerateInputs()
    {
        var random = new Random(Environment.TickCount);

        _inputsMemoryPool = MemoryPool<double>.Shared.Rent(InputSize * 5);

        var inputsMemorySpan = _inputsMemoryPool.Memory.Span;

        var openDataOffset = InputSize * 0;
        var highDataOffset = InputSize * 1;
        var lowDataOffset = InputSize * 2;
        var closeDataOffset = InputSize * 3;
        var volumeDataOffset = InputSize * 4;

        inputsMemorySpan[openDataOffset] = 100;
        
        for (var i = 0; i < InputSize; ++i)
        {
            var diff1 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff2 = (random.NextDouble() - 0.5 + 0.01) * 2.5;
            var diff3 = random.NextDouble() * 0.5;
            var diff4 = random.NextDouble() * 0.5;
            var vol = random.NextDouble() * 10000 + 500;

            if (i > 0)
            {
                inputsMemorySpan[openDataOffset + i] = inputsMemorySpan[openDataOffset + i - 1] + diff1;
            }

            inputsMemorySpan[closeDataOffset + i] = inputsMemorySpan[openDataOffset + i] + diff2;

            inputsMemorySpan[highDataOffset + i] =
                inputsMemorySpan[openDataOffset + i] > inputsMemorySpan[closeDataOffset + i]
                    ? inputsMemorySpan[openDataOffset + i] + diff3
                    : inputsMemorySpan[closeDataOffset + i] + diff3;

            inputsMemorySpan[lowDataOffset + i] =
                inputsMemorySpan[openDataOffset + i] < inputsMemorySpan[closeDataOffset + i]
                    ? inputsMemorySpan[openDataOffset + i] - diff4
                    : inputsMemorySpan[closeDataOffset + i] - diff4;

            inputsMemorySpan[volumeDataOffset + i] = vol;

            Trace.Assert(inputsMemorySpan[openDataOffset + i] <= inputsMemorySpan[highDataOffset + i]);
            Trace.Assert(inputsMemorySpan[closeDataOffset + i] <= inputsMemorySpan[highDataOffset + i]);

            Trace.Assert(inputsMemorySpan[openDataOffset + i] >= inputsMemorySpan[lowDataOffset + i]);
            Trace.Assert(inputsMemorySpan[closeDataOffset + i] >= inputsMemorySpan[lowDataOffset + i]);

            Trace.Assert(inputsMemorySpan[highDataOffset + i] >= inputsMemorySpan[lowDataOffset + i]);
            Trace.Assert(inputsMemorySpan[highDataOffset + i] >= inputsMemorySpan[openDataOffset + i]);
            Trace.Assert(inputsMemorySpan[highDataOffset + i] >= inputsMemorySpan[closeDataOffset + i]);

            Trace.Assert(inputsMemorySpan[lowDataOffset + i] <= inputsMemorySpan[openDataOffset + i]);
            Trace.Assert(inputsMemorySpan[lowDataOffset + i] <= inputsMemorySpan[closeDataOffset + i]);
        }

        /* This is a hack, since ta obv uses volume[0] as starting value and ti obv uses 0 as starting value. */
        inputsMemorySpan[volumeDataOffset] = 0;
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

            PrepareInputs(indicator, i, _tulipInputs, _tulipOptions, _tulipOutputs);
        }
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

            PrepareInputs(function, i, _talibInputs, _talibOptions, _talibOutputs);
        }
    }

    private void PrepareInputs(
        dynamic indicator,
        int indicatorIndex,
        Dictionary<int, double[][]> inputsDic,
        Dictionary<int, double[]> optionsDic,
        Dictionary<int, double[][]> outputsDic)
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