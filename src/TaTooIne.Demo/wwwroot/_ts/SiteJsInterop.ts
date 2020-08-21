import type {
    BarData, BusinessDay, ChartOptions, CrosshairMode, DeepPartial, HistogramData, IChartApi, ISeriesApi, LineData, LineStyle, LineType,
    PriceLineSource, PriceScaleMode, TickMarkType, Time, UTCTimestamp
} from './lib/lightweight-charts/dist/typings';

class SiteJsInterop {
    private chart: IChartApi;
    private chartSeriesMap: Map<string, ISeriesApi<'Candlestick' | 'Histogram' | 'Line' | 'Bar' | 'Area'>>;

    constructor() {
        //window.hljs.initHighlightingOnLoad();
    }

    static load(): void {
        window[SiteJsInterop.name] = new SiteJsInterop();
    }

    collapseNavMenu(isOpened: boolean, sender: string) {
        document.body.classList[isOpened ? 'add' : 'remove'](`${sender}--opened`);
    }

    setupChart(element: HTMLElement) {
        this.chart = LightweightCharts.createChart(element,
            {
                height: 400,
                grid: {
                    vertLines: {
                        color: 'rgba(197, 203, 206, 0.5)'
                    },
                    horzLines: {
                        color: 'rgba(197, 203, 206, 0.5)'
                    }
                },
                crosshair: {
                    mode: LightweightCharts.CrosshairMode.Normal
                },
                rightPriceScale: {
                    visible: true,
                    borderColor: 'rgba(197, 203, 206, 1)'
                },
                leftPriceScale: {
                    visible: true,
                    borderColor: 'rgba(197, 203, 206, 1)'
                },
                timeScale: {
                    borderColor: 'rgba(197, 203, 206, 0.8)'
                },
                watermark: {
                    visible: true,
                    fontSize: 24,
                    horzAlign: 'center',
                    vertAlign: 'center',
                    color: 'rgba(171, 71, 188, 0.5)'
                }
            });

        let resizeTimerId: number;
        document.body.onresize = () => {
            if (resizeTimerId) {
                clearTimeout(resizeTimerId);
            }
            resizeTimerId = setTimeout(() => this.resizeChart(), 100);
        }

        this.chartSeriesMap = new Map<string, ISeriesApi<'Candlestick' | 'Histogram' | 'Line' | 'Bar' | 'Area'>>();
    }

    setChartData(title: string, data:
        { candle: Array<BarData>, overlayLines: Array<Array<LineData>>, volume: Array<HistogramData>, valueLines: Array<Array<LineData>> }) {
        this.chartSeriesMap.forEach((v, k, m) => {
            this.chart.removeSeries(v);
            m.delete(k);
        });

        this.chart.applyOptions({
            watermark: {
                text: title.toUpperCase()
            }
        });

        if (data.candle && Array.isArray(data.candle)) {
            const candleSeries = this.chart.addCandlestickSeries({
                priceScaleId: 'left',
                title: 'AAPL'
            });
            candleSeries.setData(data.candle);
            this.chartSeriesMap.set('candle', candleSeries);
        }

        if (data.volume && data.volume.length) {
            const volumeSeries = this.chart.addHistogramSeries({
                priceFormat: {
                    type: 'volume'
                },
                priceLineVisible: false,
                color: 'rgba(76, 175, 80, 0.5)',
                priceScaleId: '',
                scaleMargins: {
                    top: 0.85,
                    bottom: 0
                }
            });
            volumeSeries.setData(data.volume);
            this.chartSeriesMap.set('volume', volumeSeries);
        }

        if (data.overlayLines && Array.isArray(data.overlayLines)) {
            for (let i = 0; i < data.overlayLines.length; i++) {
                const overlayLineSeries = this.chart.addLineSeries({
                    priceScaleId: 'right',
                    lineWidth: 1
                });
                overlayLineSeries.setData(data.overlayLines[i]);
                this.chartSeriesMap.set(`overlayLine${i}`, overlayLineSeries);
            }
        }

        if (data.valueLines && Array.isArray(data.valueLines)) {
            for (let i = 0; i < data.valueLines.length; i++) {
                const valueLineSeries = this.chart.addLineSeries({
                    priceScaleId: '',
                    scaleMargins: {
                        top: 0.75,
                        bottom: 0
                    },
                    lineWidth: 1
                });
                valueLineSeries.setData(data.valueLines[i]);
                this.chartSeriesMap.set(`valueLine${i}`, valueLineSeries);
            }
        }

        window.dispatchEvent(new UIEvent('resize'));
        this.chart.timeScale().fitContent();
    }

    private resizeChart() {
        const chartContainer = document.getElementById('chart');
        this.chart.resize(chartContainer.offsetWidth, chartContainer.offsetHeight);
    }
}

SiteJsInterop.load();

declare class LightweightChartsModule {
    get LineStyle(): { [_ in keyof typeof LineStyle]: number };
    get LineType(): { [_ in keyof typeof LineType]: number };
    get CrosshairMode(): { [_ in keyof typeof CrosshairMode]: number };
    get PriceScaleMode(): { [_ in keyof typeof PriceScaleMode]: number };
    get PriceLineSource(): { [_ in keyof typeof PriceLineSource]: number };
    get TickMarkType(): { [_ in keyof typeof TickMarkType]: number };
    isBusinessDay(time: Time): time is BusinessDay;
    isUTCTimestamp(time: Time): time is UTCTimestamp;
    createChart(container: string | HTMLElement, options?: DeepPartial<ChartOptions>): IChartApi;
}

declare var LightweightCharts: LightweightChartsModule;
