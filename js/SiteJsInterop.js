class SiteJsInterop {
    constructor() {
    }
    static load() {
        window[SiteJsInterop.name] = new SiteJsInterop();
    }
    collapseNavMenu(isOpened, sender) {
        document.body.classList[isOpened ? 'add' : 'remove'](`${sender}--opened`);
    }
    setupChart(element) {
        this.chart = LightweightCharts.createChart(element, {
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
        let resizeTimerId;
        document.body.onresize = () => {
            if (resizeTimerId) {
                clearTimeout(resizeTimerId);
            }
            resizeTimerId = setTimeout(() => this.resizeChart(), 100);
        };
        this.chartSeriesMap = new Map();
    }
    setChartData(title, data) {
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
                color: 'rgba(76, 175, 80, 0.5)',
                priceLineVisible: false,
                priceScaleId: 'left'
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
    }
    resizeChart() {
        const chartContainer = document.getElementById('chart');
        this.chart.resize(chartContainer.offsetWidth, chartContainer.offsetHeight);
        this.chart.timeScale().fitContent();
    }
}
SiteJsInterop.load();
