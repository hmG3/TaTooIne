import type {
    BusinessDay, ChartOptions, CrosshairMode, DeepPartial, IChartApi, ISeriesApi, LineStyle, LineType, PriceLineSource, PriceScaleMode,
    TickMarkType, Time, UTCTimestamp
} from "lib/lightweight-charts/dist/typings";

class SiteJsInterop {
    private chart: IChartApi;
    private chartSeriesMap: Map<string, ISeriesApi<'Candlestick' | 'Histogram' | 'Line' | 'Bar' | 'Area'>>;

    constructor() {
        //window.hljs.initHighlightingOnLoad();
    }

    collapseNavMenu(isOpened: boolean, sender: string) {
        document.body.classList[isOpened ? 'add' : 'remove'](`${sender}--opened`);
    }

    setupChart(element: HTMLElement) {
        this.chart = LightweightCharts.createChart(element,
            {
                width: 750,
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

        this.chartSeriesMap = new Map<string, ISeriesApi<'Candlestick' | 'Histogram' | 'Line' | 'Bar' | 'Area'>>();
    }

    setChartData(title: string, { candle, overlayLines, volume, valueLines }) {
        this.chartSeriesMap.forEach((v, k, m) => {
            this.chart.removeSeries(v);
            m.delete(k);
        });

        this.chart.applyOptions({
            watermark: {
                text: title.toUpperCase()
            }
        });

        if (candle && Array.isArray(candle)) {
            const candleSeries = this.chart.addCandlestickSeries({
                priceScaleId: 'left',
                title: 'AAPL'
            });
            candleSeries.setData(candle);
            this.chartSeriesMap.set('candle', candleSeries);
        }

        if (volume && volume.length) {
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
            volumeSeries.setData(volume);
            this.chartSeriesMap.set('volume', volumeSeries);
        }

        if (overlayLines && Array.isArray(overlayLines)) {
            for (let i = 0; i < overlayLines.length; i++) {
                const overlayLineSeries = this.chart.addLineSeries({
                    priceScaleId: 'right',
                    lineWidth: 1
                });
                overlayLineSeries.setData(overlayLines[i]);
                this.chartSeriesMap.set(`overlayLine${i}`, overlayLineSeries);
            }
        }

        if (valueLines && Array.isArray(valueLines)) {
            for (let i = 0; i < valueLines.length; i++) {
                const valueLineSeries = this.chart.addLineSeries({
                    priceScaleId: '',
                    scaleMargins: {
                        top: 0.75,
                        bottom: 0
                    },
                    lineWidth: 1
                });
                valueLineSeries.setData(valueLines[i]);
                this.chartSeriesMap.set(`valueLine${i}`, valueLineSeries);
            }
        }

        this.chart.timeScale().fitContent();
    }

    static load(): void {
        window[SiteJsInterop.name] = new SiteJsInterop();
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
