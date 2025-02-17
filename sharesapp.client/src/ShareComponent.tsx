import { useParams } from "react-router";

function ShareComponent() {

    const params = useParams();


    interface Security {
        SecurityInfoId: number;
        SecurityId: string;
        Name: string;
        Isin: string;
        IssueSize: number;
        IssueDate: Date;
        ListLevel: number;
    }

    return (
        <div>
            Component name - {params.secid}
        </div>
    );

}

export default ShareComponent;