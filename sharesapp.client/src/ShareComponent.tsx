import { useState, useEffect } from 'react';
import { useParams } from "react-router";
import ReactApexChart from "react-apexcharts";
import { ApexOptions } from "apexcharts";

function ShareComponent() {

    const params = useParams();
    const [security, setSecurity] = useState<Security>();

    const [series, setSeries] = useState<Candlestics>();

    interface Candlestics {
        data: {
            x: Date,
            y: number[]
        }[]
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
            }
        };

        return (
            series == undefined ?
                <div>
                    Loading chart
                </div>
                :
                <div>
                    <div id="chart" >
                        <ReactApexChart options={options} series={[series]} type="candlestick" height={350} width={800} />
                        <button>Week</button>
                        <button>Month</button>
                        <button>Year</button>
                    </div>
                    <div id="html-dist"></div>
                </div>
        );
    }

    useEffect(() => {
        GetSecurity();
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

    const chart =
        <ApexChart />

    const securityContent = security == undefined ?
        <div>
            Fetching data
        </div>
        :
        <div>

            <h1>
                {security.name}
            </h1>

            {chart}


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

    async function GetSecurity() {
        let response = await fetch('/api/Security/GetSecurity/' + params.secid);
        let data = await response.json();
        setSecurity(data);

        response = await fetch('/api/Security/GetSecurityTradeRecords/' + params.secid);
        data = await response.json();
        const correctformat = { data: data }
        setSeries(correctformat);
    }
}

export default ShareComponent;