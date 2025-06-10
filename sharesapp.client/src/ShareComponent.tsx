import { useState, useEffect } from 'react';
import { useParams } from "react-router";
import ReactApexChart from "react-apexcharts";
import { ApexOptions } from "apexcharts";

function ShareComponent() {

    const params = useParams();
    const [security, setSecurity] = useState<Security>();

    const [series, setSeries] = useState<Candlestics>();
    const [dividends, setDividends] = useState<Dividend>();

    interface Dividend {
        data:
        {
            x: Date,
            y: number
        }[]
    }

    interface Candlestics {
        data: {
            x: Date,
            y: number[]
        }[]
    }

    const toolTipBox = (y: number[]) => {
        const content = document.createElement('div');

        content.innerHTML =
            '<span style="float:left;">High:</span>' + '<span style="float:right">' + y[0] + "</span><br>" +
            '<span style="float:left;">Open:</span>' + '<span style="float:right">' + y[1] + "</span><br>" +
            '<span style="float:left;">Close:</span>' + '<span style="float:right">' + y[2] + "</span><br>" +
            '<span style="float:left;">Low:</span>' + '<span style="float:right">' + y[3] + "</span><br>" +
            '<span style="text-align:left;">Volume:</span>' + '<span style="float:right">' + y[4] + "</span><br>";

        return content;

    }

    const ApexChart = () => {

        const options: ApexOptions = {
            chart: {
                type: 'candlestick',
                height: 350
            },
            title: {
                text: 'CandleStick Chart',
                align: 'left'
            },
            xaxis: {
                type: 'datetime'
            },
            yaxis: {
                tooltip: {
                    enabled: true
                }
            },
            tooltip: {
                custom: function (opts) {
                    const y =
                        opts.ctx.w.config.series[opts.seriesIndex].data[opts.dataPointIndex]
                            .y;
                    return (
                        toolTipBox(y)
                    );
                },
            },
        };

        return (
            series == undefined ?
                <div>
                    Loading chart
                </div>
                :
                <div>
                    <div id="chart">
                        <ReactApexChart options={options}
                            series={[series]}
                            type="candlestick"
                            height={350}
                            width={800} />
                        <button onClick={GetTradeRecordsInDays}>Month</button>
                        <button onClick={GetTradeRecordsInWeeks}>Year</button>
                        <button onClick={GetTradeRecordsInMonths}>All Records</button>
                    </div>
                    <div id="html-dist"></div>
                </div>
        );
    }

    const DividendChart = () => {

        const options: ApexOptions = {
            chart: {
                type: 'scatter',
                height: 350,
                zoom: {
                    type: 'xy',
                },

            },
            title: {
                text: 'Dividends chart',
                align: 'left'
            },
            xaxis: {
                type: 'datetime',
                labels: {
                    formatter: function (value) {
                        return new Date(value).toLocaleDateString();
                    }
                },
                tooltip: {
                    enabled: true
                }
            },
            yaxis: {
                tooltip: {
                    enabled: true
                }
            },
            tooltip: {
                custom: function () {
                    return (
                        document.createElement('div')
                    );
                },
            }
        };

        return (
            dividends == undefined ?
                <div>
                    Loading chart
                </div>
                :
                <div>
                    <div id="chart">
                        <ReactApexChart
                            options={options}
                            series={[dividends]}
                            type="scatter"
                            height={350}
                            width={800} />
                    </div>
                    <div id="html-dist"></div>
                </div>
        );
    }

    useEffect(() => {
        GetSecurity();
        GetDividends();
        GetTradeRecordsInDays();
    }, []);

    interface Security {
        securityInfoId: number;
        securityId: string;
        name: string;
        isin: string;
        issueSize: number;
        issueDate: Date;
        listLevel: number;
    }

    const textWithMargins = (nameOfObject: string, valueOfobject: string) => {
        return (
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr' }}>
                <div style={{
                    textAlign: 'left',
                    fontWeight: 'bold'
                }}>
                    {nameOfObject}
                </div>

                <div style={{
                    textAlign: 'right'
                }}>
                    {valueOfobject}
                </div>
            </div>
        )
    }

    const tradeRecordsChart =
        <ApexChart />

    const dividendsChart =
        <DividendChart />

    const securityContent = security == undefined ?
        <div>
            Fetching data
        </div>
        :
        <div>

            <h1>
                {security.name}
            </h1>

            {tradeRecordsChart}
            {dividendsChart}


            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gridTemplateRows: '1fr 1fr 1fr', columnGap: 50, rowGap: 10 }}>

                {textWithMargins('Share name', security.securityId)}
                {textWithMargins('Share API ID', security.securityInfoId.toString())}
                {textWithMargins('Share issue date', security.issueDate.toString())}
                {textWithMargins('Share ISIN code', security.isin.toString())}

            </div>
        </div>

    return (
        <div>
            {securityContent}
        </div>
    );

    async function GetTradeRecordsInMonths() {
        setSeries(undefined);
        const response = await fetch('/api/Security/GetSecurityTradeRecordsByMonth/' + params.secid);
        const data = await response.json();
        const correctformat = { data: data }
        setSeries(correctformat);
    }

    async function GetTradeRecordsInWeeks() {
        setSeries(undefined);
        const response = await fetch('/api/Security/GetSecurityTradeRecordsByWeek/' + params.secid);
        const data = await response.json();
        const correctformat = { data: data }
        setSeries(correctformat);
    }

    async function GetTradeRecordsInDays() {
        setSeries(undefined);
        const response = await fetch('/api/Security/GetSecurityTradeRecordsByDay/' + params.secid);
        const data = await response.json();
        const correctformat = { data: data }
        setSeries(correctformat);
    }

    async function GetSecurity() {
        const response = await fetch('/api/Security/GetSecurity/' + params.secid);
        const data = await response.json();
        setSecurity(data);
    }

    async function GetDividends() {
        const response = await fetch('/api/Security/GetSecurityDividends/' + params.secid);
        const data = await response.json();
        const correctformat = { data: data }
        setDividends(correctformat);
    }
}

export default ShareComponent;