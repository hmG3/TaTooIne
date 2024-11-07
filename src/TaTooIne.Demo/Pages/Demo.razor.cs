using System.Linq.Expressions;
using System.Text;
using Microsoft.JSInterop;
using TALib;
using TaTooIne.Demo.Models;
using TinyCsvParser;

namespace TaTooIne.Demo.Pages;

public partial class Demo
{
    private ICollection<Candle> _sampleData = null!;
    private CalculatedState _state = null!;

    protected override async Task OnInitializedAsync()
    {
        var csvParser = new CsvParser<Candle>(new CsvParserOptions(true, ';', 1, true), new CandleCsvMapping());

        var fileStream = await Http.GetStreamAsync("sample-data/AAPL.csv");
        var fileRecords = csvParser.ReadFromStream(fileStream, Encoding.UTF8);

        _sampleData = fileRecords.Select(x => x.Result).ToList();
    }

    private async Task CalculateExample(Function function)
    {
        var functionInputs = new double[function.Inputs.Length][];
        var functionInputNames = new string[function.Inputs.Length];
        var functionOutputNames = new string[function.Outputs.Length];
        for (var i = 0; i < function.Inputs.Length; i++)
        {
            functionInputNames[i] = function.Inputs[i] == "Real"
                ? i == 0 ? nameof(Candle.Close) : nameof(Candle.Open)
                : function.Inputs[i];
        }

        for (var i = 0; i < function.Outputs.Length; i++)
        {
            functionOutputNames[i] = function.Outputs[i] == "Real" ? function.Name : function.Outputs[i];
        }

        var inputLines = new Dictionary<string, Line[]>();
        for (var i = 0; i < functionInputNames.Length; i++)
        {
            var c = Expression.Parameter(typeof(Candle), "c");
            var timeBinding = Expression.Bind(
                typeof(Line).GetProperty(nameof(Line.Time))!,
                Expression.Property(c, nameof(Candle.Time))
            );
            var valueBinding = Expression.Bind(
                typeof(Line).GetProperty(nameof(Line.Value))!,
                Expression.Property(c, functionInputNames[i])
            );
            var lineClassInit = Expression.MemberInit(Expression.New(typeof(Line)), timeBinding, valueBinding);
            var lineLambda = Expression.Lambda<Func<Candle, Line>>(lineClassInit, c);

            inputLines[functionInputNames[i]] = _sampleData.Select(lineLambda.Compile()).ToArray();
            functionInputs[i] = inputLines[functionInputNames[i]].Select(l => l.Value).ToArray();
        }

        var inputOffset = function.Lookback();

        var outputLength = functionInputs[0].Length - inputOffset;
        var functionOutputs = new double[function.Outputs.Length][];
        for (var i = 0; i < functionOutputs.Length; i++)
        {
            functionOutputs[i] = new double[outputLength];
        }

        function.Run(functionInputs, Array.Empty<double>(), functionOutputs);

        var outputLines = functionOutputs.Zip(Enumerable.Repeat(
                    inputLines.Values.ElementAt(0).Skip(inputOffset).ToList(),
                    functionOutputs.Length),
                (lines, doubles) => doubles.Zip(lines, (l, d) => l with { Value = d }).ToList())
            .ToList();

        var outputLineValues = outputLines.Select(lines => Enumerable.Repeat<Line?>(null, inputOffset).Concat(lines).ToList()).ToList();

        _state = new CalculatedState
        {
            FuncDescription = function.Description,
            InputNames = inputLines.Keys,
            OutputNames = functionOutputNames,
            InputValues = inputLines.Values.ToList(),
            OutputValues = outputLineValues
        };

        object data = function.Group switch
        {
            "Overlap Studies" => new
            {
                Candle = _sampleData,
                OverlayLines = outputLines
            },
            "Volume Indicators" => new
            {
                Volume = _sampleData.Select(c => new Line(c.Time, c.Volume)).ToList(),
                OverlayLines = outputLines
            },
            _ => new
            {
                Candle = _sampleData,
                ValueLines = outputLines
            }
        };

        await JsRuntime.InvokeVoidAsync("App.setChartData", function.Name, data);
    }
}
