import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import ShareComponent from './ShareComponent.tsx'

createRoot(document.getElementById('root')!).render(
    <BrowserRouter>
        <Routes>
            <Route path="/shares" element={<App />} />
            <Route path="/shares/:secid" element={<ShareComponent />} />
        </Routes>
    </BrowserRouter>,
)
