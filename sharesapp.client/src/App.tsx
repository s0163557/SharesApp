import { useState } from 'react';
import './App.css';

interface Security {
    securityInfoId: number;
    securityId: string;
}

function App() {
    const [securities, setSecurities] = useState<Security[]>();

    const securitiesContents = securities === undefined
        ? <button onClick={fetchSecuritiesData}>Fetch Securities</button>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Short Name</th>
                </tr>
            </thead>
            <tbody>
                {securities.map(security =>
                    <tr key={security.securityInfoId}>
                        <td>
                            {security.securityInfoId}
                        </td>
                        <td>
                            <a href={"/shares/" + security.securityId}>{security.securityId}</a>
                        </td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tableLabel">Table of shares</h1>
            <div>CI/CD in its beauty</div>
            {securitiesContents}
        </div>
    );

    async function fetchSecuritiesData() {
        const response = await fetch("/api/Security/GetSecurities");
        const data = await response.json();
        setSecurities(data);
    }

}

export default App;