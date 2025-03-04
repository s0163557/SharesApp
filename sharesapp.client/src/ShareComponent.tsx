import { useState, useEffect } from 'react';
import { useParams } from "react-router";
import ReactApexChart from "react-apexcharts";

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

        return (
            <div>
                <div id="chart">
                    <ReactApexChart series={[series]} type="candlestick" height={350} />
                </div>
                <div id="html-dist"></div>
            </div>
        );
    }

    useEffect(() => {
        GetSecurity();
        FetchSecurityTradeRecords();
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

    async function FetchSecurityTradeRecords() {
        const response = await fetch('/api/Security/GetSecurityTradeRecords/' + params.secid);
        const data = await response.json();
        
        setSeries(data);
    }

    async function GetSecurity() {
        const response = await fetch('/api/Security/GetSecurity/' + params.secid);
        const data = await response.json();
        setSecurity(data);
    }
}

export default ShareComponent;