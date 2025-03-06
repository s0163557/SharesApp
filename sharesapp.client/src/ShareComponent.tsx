import { useState, useEffect } from 'react';
import { useParams } from "react-router";
import ReactApexChart from "react-apexcharts";
import { ApexOptions } from "apexcharts";

function ShareComponent() {

    const params = useParams();
    const [security, setSecurity] = useState<Security>();

    const [series, setSeries] = useState({
        data: [{
            x: new Date(1538778600000),
            y: [6629.81, 6650.5, 6623.04, 6633.33]
        },
        {
            x: new Date(1538780400000),
            y: [6632.01, 6643.59, 6620, 6630.11]
        },
        ]
    });

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
            <div>
                <div id="chart">
                    <ReactApexChart options={options} series={[series]} type="candlestick" height={350} />
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