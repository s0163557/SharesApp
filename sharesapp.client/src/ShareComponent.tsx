import { useState, useEffect } from 'react';
import { useParams } from "react-router";

function ShareComponent() {

    const params = useParams();
    const [security, setSecurity] = useState<Security>();

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
        </div>

    return (
        securityContent
    );

    async function GetSecurity()
    {
        const response = await fetch('/api/Security/GetSecurity/' + params.secid);
        const data = await response.json();
        setSecurity(data);
    }
}

export default ShareComponent;