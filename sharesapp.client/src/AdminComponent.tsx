import { useState, useEffect } from 'react';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

function AdminComponent() {
    const [fromDate, setFromDate] = useState<Dayjs>();
    const [toDate, setToDate] = useState<Dayjs>();

    useEffect(() => {
        setFromDate(dayjs("2010-11-01"))
        setToDate(dayjs("2025-01-01"))
    }, []);

    return (
        <div>
            <p>Collect trade data</p>

            <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DatePicker
                    label="Start date"
                    format="YYYY-MM-DD"
                    value={fromDate}
                    onChange={(newFromDate) => setFromDate(dayjs(newFromDate))} />
            </LocalizationProvider>

            <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DatePicker
                    label="End date"
                    format="YYYY-MM-DD"
                    value={toDate}
                    onChange={(newToDate) => setToDate(dayjs(newToDate))} />
            </LocalizationProvider>
            <button onClick={gatherAndSendData}>Fetch Securities</button>
        </div>
    );

    async function gatherAndSendData() {
        const params = new URLSearchParams();

        if (fromDate != undefined)
            params.append("from", fromDate.format("YYYY-MM-DD").toString());

        if (toDate != undefined)
            params.append("to", toDate.format("YYYY-MM-DD").toString());

        const apiUrl = '/api/Moex/GetTradeRecordsInRangeOfDates?' + params;
        const response = await fetch(apiUrl);
        return response;
    }
}

export default AdminComponent;