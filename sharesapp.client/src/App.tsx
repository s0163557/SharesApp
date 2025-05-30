import { useState } from 'react';
import './App.css';

interface Security {
    securityInfoId: number;
    securityId: string;
    name: string;
    isin: string;
}

function App() {
    const [activeSecurities, setActiveSecurities] = useState<Security[]>();
    const [inactiveSecurities, setInactiveSecurities] = useState<Security[]>();

    const activeSecuritiesContents = activeSecurities === undefined
        ? <button onClick={fetchActiveSecuritiesData}>Fetch Securities</button>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Short Name</th>
                    <th>Long Name</th>
                    <th>ISIN</th>
                </tr>
            </thead>
            <tbody>
                {activeSecurities.map(security =>
                    <tr key={security.securityInfoId}>
                        <td>
                            {security.securityInfoId}
                        </td>
                        <td>
                            <a href={"/shares/" + security.securityId}>{security.securityId}</a>
                        </td>
                        <td>
                            {security.name}
                        </td>
                        <td>
                            {security.isin}
                        </td>
                    </tr>
                )}
            </tbody>
        </table>;

    const inactiveSecuritiesContents = inactiveSecurities === undefined
        ? <button onClick={fetchInactiveSecuritiesData}>Fetch Securities</button>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Short Name</th>
                    <th>Long Name</th>
                    <th>ISIN</th>
                </tr>
            </thead>
            <tbody>
                {inactiveSecurities.map(security =>
                    <tr key={security.securityInfoId}>
                        <td>
                            {security.securityInfoId}
                        </td>
                        <td>
                            <a href={"/shares/" + security.securityId}>{security.securityId}</a>
                        </td>
                        <td>
                            {security.name}
                        </td>
                        <td>
                            {security.isin}
                        </td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tableLabel">Table of shares traded in last 3 month</h1>
            {activeSecuritiesContents}

            <h1 id="tableLabel">Table of shares without trades in last 3 month</h1>
            {inactiveSecuritiesContents}
        </div>
    );

    async function fetchActiveSecuritiesData() {
        const response = await fetch("/api/Security/GetActiveSecurities");
        const data = await response.json();
        setActiveSecurities(data);
    }

    async function fetchInactiveSecuritiesData() {
        const response = await fetch("/api/Security/GetInactiveSecurities");
        const data = await response.json();
        setInactiveSecurities(data);
    }
}

export default App;