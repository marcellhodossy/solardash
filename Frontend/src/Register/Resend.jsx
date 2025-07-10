import { useEffect, useState } from "react";
import React from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

function Resend() {

    const navigate = useNavigate();
    const [status, setStatus] = useState('');


    const handleSubmit = async (e) => {

        if(localStorage.getItem("email") != null) {
            const email = localStorage.getItem("email")

            const response = await axios.post("http://localhost:5030/api/register/resend", {
                email
            });
            localStorage.removeItem("email");

            var error = response.data.error;

            if(error == 12) {
                setStatus("A confirmation link has been sent to your email address.");
            } else if(error = 11) {
                setStatus("The email format is incorrect.");
            } else {
                setStatus("Please activate your account using the link sent to your email address.");
            }
        } else {
            navigate('/');
        }
    }

    useEffect(() => {
        
        const trySubmit = async () => {
        await handleSubmit();
        }

        trySubmit();

    }, []);

    return (

        <h1>{status}</h1>

    );
}

export default Resend;