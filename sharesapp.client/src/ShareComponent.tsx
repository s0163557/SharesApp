import { useState, useEffect } from 'react';
import { useParams } from "react-router";
import ReactApexChart from "react-apexcharts";
import { ApexOptions } from "apexcharts";

function ShareComponent() {

    const params = useParams();
    const [security, setSecurity] = useState<Security>();

    const [series, setSeries] = useState<Candlestics>();

    interface Candlestics
    {
        data: {
            x: Date,
            y: number[]
        } []
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

        const chart = series == undefined ?
        <div>
            Loading chart
        </div>
        :
            <div>

                <div id="chart">
                    <ReactApexChart options={options} series={[series]} type="candlestick" height={350} />
                </div>
                <div id="html-dist"></div>
            </div>

        return (
            chart
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

    const securityContent = security == undefined ?
        <div>
            Fetching data
        </div>
        :
        <div>
            Component name - {security.securityId}
            Component ID - {security.securityInfoId}
            component issue date - {security.issueDate.toString()}

            
            <ApexChart />
           
        </div>

    return (
        securityContent
    );

    async function GetSecurity() {
        let response = await fetch('/api/Security/GetSecurity/' + params.secid);
        let data = await response.json();
        setSecurity(data);

        response = await fetch('/api/Security/GetSecurityTradeRecords/' + params.secid);
        data = await response.json();
        const correctformat = {data:data}
        setSeries(correctformat);
    }
}

export default ShareComponent;